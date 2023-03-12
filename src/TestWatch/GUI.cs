using System.Text.RegularExpressions;
using Spectre.Console;
using TrxFileParser.Models;

public static class GUI
{
    public static void RunningAction(string text, Action action)
    {
        AnsiConsole
            .Status()
            .Spinner(Spinner.Known.Dots)
            .Start(text, ctx => action());
    }

    static Grid? testGrid = null;
    static TestRun? lastRun = null;
    public static void DrawTestResults(TestRun testRun, WatchCommandSettings settings)
    {
        var filteredTests = testRun
                .Results
                .UnitTestResults
                .AsEnumerable();

        if (settings.Filter != null)
            filteredTests = filteredTests
                .Where(t => t.TestName
                    .Contains(settings.Filter,StringComparison.CurrentCultureIgnoreCase));

        filteredTests = filteredTests.ToArray().AsReadOnly();

        AnsiConsole.MarkupLine($"[grey62]{filteredTests.Count()}  ran at {DateTime.Now.ToString("t")}[/]\n");

        var testclasses = filteredTests
            .Select(x =>
            {
                var split = x.TestName.Split('.');
                return new
                {
                    Name = ToPrettyTestName(split[^1]),
                    Class = ToPrettyClassName(split[^2]),
                    Duration = $"{TimeSpan.Parse(x.Duration).Milliseconds} ms",
                    x.Outcome,
                    x.Output?.ErrorInfo?.Message,
                    x.Output?.ErrorInfo?.StackTrace,
                    Changed = lastRun?.Results
                        .UnitTestResults
                        .Any(utr =>
                            utr.TestName == x.TestName &&
                            utr.Outcome != x.Outcome) ?? false
                };
            })
            .GroupBy(x => x.Class)
            .Select(grp =>
                new
                {
                    Name = grp.Key,
                    Tests = grp.OrderBy(t => t.Name).ToArray().AsReadOnly(),
                    Count = grp.Count(),
                    Passed = grp.Count(x => x.Outcome == "Passed"),
                    Changed = grp.Any(t => t.Changed)
                })
            .OrderByDescending(i => i.Passed / (double)i.Count)
            .ThenBy(i => i.Name)
            .ToArray();

        lastRun = testRun;

        foreach (var group in testclasses)
        {
            bool allPassed = group.Passed == group.Count;
            var groupTitle = $"[navajowhite3][underline]{group.Name}[/]{(group.Changed?"⚡":"")} ({group.Passed}/{group.Count}) {(allPassed ? "[green3][/]" : "[maroon][/]")}[/]";
            AnsiConsole.MarkupLine(groupTitle);

            if (!allPassed || settings.ExpandPassedTests)
            {
                testGrid = new Grid();
                testGrid.AddColumns(3);
                testGrid.Columns[2].Alignment = Justify.Right;

                foreach (var test in group.Tests)
                {                    
                    string[] row = test.Outcome switch
                    {
                        "Passed" => new[] { $"{(test.Changed?"[white on grey37]":"")}{test.Name}{(test.Changed?"[/]":"")}", "[green3] pass[/]", test.Duration },
                        "Failed" => new[] { $"{(test.Changed?"[white on grey37]":"")}{test.Name}{(test.Changed?"[/]":"")}", "[maroon] fail[/]", test.Duration },
                        "NotExecuted" => new[] { $"[grey50]{test.Name}[/]", "[grey50]skipped[/]" },
                        _ => new[] { test.Name, test.Outcome, test.Duration }
                    };

                    testGrid.AddRow(row);
                }

                var panel = new Panel(testGrid)
                        .Padding(1, 0, 0, 0)
                        .Header(groupTitle)
                        .Border(BoxBorder.None);

                AnsiConsole.Write(panel);

                AnsiConsole.WriteLine();

                if (settings.SkipTestOutput == false)
                    foreach (var failed in group.Tests.Where(t => t.Outcome == "Failed")){
                        var link = LinkFromStack(failed.StackTrace);
                        AnsiConsole.MarkupLine($"[underline lightpink3]{failed.Name}[/]\n{failed.Message} {link}");
                    }

                AnsiConsole.WriteLine();
            }
        }
    }
    private static string LinkFromStack(string stack)
    {
        var matches = Regex
            .Matches(stack, @"^(.*(?:cs|fs)):line ([0-9].)$", 
                	RegexOptions.Multiline | RegexOptions.IgnoreCase);
        
        var match = matches.FirstOrDefault();

        if (match?.Success ?? false)
        {
            string path = match
                .Groups[1]
                .Value
                .Split(':')[1]
                .Replace('\\','/')
                .Replace(" ", "+");

            return $"[lightcyan3][link=vscode://file{path}]{Path.GetFileName(path)}:{match.Groups[2].Value}[/][/]";
        }
        return "";
    }


    private static object ToPrettyClassName(string inputString)
    {
        string outputString =
            Regex.Replace(
                inputString,
                "(?<=[a-z])([S|s]hould)",
                " Should"
            );
        return outputString;
    }

    static string ToPrettyTestName(string inputString)
    {
        string outputString = Regex.Replace(
            inputString,
            "(?<=[a-z])([A-Z])",
            " $1"
        );
        return outputString.ToLower();
    }
}