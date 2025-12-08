using Foundation.OpenCL.Tests.Tensors;
using System;

namespace Foundation.OpenCL.Tests
{
    [Category("Hardware")]
    public class IntelMxmIntrinsicsTests
    {
        [Test]
        public void Test_8_16_16_row_maj()
        {
            const int inputSize = 16;
            const int outputSize = 16;
            const int batchSize = 8;

            const string kernelName = "gemm_8_16_16";

            Console.WriteLine(
                $"Test {kernelName} : inputSize = {inputSize}, outputSize = {outputSize}, batchSize = {batchSize},");

            TestUtils.RunTest(
                batchSize, inputSize, outputSize,
                TensorLayout.LastIndexContiguous,
                TensorLayout.LastIndexContiguous,
                TensorLayout.FirstIndexContiguous,
                kernelName, (Half)2.5e-2);
        }

        [Test]
        public void Test_8_16_16_col_maj()
        {
            const int inputSize = 16;
            const int outputSize = 8;
            const int batchSize = 16;

            const string kernelName = "gemm_8_16_16_col_maj";

            Console.WriteLine(
                $"Test {kernelName} : inputSize = {inputSize}, outputSize = {outputSize}, batchSize = {batchSize},");

            TestUtils.RunTest(
                outputSize, inputSize, batchSize, 
                TensorLayout.FirstIndexContiguous,
                TensorLayout.FirstIndexContiguous,
                TensorLayout.FirstIndexContiguous,
                kernelName, (Half)2.5e-2);
        }
    }
}
