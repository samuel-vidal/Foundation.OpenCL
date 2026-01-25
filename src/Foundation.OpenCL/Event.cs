using System;
using System.Collections.Generic;
using System.Collections.Immutable;
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

    public sealed unsafe class Event(Handle<Event> handle)
        : InformationNode<Event, EventInfo>(handle), IReify<Event>
    {
        public static void Wait(params ReadOnlySpan<Event> events)
        {
            var handles = stackalloc Handle<Event>[events.Length];
            for (var i = 0; i < events.Length; i++) handles[i] = events[i].Handle;
            OpenCLNative.WaitForEvents(events.Length, handles).ThrowIfUnsuccessful();
        }

        public void Wait() => Wait(this);

        public void SetEvenStatus(CommandExecutionStatus status)
            => OpenCLNative.SetUserEventStatus(Handle, status).ThrowIfUnsuccessful();

        #region Native Single Cast Callback Mechanism

        public void SetEventCallback(CommandExecutionStatus status, Action callback)
        {
            void Hook(Handle<Event> handle, CommandExecutionStatus _, void* __)
            {
                try { callback(); } catch { }
            }

            OpenCLNative.SetEventCallback(Handle, status, (void*)Marshal.GetFunctionPointerForDelegate(Hook), null)
                .ThrowIfUnsuccessful();

            // this doesn't happen if there is an exception above:

            var managed = GCHandle.Alloc(Hook);
            OnDispose += () => managed.Free();
        }

        #endregion

        #region Opt-in Multicast Callback Mechanism

        private static readonly object mutex = new();
        private static readonly Dictionary<(Handle<Event>, CommandExecutionStatus), ImmutableHashSet<Event>> callbacks = new();

        private Action? onComplete;
        private Action? onRunning;
        private Action? onSubmitted;
        private Action? onQueued;

        public event Action OnComplete
        {
            add => AddCallback(CommandExecutionStatus.Complete, value, ref onComplete);
            remove => RemoveCallback(CommandExecutionStatus.Complete, value, ref onComplete);
        }

        public event Action OnRunning
        {
            add => AddCallback(CommandExecutionStatus.Running, value, ref onRunning);
            remove => RemoveCallback(CommandExecutionStatus.Running, value, ref onRunning);
        }

        public event Action OnSubmitted
        {
            add => AddCallback(CommandExecutionStatus.Submitted, value, ref onSubmitted);
            remove => RemoveCallback(CommandExecutionStatus.Submitted, value, ref onSubmitted);
        }

        public event Action OnQueued
        {
            add => AddCallback(CommandExecutionStatus.Queued, value, ref onQueued);
            remove => RemoveCallback(CommandExecutionStatus.Queued, value, ref onQueued);
        }

        private void AddCallback(CommandExecutionStatus status, Action callback, ref Action? targets)
        {
            lock (mutex)
            {
                if (Handle.Value == 0) return;

                var original = targets;
                targets += callback;

                if (original == null)
                {
                    if (!callbacks.TryGetValue((Handle, status), out var list))
                    {
                        callbacks[(Handle, status)] = ImmutableHashSet<Event>.Empty.Add(this);

                        // needs to install native callback here
                        InstallNativeCallback(status);
                    }
                    else
                    {
                        callbacks[(Handle, status)] = list.Add(this);
                    }
                }
            }
        }

        private void RemoveCallback(CommandExecutionStatus status, Action callback, ref Action? targets)
        {
            lock (mutex)
            {
                targets -= callback;
                if (targets == null)
                {
                    RemoveCallbackAux(status);
                }
            }
        }

        private void RemoveCallbackAux(CommandExecutionStatus status)
        {
            if (!callbacks.TryGetValue((Handle, status), out var list)) return;

            list = list.Remove(this);
            if (list.IsEmpty)
            {
                callbacks.Remove((Handle, status));

                // needs to de-install native callback here
                UninstallNativeCallback(status);
            }
            else
            {
                callbacks[(Handle, status)] = list;
            }
        }

        private void InstallNativeCallback(CommandExecutionStatus status)
        {
            OpenCLNative.SetEventCallback(Handle, status, (void*)Marshal.GetFunctionPointerForDelegate(OnCallback), null)
                .ThrowIfUnsuccessful();
        }

        private void UninstallNativeCallback(CommandExecutionStatus status)
        {
            OpenCLNative.SetEventCallback(Handle, status, null, null)
                .ThrowIfUnsuccessful();
        }

        private static void OnCallback(Handle<Event> handle, CommandExecutionStatus status, void* _)
        {
            ImmutableHashSet<Event>? existing;
            lock (mutex) if (!callbacks.TryGetValue((handle, status), out existing)) return;
            foreach (var obj in existing)
            {
                try
                {
                    var multicast = status switch
                    {
                        CommandExecutionStatus.Complete => obj.onComplete,
                        CommandExecutionStatus.Queued => obj.onQueued,
                        CommandExecutionStatus.Running => obj.onRunning,
                        CommandExecutionStatus.Submitted => obj.onSubmitted,
                        _ => throw new NotSupportedException()
                    };
                    if (multicast != null) multicast();
                }
                catch
                {

                }
            }
        }

        #endregion

        protected override void RetainHook() => OpenCLNative.RetainEvent(Handle).ThrowIfUnsuccessful();

        protected override void ReleaseHook(Handle<Event> tmpHandle)
        {
            lock (mutex)
            {
                onComplete = null;
                RemoveCallbackAux(CommandExecutionStatus.Complete);
                onQueued = null;
                RemoveCallbackAux(CommandExecutionStatus.Queued);
                onRunning = null;
                RemoveCallbackAux(CommandExecutionStatus.Running);
                onSubmitted = null;
                RemoveCallbackAux(CommandExecutionStatus.Submitted);
            }

            OpenCLNative.ReleaseEvent(tmpHandle).ThrowIfUnsuccessful();
        }

        protected override void GetInfo(EventInfo paramName, nuint paramValueSize, void* paramValue, out nuint paramValueSizeRet)
             => OpenCLNative.GetEventInfo(Handle, paramName, paramValueSize, paramValue, out paramValueSizeRet).ThrowIfUnsuccessful();

        public static Event Reify(Handle<Event> handle) => new(handle);
    }
}
