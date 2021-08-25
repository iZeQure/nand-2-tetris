using System;
using System.Collections.Generic;
using System.Linq;

namespace _16BitAssembler
{
    static class Parser
    {
        public static string Comp(this IFileReader fileReader, int index)
        {
            string comp = fileReader.FileText.ElementAt(index).Split("=")[1].Split(";")[0];

            return comp;
        }

        public static string Dest(this IFileReader fileReader, int index)
        {
            string dest = fileReader.FileText.ElementAt(index).Split("=")[0];

            return dest;
        }

        public static string Jump(this IFileReader fileReader, int index)
        {
            char splitter = ';';

            if (fileReader.FileText.ElementAt(index).Contains(splitter))
            {
                var element = fileReader.FileText.ElementAt(index);
                var split = element.Split(splitter);
                string jumper = split.LastOrDefault();

                return jumper;
            }

            return "null";
        }

        public static bool IsAInstruction(this IFileReader fileReader, int index)
        {
            var line = fileReader.FileText.ElementAt(index);

            if (line.Contains("@"))
            {
                return true;
            }

            return false;
        }

        public static bool IsCInstruction(this IFileReader fileReader, int index)
        {
            var line = fileReader.FileText.ElementAt(index);

            if (line.Contains("="))
            {
                return true;
            }

            return false;
        }

        public static bool IsJumping(this IFileReader fileReader, int index)
        {
            var line = fileReader.FileText.ElementAt(index);

            if (line.Contains(";"))
            {
                return true;
            }

            return false;
        }
    }
}
