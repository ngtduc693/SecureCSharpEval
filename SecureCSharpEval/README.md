# SecureCSharpEval

**SecureCSharpEval**  is a secure and sandboxed C# script execution library that allows users to evaluate C# code dynamically while ensuring safety and preventing malicious activities.

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

- Basic Usage

````csharp
var eval = new SecureCSharpEval(new CodeValidator(), new ProcessExecutor());
string code = "return x + y;";
var parameters = new Dictionary<string, object>
{
    {"x", 10},
    {"y", 20}
};

object? result = await eval.EvaluateAsync(code, parameters);
Console.WriteLine($"Result: {result}");
````

- Using Conditional Statements and Loops

```csharp
string code = @" 
    int sum = 0;
    for (int i = 0; i < n; i++)
    {
        if (i % 2 == 0)
        {
            sum += i;
        }
    }
    return sum;
";

var parameters = new Dictionary<string, object>
{
    {"n", 10}
};

object? result = await eval.EvaluateAsync(code, parameters);
Console.WriteLine($"Result: {result}");
```

## Configuring Execution Limits

```csharp
eval.ExecutionTimeoutMs = 3000;  // Increase timeout to 3 seconds
eval.MaxMemoryUsageMb = 100;     // Set max memory usage to 100 MB
eval.AllowShellExecution = true; // Enable shell execution (use with caution)
```

## Security Considerations
- By default, execution is **sandboxed** and **restricted** from running system commands.
- If enabling **AllowShellExecution**, ensure proper input validation.
- Avoid allowing user-provided scripts without proper review.

## Contact

For any questions, feel free to contact me or create an issue in the repository.