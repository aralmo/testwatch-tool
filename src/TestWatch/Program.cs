using Spectre.Console.Cli;

internal class Program
{
    private static int Main(string[] args)
    {
        // new WatchCommand().Equals(
        //     new WatchCommandSettings(){
        //         Path = @"C:\Users\work\repo\testwatch-tool\src\Test",
        //         WatchPath = @"C:\Users\work\repo\testwatch-tool\src\"
        //     });

        var app = new CommandApp<WatchCommand>();
        return app.Run(args);
    }


}
