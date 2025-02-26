using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SecureCSharpEval
{
    public class SecurityConfiguration
    {
        public HashSet<string> AllowedNamespaces { get; set; } = new HashSet<string>
        {
            "System",
            "System.Collections.Generic",
            "System.Linq",
            "System.Text",
            "System.Threading.Tasks"
        };
        public HashSet<string> BlockedNamespaces { get; set; } = new HashSet<string>
        {
            "System.IO",
            "System.Net",
            "System.Reflection",
            "System.Diagnostics",
            "System.Runtime",
            "Microsoft.Win32"
        };
        public HashSet<string> BlockedKeywords { get; set; } = new HashSet<string>
        {
            "unsafe",
            "fixed",
            "stackalloc",
            "Process",
            "File",
            "Directory",
            "Registry",
            "Socket",
            "WebClient",
            "HttpClient"
        };
        public int TimeoutMs { get; set; } = 5000;
        public int MaxSourceSizeBytes { get; set; } = 10 * 1024;
    }
}
