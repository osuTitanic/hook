using System;
using System.Collections.Generic;
using System.IO;

namespace TitanicHook.GUI
{
    internal sealed class Extra
    {
        internal static readonly Uri ReleaseUrl = new Uri("https://osu.titanic.sh/release/");

        public string Filename { get; private set; }
        public string Checksum { get; private set; }
        public string Description { get; private set; }
        public string CheckFilename { get; private set; }

        private Extra(string[] fields)
        {
            Filename = fields[0];
            Checksum = fields[1];
            Description = fields[2].Replace('-', ' ').Trim();
            CheckFilename = fields[4].Replace('/', Path.DirectorySeparatorChar);
        }

        public override string ToString()
        {
            return Description;
        }

        public bool IsInstalled(string target)
        {
            return File.Exists(ClientInstaller.ResolvePathSafe(target, CheckFilename));
        }

        public static Extra[] Parse(string releaseData)
        {
            List<Extra> packages = new List<Extra>();

            foreach (string line in releaseData.Split('\n'))
            {
                string[] fields = line.Trim('\r').Split(new[] { ' ' }, 5);

                if (fields.Length < 5 || fields[4].Length == 0)
                    continue;

                string[] actions = fields[3].Split(',');
                bool isExtra = Array.Exists(actions, a => a == "extra");

                if (!isExtra)
                    continue;

                Extra package = new Extra(fields);
                ClientInstaller.ResolvePathSafe(AppDomain.CurrentDomain.BaseDirectory, package.Filename);
                ClientInstaller.ResolvePathSafe(AppDomain.CurrentDomain.BaseDirectory, package.CheckFilename);
                packages.Add(package);
            }

            return packages.ToArray();
        }
    }
}
