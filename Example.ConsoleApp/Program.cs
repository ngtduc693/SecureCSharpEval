namespace Example.ConsoleApp;

using SecureCSharpEval;
using System.Reflection.Metadata.Ecma335;
using System.Text.RegularExpressions;
using static System.Runtime.InteropServices.JavaScript.JSType;

class Program
{
#pragma warning disable S1144
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
#pragma warning restore S1144
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