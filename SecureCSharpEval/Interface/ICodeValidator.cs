using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SecureCSharpEval.Interface;

public interface ICodeValidator
{    
    void Validate(string code, bool allowShellExecution);
}
