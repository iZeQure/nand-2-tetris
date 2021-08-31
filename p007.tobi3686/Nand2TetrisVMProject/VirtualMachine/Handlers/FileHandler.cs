using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using VirtualMachine.Enums;
using VirtualMachine.UI;
using VirtualMachine.Helpers;

namespace VirtualMachine.Handlers
{
    public class FileHandler
    {
        public string[] GetDirectoryFiles(string rootDirecotry, string pattern, SearchOption searchOption = SearchOption.AllDirectories)
        {
            return Directory.GetFiles(rootDirecotry, pattern, searchOption);
        }

        /// <summary>
        /// Writes all lines to a file using <see cref="File"/>.
        /// </summary>
        /// <param name="filePath"></param>
        /// <param name="data"></param>
        public void WriteFile(string filePath, List<string> data)
        {
            File.WriteAllLines(filePath, data);
        }

        /// <summary>
        /// Writes to file using <see cref="StreamWriter"/>.
        /// </summary>
        /// <param name="filePath">Path of where the file is located by full name.</param>
        /// <param name="data">Data to be written to the file.</param>
        /// <param name="headers">Represents the amount of columns in the file separated by known headers.</param>
        public void WriteFile(string filePath, List<string> data, string headers)
        {
            using var sw = new StreamWriter(filePath);
            sw.WriteLine(headers);

            foreach (var line in data)
            {
                sw.WriteLine(line);
            }
        }

        public List<string> LoadFile(string filePath)
        {
            ConsoleUIManager.Beautify(Color.Default, $"Reading {FileHelper.GetFileName(filePath)} from directory.");
            var file = File.ReadAllLines(filePath);

            return TruncateFile(file);
        }

        /// <summary>
        /// Removes unnecessary things from the file.
        /// </summary>
        /// <param name="fileText"></param>
        /// <returns>A <see cref="List{T}"/> with the clean file text.</returns>
        private List<string> TruncateFile(string[] fileText)
        {
            ConsoleUIManager.Beautify(0, "Cleaning file for comments and whitespaces . . .");

            List<string> lines = new();

            for (int i = 0; i < fileText.Length; i++)
            {
                string line = Regex.Replace(fileText[i], "//" + ".+", string.Empty).Trim();

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
