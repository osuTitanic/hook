using System;
using System.Collections.Generic;
using System.IO;

namespace TitanicHook.GUI
{
    internal sealed class Extra
    {
        internal static readonly Uri ReleaseUrl = new Uri("https://osu.titanic.sh/release/");

        public string Filename { get; }
        public string Checksum { get; }
        public string Description { get; }
        public string CheckFilename { get; }

        private Extra(string filename, string checksum, string description, string checkFilename)
        {
            Filename = filename;
            Checksum = checksum;
            Description = description;
            CheckFilename = checkFilename;
        }

        public override string ToString()
        {
            return Description;
        }

        public bool IsInstalled(string target)
        {
            return File.Exists(Installer.ResolvePathSafe(target, CheckFilename));
        }

        public static Extra[] Parse(string releaseData, string target)
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

                Extra package = new Extra(
                    filename: fields[0],
                    checksum: fields[1],
                    description: fields[2].Replace('-', ' ').Trim(),
                    checkFilename: fields[4].Replace('/', Path.DirectorySeparatorChar)
                );
                Installer.ResolvePathSafe(target, package.Filename);
                Installer.ResolvePathSafe(target, package.CheckFilename);
                packages.Add(package);
            }

            return packages.ToArray();
        }
    }
}
