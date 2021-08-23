using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace _16BitAssembler
{
    class FileReader : IFileReader
    {
        private readonly string _path;
        private readonly CancellationTokenSource _tokenSource;
        private string[] _fileText;

        public string[] FileText { get { return _fileText; } }

        public FileReader(string path)
        {
            _path = path;
            _tokenSource = new();
        }

        public async Task<IFileReader> ReadAsync()
        {
            var readLines = await File.ReadAllLinesAsync(_path, _tokenSource.Token);

            _fileText = RemoveComments(readLines, "// ");

            return this;
        }

        public async Task WriteOutputAsync(string[] text)
        {
            await File.WriteAllLinesAsync("C:\\Users\\Tobias Rosenvinge\\Desktop\\binary-output.txt", text, _tokenSource.Token);
        }

        private static string[] RemoveComments(string[] text, string delimiter)
        {
            List<string> lines = new();

            for (int i = 0; i < text.Length; i++)
            {
                string line = Regex.Replace(text[i], delimiter + ".+", string.Empty).Trim();

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
