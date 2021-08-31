using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VirtualMachine.Helpers
{
    public class FileHelper
    {
        public static string GetFileName(string filePath)
        {
            return Path.GetFileName(filePath);
        }

        public static FileInfo GetFileInfo(string filePath)
        {
            return new FileInfo(filePath);
        }

        public static string ConvertFileSizeToReadableNumbers(long length)
        {
            return length switch
            {
                >= 1 << 30 => $"{length >> 30}Gb",
                >= 1 << 20 => $"{length >> 20}Mb",
                >= 1 << 10 => $"{length >> 10}Kb",
                _ => $"{length}B"
            };
        }
    }
}
