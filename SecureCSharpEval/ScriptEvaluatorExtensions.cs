using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SecureCSharpEval
{
    public static class ScriptEvaluatorExtensions
    {
        public static async Task<T> EvaluateAsync<T>(this ScriptEvaluator evaluator, string code, Dictionary<string, object> parameters = null)
        {
            var result = await evaluator.EvaluateAsync(code, parameters, typeof(T));

            if (result.HasError)
            {
                throw new InvalidOperationException(result.ErrorMessage);
            }

            return (T)result.Result;
        }
    }
}
