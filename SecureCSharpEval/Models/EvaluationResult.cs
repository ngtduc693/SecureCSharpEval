using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SecureCSharpEval.Models
{
    public class EvaluationResult
    {
        public object Result { get; set; }

        public bool HasError { get; set; }

        public string ErrorMessage { get; set; }

        public long ExecutionTimeMs { get; set; }

        public IEnumerable<Diagnostic> Warnings { get; set; }
    }

}
