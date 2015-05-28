using System;
using System.Diagnostics;
using System.IO;

namespace Konamiman.Sjasm.Tests
{
    public abstract class TestsClass
    {
        protected ExecutionResult ExecuteSjasm(string arguments)
        {
            var startInfo = new ProcessStartInfo(@"..\..\bin\sjasm.exe", arguments) {
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

        protected AssemblyResult Assemble(string sourceCode, bool throwOnErrors = true)
        {
            File.WriteAllText("temp.asm", sourceCode);

            var result = ExecuteSjasm("-e temp.asm");

            if(result.ExitCode != 0 && throwOnErrors) {
                throw new InvalidOperationException(
                    $"Assembly failed with error code {result.ExitCode}:\r\n\r\n{result.StandardError}");
            }

            return new AssemblyResult
            {
                AssembledCode = File.Exists("temp.out") ? null : File.ReadAllBytes("temp.out"),
                ExitCode = result.ExitCode,
                Errors = result.StandardError.Split(new[] {Environment.NewLine}, StringSplitOptions.RemoveEmptyEntries)
            };
        }
    }

    public class AssemblyResult
    {
        public byte[] AssembledCode { get; set; }
        public int ExitCode { get; set; }
        public string[] Errors { get; set; }
    }
}
