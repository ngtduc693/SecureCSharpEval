using SecureCSharpEval.Interface;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace SecureCSharpEval.Services;

public class ProcessExecutor : IProcessExecutor
{
    public async Task<object?> ExecuteAsync(string code, string jsonParams, ISecureCSharpEval settings)
    {
        var startInfo = new ProcessStartInfo
        {
            FileName = "dotnet",
            Arguments = $"exec SandboxExecutor.dll \"{code}\" \"{jsonParams}\"",
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        if (!settings.AllowShellExecution)
        {
            startInfo.EnvironmentVariables["DISABLE_SHELL_EXEC"] = "true";
        }

        startInfo.EnvironmentVariables["MAX_MEMORY_MB"] = settings.MaxMemoryUsageMb.ToString();
        startInfo.EnvironmentVariables["MAX_CPU_TIME_MS"] = settings.MaxCpuTimeMs.ToString();

        using var process = new Process { StartInfo = startInfo };
        process.Start();

        if (!process.WaitForExit(settings.ExecutionTimeoutMs))
        {
            process.Kill();
            throw new TimeoutException("Script execution timed out.");
        }

        string result = await process.StandardOutput.ReadToEndAsync();
        return JsonSerializer.Deserialize<object>(result);
    }
}
