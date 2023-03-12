using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using Spectre.Console.Cli;
using TrxFileParser.Models;

public class WatchCommandSettings : CommandSettings
{
    [Description("Test project path")]
    [CommandArgument(0, "<path>")]
    public string? Path { get; set; }

    [Description("Show only tests that contain this text")]
    [CommandArgument(1, "[filter]")]
    public string? Filter { get; set; }

    [Description("Starts watching for changes in the set path")]
    [CommandOption("-w|--watch-path")]
    public string? WatchPath { get; set; }

    [Description("Hide test output for failed tests")]
    [CommandOption("-n|--no-output")]
    [DefaultValue(false)]
    public bool SkipTestOutput { get; set; }

    [Description("Only show test lists when matching this results (p|f|i) ex. '--show fi' would only show failed and ignored tests.")]
    [CommandOption("-s|--show")]
    public string? Show {get;set;}

    [Description("Displays test list for a class even if it does not have failed tests")]
    [CommandOption("-e|--expand")]
    [DefaultValue(false)]
    public bool ExpandPassedTests { get; set; }
}

public class WatchCommand : Command<WatchCommandSettings>
{
    public override int Execute([NotNull] CommandContext context, [NotNull] WatchCommandSettings settings)
    {
        string tempPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(tempPath);
        string root = Path.GetFullPath(settings.Path);

        Watcher.Trigger += (_, token) =>
        {
            RunTests(tempPath, root, token, settings);
        };

        RunTests(tempPath, root, new(), settings);

        if (settings.WatchPath != null)
        {
            //trigger once
            using (Watcher.Start(Path.GetFullPath(settings.WatchPath), 100))
            {
                Console.CursorVisible = false;
                Console.ReadLine();
                Console.CursorVisible = true;
            }
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
        
        if (settings.WatchPath != null)
            Console.Clear();
        
        if (run != null)
        {
            GUI.DrawTestResults(run, settings);
        }
    }
}
