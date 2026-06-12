using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace Foundation.OpenCL
{
    #region Constants

    public enum EventInfo
    {
        CommandQueue = 0x11D0,                         // CL_EVENT_COMMAND_QUEUE
        CommandType = 0x11D1,                          // CL_EVENT_COMMAND_TYPE
        ReferenceCount = 0x11D2,                       // CL_EVENT_REFERENCE_COUNT
        CommandExecutionStatus = 0x11D3,               // CL_EVENT_COMMAND_EXECUTION_STATUS
        Context = 0x11D4,                              // CL_EVENT_CONTEXT
        CommandTerminationReasonArm = 0x41ED,          // CL_EVENT_COMMAND_TERMINATION_REASON_ARM
    }

    public enum CommandExecutionStatus
    {
        Complete = 0x0,           // CL_COMPLETE
        Running = 0x1,            // CL_RUNNING
        Submitted = 0x2,          // CL_SUBMITTED
        Queued = 0x3,             // CL_QUEUED
    }

    #endregion

    public sealed unsafe class Event : InformationNode<Event, EventInfo>, IReify<Event>
    {
        private Event(Handle<Event> handle) : base(handle) { }

        public static void Wait(params ReadOnlySpan<Event> events)
        {
            var handles = stackalloc Handle<Event>[events.Length];
            for (var i = 0; i < events.Length; i++) handles[i] = events[i].Handle;
            OpenCLNative.WaitForEvents(events.Length, handles).ThrowIfUnsuccessful();
        }

        public void Wait() => Wait(this);

        public void SetEventStatus(CommandExecutionStatus status)
            => OpenCLNative.SetUserEventStatus(Handle, status).ThrowIfUnsuccessful();

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate void EventCallback(Handle<Event> handle, CommandExecutionStatus status, void * data); 

        private class EventCallbackClosure : IDisposable
        {
            private Action? callback;
            private GCHandle managed;

            public EventCallback Delegate { get; }

            public EventCallbackClosure(Action callback)
            {
                this.callback = callback;
                managed = GCHandle.Alloc(this);
                Delegate = Invoke;
            }

            private void Invoke(Handle<Event> handle, CommandExecutionStatus status, void* data)
            {
                try { callback(); } catch { } finally { Dispose(); }
            }

            public void Dispose()
            {
                managed.Free();
                callback = null;
            }
        }

        public void SetEventCallback(CommandExecutionStatus status, Action callback)
        {
            var closure = new EventCallbackClosure(callback);

            try
            {
                OpenCLNative.SetEventCallback(Handle, status, (void*)Marshal.GetFunctionPointerForDelegate(closure.Delegate), null)
                    .ThrowIfUnsuccessful();
            }
            catch
            {
                closure.Dispose();
                throw;
            }
        }

        protected override void RetainHook() => OpenCLNative.RetainEvent(Handle).ThrowIfUnsuccessful();

        protected override void ReleaseHook(Handle<Event> tmpHandle)
        {
            OpenCLNative.ReleaseEvent(tmpHandle).ThrowIfUnsuccessful();
        }

        protected override void GetInfo(EventInfo paramName, nuint paramValueSize, void* paramValue, out nuint paramValueSizeRet)
             => OpenCLNative.GetEventInfo(Handle, paramName, paramValueSize, paramValue, out paramValueSizeRet).ThrowIfUnsuccessful();

        private static readonly LinkedList<IDecorator<Event>> decorators = new();

        public static Event Reify(Handle<Event> handle)
        {
            var evt = new Event (handle);

            lock (decorators)
            {
                foreach (var decorator in decorators)
                {
                    evt = decorator.Decorate(evt);
                }
            }

            return evt;
        }

        private class Remove(LinkedListNode<IDecorator<Event>> node) : IDisposable
        {
            public void Dispose()
            {
                lock (decorators) decorators.Remove(node);
            }
        }

        public static IDisposable AddDecorator(IDecorator<Event> decorator)
        {
            lock (decorators)
            {
                var node = decorators.AddLast(decorator);
                return new Remove(node);
            }
        }
    }

    public interface IDecorator<T>
    {
        T Decorate(T item);
    }

    public static class EventExtensions
    {
        public static CommandExecutionStatus GetCommandExecutionStatus(this Event evt)
            => evt.GetInfo<CommandExecutionStatus>(EventInfo.CommandExecutionStatus);

        public static void OnComplete(this Event evt, Action callback)
            => evt.SetEventCallback(CommandExecutionStatus.Complete, callback);

        public static void OnQueued(this Event evt, Action callback)
            => evt.SetEventCallback(CommandExecutionStatus.Queued, callback);

        public static void OnRunning(this Event evt, Action callback)
            => evt.SetEventCallback(CommandExecutionStatus.Running, callback);

        public static void OnSubmitted(this Event evt, Action callback)
            => evt.SetEventCallback(CommandExecutionStatus.Submitted, callback);
    }
}
