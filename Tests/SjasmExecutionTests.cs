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
        public void Returns_exit_code_4_when_executed_without_parameters()
        {
            var result = ExecuteSjasm("");

            Assert.AreEqual(4, result.ExitCode);
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
            var result = ExecuteSjasm("");

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

        [Test]
        public void Does_not_reverse_multi_POP_if_no_c_option_specified()
        {
            var program1Source = " pop af,bc,de,hl";
            var program2Source = " pop hl\r\n pop de\r\n pop bc\r\n pop af";

            var program1 = Assemble(program1Source).AssembledCode;
            var program2 = Assemble(program2Source).AssembledCode;

            CollectionAssert.AreEqual(program2, program1);
        }

        [Test]
        public void Reverses_multi_POP_if_c_option_specified()
        {
            var program1Source = " pop af,bc,de,hl";
            var program2Source = " pop af\r\n pop bc\r\n pop de\r\n pop hl";

            AssertProduceSameCode(program1Source, program2Source, "-c");
        }

        [Test]
        public void Cannot_parse_Compass_macros_and_own_macros_with_parameters_if_c_option_not_specified()
        {
            AssertDoesNotCompile("test: macro @a\r\n endm");
        }

        [Test]
        public void Can_parse_Compass_macros_and_own_macros_with_parameters_if_c_option_specified()
        {
            var program1Source =
 @"test: macro @a,@b
 ld a,@a
 ld b,@b
 endm

;Remark

 test 1,2
 test 3,4 ;Uh!

  ;Remark

 macro test2 aa,bb
 ld a,aa
 ld b,bb ;Oh!
 endm

 test2 5,6
 test2 7,8

test3: macro
 nop
 nop
 endm

 test3

 macro test4
 halt
 endm

 test4
";

            var program2Source =
@" ld a,1
 ld b,2
 ld a,3
 ld b,4
 ld a,5
 ld b,6
 ld a,7
 ld b,8
 nop
 nop
 halt
 ";

            AssertProduceSameCode(program1Source, program2Source, "-c");
        }

        [Test]
        public void Supports_local_labels_inside_Compass_style_macros()
        {
            var program1Source =
@"test: macro
loop@sym: djnz loop@sym
data@sym: db 0
 ld hl,data@sym
 endm

 test
 test
 test";

            var program2Source =
@" macro test
.loopSym: djnz .loopSym
.dataSym: db 0
 ld hl,.dataSym
 endm

 test
 test
 test";

            AssertProduceSameCode(program1Source, program2Source, "-c");
        }

        [Test]
        public void Accepts_empty_string_as_zero_if_c_option_specified()
        {
            var program1Source = " ld a,\"\"\r\n ld b,''";
            var program2Source = " ld a,0\r\n ld b,0";

            AssertProduceSameCode(program1Source, program2Source, "-c");
        }

        [Test]
        public void Does_not_accept_empty_string_as_zero_if_c_option_not_specified()
        {
            AssertDoesNotCompile(" ld a,\"\"");
            AssertDoesNotCompile(" ld a,''");
        }

        [Test]
        public void Accepts_spaces_in_numeric_constants_if_c_option_specified()
        {
            var program1Source =
@" ld a,% 11 00 11 00 ;!
 ld a,11 00 11 00 b ;!
 ld a,&b 11 00 11 00 ;!
 ld bc,&h AA BB ;!
 ld bc,0 AA BB h ;!
 ld bc,0x AA BB ;!
 ld de,12 34 ;!
";
            var program2Source = 
@" ld a,%11001100
 ld a,11001100b
 ld a,&b11001100
 ld bc,&hAABB
 ld bc,0AABBh
 ld bc,0xAABB
 ld de,1234
";

            AssertProduceSameCode(program1Source, program2Source, "-c");
        }

        [Test]
        public void Does_not_accept_spaces_in_numeric_constants_if_c_option_not_specified()
        {
            AssertDoesNotCompile(" ld a,%11 00 11 00");
        }

        [Test]
        public void Can_use_cond_endc_if_c_option_specified()
        {
            var program1Source =
 @" cond 1
 nop
 endc

 cond 1
 nop
 endif

 if 1
 nop
 endc

 if 1
 nop
 endif";
            var program2Source = " ds 4,0";

            AssertProduceSameCode(program1Source, program2Source, "-c");
        }

        [Test]
        public void Cannot_use_cond_endc_if_c_option_not_specified()
        {
            AssertDoesNotCompile(" cond\r\n endif");
            AssertDoesNotCompile(" if\r\n endc");
        }
    }
}
