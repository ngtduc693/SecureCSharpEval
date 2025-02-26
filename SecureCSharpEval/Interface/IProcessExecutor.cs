using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SecureCSharpEval.Interface;

public interface IProcessExecutor
{
    Task<object?> ExecuteAsync(string code, string jsonParams, ISecureCSharpEval settings);
}
