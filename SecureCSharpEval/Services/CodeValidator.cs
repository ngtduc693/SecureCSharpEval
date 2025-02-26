using SecureCSharpEval.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SecureCSharpEval.Services;

public class CodeValidator : ICodeValidator
{
    private static readonly HashSet<string> ForbiddenKeywords = new()
    {
        "System.IO", "System.Diagnostics", "Process.Start", "File.WriteAllText", "Environment.Exit",
        "DllImport", "Marshal", "AppDomain", "Unsafe", "Reflection"
    };

    public void Validate(string code, bool allowShellExecution)
    {
        if (!allowShellExecution && ForbiddenKeywords.Any(code.Contains))
        {
            throw new UnauthorizedAccessException("The provided code contains forbidden operations.");
        }
    }
}
