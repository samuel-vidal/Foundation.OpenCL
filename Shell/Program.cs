using Foundation.OpenCL.Tests;

namespace Shell
{
    internal class Program
    {
        static void Main(string[] args)
        {
            var test = new IntelMxmIntrinsicsTests();

            test.Test_1_2048_16n_row_maj(128);
        }
    }
}
