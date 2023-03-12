using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using Spectre.Console.Cli;
using TrxFileParser.Models;

public class WatchCommandSettings : CommandSettings
{
    [Description("Test project path")]
    [CommandArgument(0, "<path>")]
    public string? Path { get; set; }

    [Description("Show only tests that contain this filter")]
    [CommandArgument(1, "[filter]")]
    public string? Filter { get; set; }

    [Description("Doesn't show test output")]
    [CommandOption("-n|--no-output")]
    [DefaultValue(false)]
    public bool SkipTestOutput {get;set;}

    [Description("Displays full test list for classes that don't have failed tests")]
    [CommandOption("-e|--expand")]
    [DefaultValue(false)]
    public bool ExpandPassedTests {get;set;}
}

public class WatchCommand : Command<WatchCommandSettings>
{
    public override int Execute([NotNull] CommandContext context, [NotNull] WatchCommandSettings settings)
    {
        var tempPath = Directory.CreateTempSubdirectory("tw_").FullName;
        string root = Path.GetFullPath(settings.Path);

        Watcher.Trigger += (_, token) =>
        {
            RunTests(tempPath, root, token, settings);
        };

        RunTests(tempPath, root, new(), settings);

        //trigger once
        using (Watcher.Start(root, 100))
        {
            Console.ReadLine();
        }

        return 0;
    }

    static void RunTests(string tempPath, string root, CancellationToken token, WatchCommandSettings settings)
    {
        TestRun? run = null;
        GUI.RunningAction("Running tests", () =>
        {
            run = TestRunner.RunTests(root, tempPath, token);
        });
        Console.Clear();
        if (run != null)
        {
            GUI.DrawTestResults(run, settings);
        }
    }
}
