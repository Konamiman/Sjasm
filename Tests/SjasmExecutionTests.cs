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
        public void Returns_exit_code_0_when_executed_without_parameters()
        {
            var result = ExecuteSjasm("");

            Assert.AreEqual(0, result.ExitCode);
        }

        [Test]
        public void Returns_standard_output_when_executed()
        {
            var result = ExecuteSjasm("");

            Assert.True(result.StandardOutput.StartsWith("SjASM Z80"));
        }

        [Test]
        public void Prints_errors_in_stdout_if_no_e_switch_specified()
        {
            var result = ExecuteSjasm("x");

            Assert.True(result.StandardOutput.Contains("Error"));
        }

        [Test]
        public void Prints_errors_in_stderr_if_e_switch_specified()
        {
            var result = ExecuteSjasm("-e x");

            Assert.True(result.StandardError.Contains("Error"));
        }

        [Test]
        public void Returns_errorcode_4_if_no_input_file_provider()
        {
            var result = ExecuteSjasm("-e");

            Assert.AreEqual(4, result.ExitCode);
        }

        [Test]
        public void Returns_errorcode_5_if_unknown_option_specified()
        {
            var result = ExecuteSjasm("-x x");

            Assert.AreEqual(5, result.ExitCode);
        }
    }
}
