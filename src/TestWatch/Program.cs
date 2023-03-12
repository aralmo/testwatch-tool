using System.Management.Automation;
using Spectre.Console;
using Spectre.Console.Cli;
using TrxFileParser;
using TrxFileParser.Models;

internal class Program
{
    private static int Main(string[] args)
    {
        var app = new CommandApp<WatchCommand>();
        return app.Run(args);
    }


}
