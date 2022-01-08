using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using NUnit.Framework;

namespace Konamiman.Sjasm.Tests
{
    public abstract class TestsClass
    {
        protected ExecutionResult ExecuteSjasm(string arguments)
        {
            var myPath = new Uri(Path.GetDirectoryName(Assembly.GetExecutingAssembly().CodeBase)).LocalPath;
            var sjasmPath = Path.Combine(myPath, "../../bin/Sjasm.exe");

            var startInfo = new ProcessStartInfo(sjasmPath, arguments) {
                CreateNoWindow = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false
            };

            var proc = new Process();
            proc.StartInfo = startInfo;
            proc.Start();
            proc.WaitForExit();

            return new ExecutionResult
            {
                ExitCode = proc.ExitCode,
                StandardError = proc.StandardError.ReadToEnd(),
                StandardOutput = proc.StandardOutput.ReadToEnd()
            };
        }

        protected AssemblyResult Assemble(string sourceCode, string commandLineOptions="", bool throwOnErrors = true)
        {
            File.WriteAllText("temp.asm", sourceCode);

            var result = ExecuteSjasm($"-e {commandLineOptions} temp.asm");

            if(result.ExitCode != 0 && throwOnErrors) {
                throw new InvalidOperationException(
                    $"Assembly failed with error code {result.ExitCode}:\r\n\r\n{result.StandardError}");
            }

            return new AssemblyResult
            {
                AssembledCode = File.Exists("temp.out") ? File.ReadAllBytes("temp.out") : null,
                ExitCode = result.ExitCode,
                Errors = result.StandardError.Split(new[] {Environment.NewLine}, StringSplitOptions.RemoveEmptyEntries)
            };
        }

        protected void AssertProduceSameCode(
            string program1Source,
            string program2Source, 
            string program1Options = "", 
            string program2Options = "")
        {
            var program1 = Assemble(program1Source, program1Options).AssembledCode;
            var program2 = Assemble(program2Source, program2Options).AssembledCode;

            CollectionAssert.AreEqual(program2, program1);
        }

        protected void AssertDoesNotCompile(string source)
        {
            var result = Assemble(source, throwOnErrors: false);
            Assert.AreEqual(1, result.ExitCode);
        }
    }

    public class AssemblyResult
    {
        public byte[] AssembledCode { get; set; }
        public int ExitCode { get; set; }
        public string[] Errors { get; set; }
    }
}
