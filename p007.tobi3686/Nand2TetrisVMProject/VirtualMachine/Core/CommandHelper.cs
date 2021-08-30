using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VirtualMachine.Core
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
    }
}
