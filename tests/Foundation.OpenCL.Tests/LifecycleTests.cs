namespace Foundation.OpenCL.Tests
{
    [Explicit]
    public class LifecycleTests
    {
        [Test]
        public void TestQueueLifecycle()
        {
            var platforms = Platform.GetPlatforms();
            var platform = platforms[0];
            var devices = platform.GetDevices(DeviceType.Gpu);
            var device = devices[0];

            Assert.That(device.GetReferenceCount(), Is.EqualTo(1));

            var context = Context.CreateContext([device]);

            Assert.That(context.GetReferenceCount(), Is.EqualTo(1));

            var queue = context.CreateCommandQueue(device);

            Assert.That(queue.GetReferenceCount(), Is.EqualTo(1));

            var queue2 = queue.Retain();

            Assert.That(queue.GetReferenceCount(), Is.EqualTo(2));

            Assert.That(context.GetReferenceCount(), Is.EqualTo(1));

            queue.Flush();
            queue.Finish();

            context.Dispose();        // Succeeds !!!

            Assert.That(queue.GetReferenceCount(), Is.EqualTo(2));

            queue.Dispose();

            Assert.That(queue2.GetReferenceCount(), Is.EqualTo(1));

            queue2.Dispose();        // Succeeds !!!
        }

        [Test]
        public void TestContextLifecycle()
        {
            var platforms = Platform.GetPlatforms();
            var platform = platforms[0];
            var devices = platform.GetDevices(DeviceType.Gpu);
            var device = devices[0];

            Assert.That(device.GetReferenceCount(), Is.EqualTo(1));

            var context = Context.CreateContext([device]);

            Assert.That(context.GetReferenceCount(), Is.EqualTo(1));

            context.Dispose();        // Succeeds !!!
        }
    }
}
