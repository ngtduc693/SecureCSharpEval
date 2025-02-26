using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Loader;
using System.Text;
using System.Threading.Tasks;
using SecureCSharpEval.Models;
using System.Text.RegularExpressions;

namespace SecureCSharpEval
{
    public class ScriptEvaluator
    {
        private readonly SecurityConfiguration _securityConfig;

        public ScriptEvaluator() : this(new SecurityConfiguration())
        {
        }
        
        public ScriptEvaluator(SecurityConfiguration securityConfig)
        {
            _securityConfig = securityConfig;
        }

        public async Task<EvaluationResult> EvaluateAsync(string code, Dictionary<string, object> parameters = null, Type returnType = null)
        {
            code = Regex.Replace(code, @"\s+", " ");
            var result = new EvaluationResult();

            try
            {
                if (code.Length > _securityConfig.MaxSourceSizeBytes)
                {
                    result.HasError = true;
                    result.ErrorMessage = $"Overload the ({_securityConfig.MaxSourceSizeBytes} bytes)";
                    return result;
                }

                var securityCheck = ValidateCodeSecurity(code);
                if (!securityCheck.IsValid)
                {
                    result.HasError = true;
                    result.ErrorMessage = securityCheck.ErrorMessage;
                    return result;
                }

                var scriptCode = PrepareCode(code, parameters, returnType);

                var compilationResult = await CompileCodeAsync(scriptCode);

                if (compilationResult.HasError)
                {
                    result.HasError = true;
                    result.ErrorMessage = compilationResult.ErrorMessage;
                    result.Warnings = compilationResult.Warnings;
                    return result;
                }

                var executionResult = await ExecuteCodeAsync((byte[])compilationResult.Result, parameters);

                result.Result = executionResult.Result;
                result.HasError = executionResult.HasError;
                result.ErrorMessage = executionResult.ErrorMessage;
                result.ExecutionTimeMs = executionResult.ExecutionTimeMs;

                return result;
            }
            catch (Exception ex)
            {
                result.HasError = true;
                result.ErrorMessage = $"Error: {ex.Message}";
                return result;
            }
        }

        private (bool IsValid, string ErrorMessage) ValidateCodeSecurity(string code)
        {
            foreach (var keyword in _securityConfig.BlockedKeywords)
            {
                if (code.Contains(keyword))
                {
                    return (false, $"Blocked: {keyword}");
                }
            }
            
            var syntaxTree = CSharpSyntaxTree.ParseText(code);
            var root = syntaxTree.GetRoot();

            var usingDirectives = root.DescendantNodes().OfType<Microsoft.CodeAnalysis.CSharp.Syntax.UsingDirectiveSyntax>();

            foreach (var usingDirective in usingDirectives)
            {
                var ns = usingDirective.Name.ToString();

                foreach (var blockedNs in _securityConfig.BlockedNamespaces)
                {
                    if (ns == blockedNs || ns.StartsWith(blockedNs + "."))
                    {
                        return (false, $"Blocked: {ns}");
                    }
                }

                bool isAllowed = false;
                foreach (var allowedNs in _securityConfig.AllowedNamespaces)
                {
                    if (ns == allowedNs || ns.StartsWith(allowedNs + "."))
                    {
                        isAllowed = true;
                        break;
                    }
                }

                if (!isAllowed)
                {
                    return (false, $"Blocked: {ns}");
                }
            }

            return (true, null);
        }

        private string PrepareCode(string code, Dictionary<string, object> parameters, Type returnType)
        {
            var usings = string.Join("\n", _securityConfig.AllowedNamespaces.Select(ns => $"using {ns};"));

            var paramDeclarations = "";
            var paramAssignments = "";

            if (parameters != null && parameters.Count > 0)
            {
                paramDeclarations = string.Join("\n        ", parameters.Select(p =>
                {
                    var type = p.Value?.GetType().FullName ?? "object";
                    return $"public static {type} {p.Key};";
                }));

                paramAssignments = "";
            }

            var returnTypeStr = returnType?.FullName ?? "object";

            return $@"
                {usings}

                namespace ScriptEvaluation
                {{
                    public static class Script
                    {{
                        {paramDeclarations}
        
                        public static {returnTypeStr} Evaluate()
                        {{
                {paramAssignments}
            
                            {code}
                        }}
                    }}
                }}";
        }

