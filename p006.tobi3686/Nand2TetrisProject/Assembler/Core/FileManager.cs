using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Assembler.Core
{
    public class FileManager
    {
        public string[] LoadFile(string path)
        {
            var file = File.ReadAllLines(path);

            file = TruncateFile(file, "//");

            return file;
        }

        public void WriteFile(string path, string[] file)
        {
            File.WriteAllLines(path, file);
        }

        private static string[] TruncateFile(string[] file, string delimiter)
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

            return lines.ToArray();
        }
    }
}
