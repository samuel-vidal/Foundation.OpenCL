using System;
using System.Threading;

namespace Foundation.OpenCL.Tests.Events
{
    [Category("Hardware")]
    public class EventStatusTests
    {
        [Test]
        public void TestGetSetCompletionStatus()
        {
            var platforms = Platform.GetPlatforms();
            var platform = platforms[0];
            var devices = platform.GetDevices(DeviceType.Gpu);
            var device = devices[0];

            using var context = platform.CreateContext([device]);
            using var evt = context.CreateUserEvent();

            var status = evt.GetCommandExecutionStatus();

            Console.WriteLine(status);

            evt.SetEventStatus(CommandExecutionStatus.Complete);

            var completed = evt.GetCommandExecutionStatus();

            Assert.That(completed, Is.EqualTo(CommandExecutionStatus.Complete));
        }

        [Test]
        public void OnComplete_Fires_All_Subscribers()
        {
            var platforms = Platform.GetPlatforms();
            var platform = platforms[0];
            var devices = platform.GetDevices(DeviceType.Gpu);
            var device = devices[0];

            using var context = platform.CreateContext([device]);
            using var evt = context.CreateUserEvent();

            int count = 0;

            evt.OnComplete(() => Interlocked.Increment(ref count));
            evt.OnComplete(() => Interlocked.Increment(ref count));
            evt.OnComplete(() => Interlocked.Increment(ref count));

            evt.SetEventStatus(CommandExecutionStatus.Complete);

            Thread.Sleep(100);

            Interlocked.MemoryBarrier();

            Assert.That(count, Is.EqualTo(3));
        }

        [Test]
        public void SignalOncePolicy_Handles_Race_With_Already_Completed_Event()
        {
            var platforms = Platform.GetPlatforms();
            var platform = platforms[0];
            var devices = platform.GetDevices(DeviceType.Gpu);
            var device = devices[0];

            var context = platform.CreateContext([device]);

            var userEvent = context.CreateUserEvent();
            var uploadEvent = context.CreateUserEvent();

            uploadEvent.SetEventStatus(CommandExecutionStatus.Complete);

            var mutex = new object();

            var fired = false;

            uploadEvent.OnComplete(()=>
            {
                userEvent.SetEventStatus(CommandExecutionStatus.Complete);

                lock (mutex)
                {
                    fired = true;
                    Monitor.Pulse(mutex);
                }
            });

            lock (mutex)
            {
                while (!fired)
                    Monitor.Wait(mutex, TimeSpan.FromMilliseconds(10));
            }

            // User event should be signaled
            Assert.That(userEvent.GetCommandExecutionStatus(), Is.EqualTo(CommandExecutionStatus.Complete));

            uploadEvent.Dispose();
            userEvent.Dispose();
            context.Dispose();
        }

        [Test]
        public void HardCore()
        {
            for (int i = 0; i < 20; i++)
            {
                SignalOncePolicy_Handles_Race_With_Already_Completed_Event();
            }
        }
    }
}