        private async Task<EvaluationResult> CompileCodeAsync(string code)
        {
            var result = new EvaluationResult();

            try
            {
                var syntaxTree = CSharpSyntaxTree.ParseText(code);

                var assemblyPath = Path.GetDirectoryName(typeof(object).Assembly.Location);
                var references = new List<MetadataReference>
                {
                    MetadataReference.CreateFromFile(typeof(object).Assembly.Location),
                    MetadataReference.CreateFromFile(typeof(Console).Assembly.Location),
                    MetadataReference.CreateFromFile(typeof(Enumerable).Assembly.Location),
                    MetadataReference.CreateFromFile(Path.Combine(assemblyPath, "mscorlib.dll")),
                    MetadataReference.CreateFromFile(Path.Combine(assemblyPath, "System.dll")),
                    MetadataReference.CreateFromFile(Path.Combine(assemblyPath, "System.Core.dll")),
                    MetadataReference.CreateFromFile(Path.Combine(assemblyPath, "System.Runtime.dll"))
                };

                var compilation = CSharpCompilation.Create(
                    $"ScriptAssembly_{Guid.NewGuid():N}",
                    new[] { syntaxTree },
                    references,
                    new CSharpCompilationOptions(
                        OutputKind.DynamicallyLinkedLibrary,
                        optimizationLevel: OptimizationLevel.Release,
                        allowUnsafe: false
                    )
                );

                var diagnostics = compilation.GetDiagnostics();
                var errors = diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error).ToList();

                if (errors.Any())
                {
                    result.HasError = true;
                    result.ErrorMessage = string.Join("\n", errors.Select(e => $"[{e.Location}] {e.GetMessage()}"));
                    result.Warnings = diagnostics.Where(d => d.Severity == DiagnosticSeverity.Warning);
                    return result;
                }

                using var ms = new MemoryStream();
                var emitResult = compilation.Emit(ms);

                if (!emitResult.Success)
                {
                    result.HasError = true;
                    result.ErrorMessage = string.Join("\n", emitResult.Diagnostics
                        .Where(d => d.Severity == DiagnosticSeverity.Error)
                        .Select(e => e.GetMessage()));
                    result.Warnings = emitResult.Diagnostics.Where(d => d.Severity == DiagnosticSeverity.Warning);
                    return result;
                }

                ms.Seek(0, SeekOrigin.Begin);
                result.Result = ms.ToArray();
                result.Warnings = diagnostics.Where(d => d.Severity == DiagnosticSeverity.Warning);

                return result;
            }
            catch (Exception ex)
            {
                result.HasError = true;
                result.ErrorMessage = $"Error: {ex.Message}";
                return result;
            }
        }

        private async Task<EvaluationResult> ExecuteCodeAsync(byte[] assemblyBytes, Dictionary<string, object> parameters)
        {
            var result = new EvaluationResult();
            var startTime = DateTime.Now;

            try
            {
                var loadContext = new AssemblyLoadContext("ScriptExecution", true);

                var executionTask = Task.Run(() =>
                {
                    try
                    {
                        using var ms = new MemoryStream(assemblyBytes);
                        var assembly = loadContext.LoadFromStream(ms);

                        var scriptType = assembly.GetType("ScriptEvaluation.Script");
                        if (scriptType == null)
                        {
                            throw new InvalidOperationException("Not found Script");
                        }

                        if (parameters != null)
                        {
                            foreach (var param in parameters)
                            {
                                var field = scriptType.GetField(param.Key, BindingFlags.Public | BindingFlags.Static);
                                if (field != null)
                                {
                                    field.SetValue(null, param.Value);
                                }
                            }
                        }

                        var evaluateMethod = scriptType.GetMethod("Evaluate", BindingFlags.Public | BindingFlags.Static);
                        if (evaluateMethod == null)
                        {
                            throw new InvalidOperationException("Not found Evaluate");
                        }

                        var methodResult = evaluateMethod.Invoke(null, null);
                        return methodResult;
                    }
                    catch (Exception ex)
                    {
                        var innerEx = ex.InnerException ?? ex;
                        throw new Exception($"Error: {innerEx.Message}", innerEx);
                    }
                });

                var timeoutTask = Task.Delay(_securityConfig.TimeoutMs);
                var completedTask = await Task.WhenAny(executionTask, timeoutTask);

                if (completedTask == timeoutTask)
                {
                    loadContext.Unload();

                    result.HasError = true;
                    result.ErrorMessage = $"Out of time execution (>{_securityConfig.TimeoutMs}ms)";
                    result.ExecutionTimeMs = (long)(DateTime.Now - startTime).TotalMilliseconds;

                    return result;
                }

                result.Result = await executionTask;
                result.ExecutionTimeMs = (long)(DateTime.Now - startTime).TotalMilliseconds;

                loadContext.Unload();

                return result;
            }
            catch (Exception ex)
            {
                result.HasError = true;
                result.ErrorMessage = ex.Message;
                result.ExecutionTimeMs = (long)(DateTime.Now - startTime).TotalMilliseconds;

                return result;
            }
        }

    }


}
