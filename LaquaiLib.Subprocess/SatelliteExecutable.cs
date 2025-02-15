using System.IO.Pipes;

namespace LaquaiLib.Subprocess;

internal class SatelliteExecutable
{
    public static async Task<int> Main(string[] args)
    {
        var work = MessagePackUtil.Deserialize(Convert.FromBase64String(args[0]));

        var pipeName = args[1];
        using var pipeClient = new NamedPipeClientStream(".", pipeName, PipeDirection.In, PipeOptions.Asynchronous);
        using var reader = new StreamReader(pipeClient, leaveOpen: true);
        var connection = pipeClient.ConnectAsync();
        var sync = Task.Run(async () =>
        {
            while ()
        });

        var arguments = MessagePackUtil.Deserialize<object[]>(Convert.FromBase64String(args[2]));
        switch (work)
        {
            case Func<object[], Task<int>> func:
                return await func(arguments);
            case Func<object[], Task> func:
                await func(arguments);
                return 0;
            default:
                throw new ArgumentException("The provided value is not a valid work function.");
        }
    }
}
