using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IronWhisperReceiver.Registry
{
    public class RegProject : RegCore
    {
        public List<RegFile> Files;
        /// <summary>
        /// Path to root of project
        /// </summary>
        public string Path;
        public bool isGitRepository;
        public string Description;
        public List<string> Notes = new List<string>();

        public RegProject ()
        {
            Files = new List<RegFile>();
            Path = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            isGitRepository = false;
            Description = "This is a project";
            Notes = new List<string>();
        }
    }
}
