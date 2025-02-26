using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.Scripting;
using System.Diagnostics;
using SecureCSharpEval.Interface;

namespace SecureCSharpEval.Services;

public class SecureCSharpEval: ISecureCSharpEval
{
    private readonly ICodeValidator _codeValidator;
    private readonly IProcessExecutor _processExecutor;

    public bool AllowShellExecution { get; set; } = false;
    public int ExecutionTimeoutMs { get; set; } = 2000;
    public int MaxMemoryUsageMb { get; set; } = 50;
    public int MaxCpuTimeMs { get; set; } = 1000;

    public SecureCSharpEval(ICodeValidator codeValidator, IProcessExecutor processExecutor)
    {
        _codeValidator = codeValidator;
        _processExecutor = processExecutor;
    }

    public async Task<object?> EvaluateAsync(string code, Dictionary<string, object>? parameters = null)
    {
        _codeValidator.Validate(code, AllowShellExecution);
        string jsonParams = JsonSerializer.Serialize(parameters ?? new Dictionary<string, object>());

        return await _processExecutor.ExecuteAsync(code, jsonParams, this);
    }
}
