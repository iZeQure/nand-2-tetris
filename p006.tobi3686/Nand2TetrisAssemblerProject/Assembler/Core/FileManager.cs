using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Assembler.Core
{
    public class FileManager
    {
        public async Task<List<string>> LoadFileAsync(string path)
        {
            var file = await File.ReadAllLinesAsync(path);

            var truncatedFile = TruncateFile(file, "//");

            return truncatedFile;
        }

        public async Task WriteFileAsync(string path, string[] file)
        {
            await File.WriteAllLinesAsync(path, file);
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
