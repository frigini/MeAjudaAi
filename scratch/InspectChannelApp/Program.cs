using System;
using System.Linq;
using RabbitMQ.Client;

public class Program
{
    public static void Main()
    {
        var type = typeof(IChannel);
        Console.WriteLine($"Methods of {type.FullName}:");
        foreach (var method in type.GetMethods().OrderBy(m => m.Name))
        {
            var parameters = string.Join(", ", method.GetParameters().Select(p => $"{p.ParameterType.Name} {p.Name}"));
            Console.WriteLine($"- {method.ReturnType.Name} {method.Name}({parameters})");
        }
    }
}
