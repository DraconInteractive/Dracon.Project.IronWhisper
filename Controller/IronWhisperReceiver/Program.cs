using IronWhisper_CentralController.Core;

class Program
{
    static async Task Main(string[] args)
    {
        var core = new CoreSystem();
        await core.Run();
    }
}


