using IronWhisper_CentralController.Core;
using System.Windows.Forms;

class Program
{
    static async Task Main(string[] args)
    {
        var core = new CoreSystem();

        await core.Run();
    }
}


