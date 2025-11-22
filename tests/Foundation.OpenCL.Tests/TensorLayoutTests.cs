using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Foundation.OpenCL.Tests
{
    internal class TensorLayoutTests
    {
        [Test]
        public void BasicContiguousTest()
        {
            // Arrange
            var source = new SubTensorLayout<float>(
                origin: new nuint[] { 0, 0, 0 },
                dimension: new nuint[] { 64, 32, 16 },
                stride: new nuint[] { 1, 64, 2048 }); // column-major

            var target = new SubTensorLayout<float>(
                origin: new nuint[] { 0, 0, 0 },
                dimension: new nuint[] { 64, 32, 16 },
                stride: new nuint[] { 1, 64, 2048 });

            // Act
            Span<nuint> targetOrigin = stackalloc nuint[3];
            Span<nuint> sourceOrigin = stackalloc nuint[3];
            Span<nuint> region = stackalloc nuint[3];
            Span<nuint> targetPitch = stackalloc nuint[3];
            Span<nuint> sourcePitch = stackalloc nuint[3];

            BufferRectangularExtensions.ConvertArguments(
                target, source, targetOrigin, sourceOrigin, region, targetPitch, sourcePitch);

            // Assert
            Assert.That(region[0], Is.EqualTo((nuint)(64 * 32 * 16 * sizeof(float))));
            Assert.That(region[1], Is.EqualTo((nuint)1));
            Assert.That(region[2], Is.EqualTo((nuint)1));

            Assert.That(targetOrigin[0], Is.EqualTo((nuint)0));
            Assert.That(targetOrigin[1], Is.EqualTo((nuint)0));
            Assert.That(targetOrigin[2], Is.EqualTo((nuint)0));

            Assert.That(sourceOrigin[0], Is.EqualTo((nuint)0));
            Assert.That(sourceOrigin[1], Is.EqualTo((nuint)0));
            Assert.That(sourceOrigin[2], Is.EqualTo((nuint)0));

            Assert.That(targetPitch[0], Is.EqualTo((nuint)0));
            Assert.That(targetPitch[1], Is.EqualTo((nuint)0));
            Assert.That(targetPitch[2], Is.EqualTo((nuint)0));

            Assert.That(sourcePitch[0], Is.EqualTo((nuint)0));
            Assert.That(sourcePitch[1], Is.EqualTo((nuint)0));
            Assert.That(sourcePitch[2], Is.EqualTo((nuint)0));
        }

        [Test]
        public void TwoDimensionQuadrantTest()
        {
            // We assume a 1024 x 2048 contiguous tensor cut in four equal quadrants
            // NE NW
            // SE SW

            // Arrange
            // NW quadrant
            var source = new SubTensorLayout<float>(
                origin: new nuint[] { 0, 1024 },
                dimension: new nuint[] { 512, 1024 },
                stride: new nuint[] { 1, 1024 });

            // SE quadrant
            var target = new SubTensorLayout<float>(
                origin: new nuint[] { 512, 0 },
                dimension: new nuint[] { 512, 1024 },
                stride: new nuint[] { 1, 1024 });

            // Act
            Span<nuint> targetOrigin = stackalloc nuint[3];
            Span<nuint> sourceOrigin = stackalloc nuint[3];
            Span<nuint> region = stackalloc nuint[3];
            Span<nuint> targetPitch = stackalloc nuint[3];
            Span<nuint> sourcePitch = stackalloc nuint[3];

            BufferRectangularExtensions.ConvertArguments(
                target, source, targetOrigin, sourceOrigin, region, targetPitch, sourcePitch);

            // Assert
            Assert.That(region[0], Is.EqualTo((nuint)(512 * sizeof(float))));
            Assert.That(region[1], Is.EqualTo((nuint)1024));
            Assert.That(region[2], Is.EqualTo((nuint)1));

            Assert.That(sourceOrigin[0], Is.EqualTo((nuint)0));
            Assert.That(sourceOrigin[1], Is.EqualTo((nuint)1024));
            Assert.That(sourceOrigin[2], Is.EqualTo((nuint)0));

            Assert.That(sourcePitch[0], Is.EqualTo((nuint)(1024 * sizeof(float))));
            Assert.That(sourcePitch[1], Is.EqualTo((nuint)0));
            Assert.That(sourcePitch[2], Is.EqualTo((nuint)0));

            Assert.That(targetOrigin[0], Is.EqualTo((nuint)(512 * sizeof(float))));
            Assert.That(targetOrigin[1], Is.EqualTo((nuint)0));
            Assert.That(targetOrigin[2], Is.EqualTo((nuint)0));

            Assert.That(targetPitch[0], Is.EqualTo((nuint)(1024 * sizeof(float))));
            Assert.That(targetPitch[1], Is.EqualTo((nuint)0));
            Assert.That(targetPitch[2], Is.EqualTo((nuint)0));
        }
    }
}
