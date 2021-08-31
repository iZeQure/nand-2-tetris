using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VirtualMachine.Helpers
{
    public class CommandHelper
    {
        public static bool ContainsCommandPrefix(string command, string prefix)
        {
            return command.Contains(prefix);
        }

        public static string[] SplitCommand(string command)
        {
            return command.Split(" ");
        }

        public static string MapCommandArgument(string arg, string offset)
        {
            if (arg.Equals("pointer"))
            {
                return offset.Equals("0") ? "@THIS" : "@THAT";
            }

            return arg switch
            {
                "local" => "@LCL",
                "argument" => "@ARG",
                "this" => "@THIS",
                "that" => "@THAT",
                _ => "",
            };
        }

        /// <summary>
        /// Maps the index for the current memory position.
        /// </summary>
        /// <param name="index">The index to indent with.</param>
        /// <param name="valueToMapWith">Start position of the index in memory.</param>
        /// <returns>An integer representing the index position for the giving arguments.</returns>
        public static int MapIndex(string index, int valueToMapWith)
        {
            int i = int.TryParse(index, out int result) ? result : 0;
            i += valueToMapWith;

            return i;
        }
    }
}
