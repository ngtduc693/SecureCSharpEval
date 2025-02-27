# SecureCSharpEval

**SecureCSharpEval**  is a secure and sandboxed C# script execution library that allows users to evaluate C# code dynamically while ensuring safety and preventing malicious activities.

![Illustration](https://raw.githubusercontent.com/ngtduc693/SecureCSharpEval/refs/heads/main/imgs/demo.png)
## Features

- Execute **C# scripts dynamically** with controlled parameters.
- **Prevents unsafe operations** such as file access, process execution, and system manipulation.
- **Configurable** execution timeout and memory limits.
- Optimized for .NET – Fully compatible with .NET 6/7/8/9.
- **Optional shell execution** support (**DISABLED** by default for security).

## Installation

You can install this library via NuGet Package Manager:

```bash
Install-Package SecureCSharpEval
```

## Usage

- Using Conditional Statements, Loops and the new method

```csharp
class Program
{
    private static string checkPrime = @"
    static bool IsPrime(int n)
    {
        if (n < 2) return false;
        for (int i = 2; i * i <= n; i++)
        {
            if (n % i == 0) return false;
        }
        return true;
    };
    return IsPrime(number);
    ";
    static async Task Main(string[] args)
    {
        var n = 4;
        var evaluator = new ScriptEvaluator();

        Console.WriteLine("Before: {0}",n);

        var parameters = new Dictionary<string, object>
        {
            { "number", n },
        };
        
        var result = await evaluator.EvaluateAsync(checkPrime, parameters);

        if (result.HasError)
        {
            Console.WriteLine($"Error: {result.ErrorMessage}");
        }
        else
        {
            Console.WriteLine($"Is Prime: {result.Result}");
            Console.WriteLine($"Execution Time: {result.ExecutionTimeMs}ms");
        }

        Console.ReadKey();
    }
}
```

## Configuring Security Options

```csharp
ScriptEvaluator(new SecurityConfiguration()
{
    TimeoutMs = 3000,  // Increase timeout to 3 seconds
    BlockedKeywords  = new HashSet<string>{}, // optional
    BlockedNamespaces =  new HashSet<string>{}; // optional
}
```

## Security Configuration Default

```
- AllowedNamespaces: [
    System,
    System.Collections.Generic,
    System.Linq,
    System.Text,
    System.Threading.Tasks ]
- BlockedNamespaces: [
    System.IO,
    System.Net,
    System.Reflection,
    System.Diagnostics,
    System.Runtime,
    Microsoft.Win32]
- BlockedKeywords: [
    unsafe,
    fixed,
    stackalloc,
    Process,
    File,
    Directory,
    Registry,
    Socket,
    WebClient,
    HttpClient]
- TimeoutMs: 5000ms (5 seconds)
```
**- Execution time limit**
![Execution time limit](https://raw.githubusercontent.com/ngtduc693/SecureCSharpEval/refs/heads/main/imgs/execution%20time%20limit.png)

**- Prevent remote command execution**
![Prevent remote command execution](https://raw.githubusercontent.com/ngtduc693/SecureCSharpEval/refs/heads/main/imgs/remote%20command%20execution%20limit.png)

**- Prevent remote command execution**
![Prevent access to a file or folder](https://raw.githubusercontent.com/ngtduc693/SecureCSharpEval/refs/heads/main/imgs/Files%20or%20Folders%20limit.png)

## Security Considerations
- By default, execution is **sandboxed** and **restricted** from running system commands.
- Avoid allowing user-provided scripts without proper review.

## Contact

For any questions, feel free to contact me or create an issue in the repository.
