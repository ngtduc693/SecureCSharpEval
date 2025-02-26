using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace SecureCSharpEval.Interface;

public interface ISecureCSharpEval
{
    bool AllowShellExecution { get; set; }
    int ExecutionTimeoutMs { get; set; }
    int MaxMemoryUsageMb { get; set; }
    int MaxCpuTimeMs { get; set; }
    Task<object?> EvaluateAsync(string code, Dictionary<string, object>? parameters = null);
}
