using NUnit.Framework;

namespace Konamiman.Sjasm.Tests
{
    public class SjasmExecutionTests : TestsClass
    {
        [Test]
        public void Can_execute_sjasm()
        {
            ExecuteSjasm("");
        }

        [Test]
        public void Returns_exit_code_1_when_executed_without_parameters()
        {
            var result = ExecuteSjasm("");

            Assert.AreEqual(1, result.ExitCode);
        }

        [Test]
        public void Returns_standard_output_when_executed()
        {
            var result = ExecuteSjasm("");

            Assert.True(result.StandardOutput.StartsWith("SjASM Z80"));
        }
    }
}
