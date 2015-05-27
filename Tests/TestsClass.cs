using System.Diagnostics;

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
    }
}
