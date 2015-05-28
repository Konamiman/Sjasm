using System.Text;
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
        public void Returns_errorcode_1_if_source_has_errors()
        {
            var result = Assemble(" dummy", throwOnErrors: false);

            Assert.AreEqual(1, result.ExitCode);
            CollectionAssert.IsNotEmpty(result.Errors);
        }

        [Test]
        public void Returns_errorcode_2_if_cant_open_file()
        {
            var result = Assemble(@" incbin""dummy""", throwOnErrors: false);

            Assert.AreEqual(2, result.ExitCode);
            CollectionAssert.IsNotEmpty(result.Errors);
        }

        [Test]
        public void Returns_errorcode_3_on_fatal_error()
        {
            var programBuilder = new StringBuilder();
            for(int i = 1; i <= 40000; i++)
                programBuilder.AppendLine($"label{i}: nop");
            var program = programBuilder.ToString();
            
            var result = Assemble(program, throwOnErrors: false);

            Assert.AreEqual(3, result.ExitCode);
            CollectionAssert.IsNotEmpty(result.Errors);
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

        [Test]
        public void Returns_error_messages_in_visual_studio_format_if_v_option_specified()
        {
            var result = Assemble(" dummy", "-v", false);

            var errorParts = result.Errors[0].Split(':');
            Assert.GreaterOrEqual(errorParts.Length, 3);
            Assert.True(errorParts[0].EndsWith("(1) "));
            Assert.AreEqual(" error PASS2 ", errorParts[1]);
        }

        [Test]
        public void Returns_error_messages_in_non_visual_studio_format_if_v_option_specified()
        {
            var result = Assemble(" dummy", throwOnErrors: false);

            var errorParts = result.Errors[0].Split(':');
            Assert.GreaterOrEqual(errorParts.Length, 2);
            Assert.True(errorParts[0].EndsWith("line 1"));
        }
    }
}
