using git = Ignore;
public static class Watcher
{
    public static event EventHandler<CancellationToken>? Trigger;

    public static IDisposable Start(string path, int debouncems)
    {
        var fullpath = Path.GetFullPath(path);
        var watcher = new FileSystemWatcher(fullpath);

        var ignores = Directory
            .GetFiles(fullpath, ".gitignore", SearchOption.AllDirectories)
            .Select(path =>
            {
                var ignore = new git.Ignore();
                foreach (var line in File.ReadAllLines(path).Where(line => !line.StartsWith("#")))
                    ignore.Add(line);

                return new
                {
                    Path = Path.GetDirectoryName(path),
                    Ignore = ignore
                };
            })
            .OrderByDescending(i => i.Path!.Length)
            .ToArray();


        watcher.NotifyFilter = NotifyFilters.Attributes
                             | NotifyFilters.CreationTime
                             | NotifyFilters.DirectoryName
                             | NotifyFilters.FileName
                             | NotifyFilters.LastWrite
                             | NotifyFilters.Security
                             | NotifyFilters.Size;

        void Changed(object sender, FileSystemEventArgs e)
        {
            var match = ignores.FirstOrDefault(ign => e.FullPath.Contains(ign.Path));
            if (match == null || match.Ignore.IsIgnored(e.FullPath.Replace('\\', '/')) == false)
            {
                debounce(debouncems);
            }
        }

        watcher.Changed += Changed;
        watcher.Deleted += Changed;
        watcher.Renamed += Changed;

        watcher.IncludeSubdirectories = true;
        watcher.EnableRaisingEvents = true;

        return watcher;
    }
    static CancellationTokenSource? cts = null;
    static object deblock = new();
    static void debounce(int ms)
    {
        lock (deblock)
        {
            cts?.Cancel();
            cts = new CancellationTokenSource();
            var token = cts.Token;//in closure
            Task.Run(async () =>
            {
                Thread.Sleep(ms);
                if (!token.IsCancellationRequested)
                {
                    Trigger?.Invoke(null, token);
                }
            }, token);
        }
    }

}