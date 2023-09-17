using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IronWhisperReceiver.Registry
{
    // A file resource that can either be accessed for information or modified via AP Operation
    public class RegFile : RegCore
    {
        public string Path;

        public RegFile()
        {
            Path = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
        }
    }
}
