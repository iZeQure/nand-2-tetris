using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;

namespace Assembler.Core
{
    public class FileManager
    {
        public List<string> LoadFile(string path)
        {
            var file = File.ReadAllLines(path);

            var truncatedFile = TruncateFile(file, "//");

            return truncatedFile;
        }

        public void WriteFile(string path, string[] file)
        {
            File.WriteAllLines(path, file);
        }

        private static List<string> TruncateFile(string[] file, string delimiter)
        {
            List<string> lines = new();

            for (int i = 0; i < file.Length; i++)
            {
                string line = Regex.Replace(file[i], delimiter + ".+", string.Empty).Trim();

                if (string.IsNullOrEmpty(line))
                {
                    continue;
                }

                lines.Add(line);
            }

            return lines;
        }
    }
}
