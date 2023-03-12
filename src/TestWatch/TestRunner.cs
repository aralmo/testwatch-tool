using System.Management.Automation;
using TrxFileParser;
using TrxFileParser.Models;

public static class TestRunner
{
    public static event EventHandler<TestRun>? OnTestRunFinished;
    public static TestRun? RunTests(string root, string tempPath, CancellationToken token)
    {
        var ps = PowerShell.Create();
        string logName = Path.Combine(tempPath, "log.trx");

        ps.AddScript($"set-location \"{root}\"")
          .AddScript($"dotnet test -o \"{tempPath}\" -l \"trx;LogFileName={logName}\"");

        token.Register(() => ps.Stop());

        var result = ps.Invoke();
        ps.Dispose();

        string output = String.Join("\r\n", result);

        if (!token.IsCancellationRequested)
        {
            return TrxDeserializer.Deserialize(logName);
        }

        return null;
    }
}