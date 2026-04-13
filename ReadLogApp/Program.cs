using System;
using System.IO;
using System.Text.RegularExpressions;

class Program
{
    static void Main()
    {
        var text = File.ReadAllText(@"c:\Code\MeAjudaAi\test_output.log");
        var match = Regex.Match(text, @"Ocorreu um erro inesperado: (.*?)\}");
        if (match.Success) {
            Console.WriteLine(match.Value);
        }
        else {
            var match2 = Regex.Match(text, "DEBUG RESPONSE BODY.*");
            Console.WriteLine(match2.Value);
        }
    }
}
