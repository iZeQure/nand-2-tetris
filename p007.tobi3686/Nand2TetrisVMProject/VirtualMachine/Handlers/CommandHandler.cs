using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using VirtualMachine.Core;

namespace VirtualMachine.Handlers
{
    public class CommandHandler
    {
        private Translator _translator = new();

        private const string LABEL_NAME = "MUSHROOM";        
        private const int MAPPED_TEMP_COMPILER = 5;
        private const int MAPPED_TEMPORARY_FREE = 13;
        private const int MAPPED_STATIC = 16; // Static values starts at index 16.
        private int _labelStartIndex = 33; // Just a random number, could've been zero.

        public List<string> PushCommand(string vmCommand)
        {
            string[] commands = CommandHelper.SplitCommand(vmCommand);

            var data = new List<string>();

            switch (commands[1])
            {
                case "":
                    /* Translate a  command. Location is RAM[117]. */
                    data.AddRange(new string[] { $"@{MAPPED_STATIC + commands[2]}", "D=A" });
                    break;
                case "constant":
                    data.AddRange(new string[] { $"@{commands[2]}", "D=A" });
                    break;
                case "local":
                case "argument":
                case "this":
                case "that":
                case "pointer":
                    var mappedCommand = CommandHelper.MapCommandArgument(commands[1], commands[2]);

                    if (string.IsNullOrEmpty(mappedCommand))
                    {
                        data.AddRange(new string[] { "Coudln't Handle Command Argument =>", vmCommand });
                        break;
                    }

                    data.AddRange(new string[] { mappedCommand, "D=M", $"@{commands[2]}", "A=D+A", "D=M" });
                    break;
                case "temp":
                    return TempCommand(commands[0], commands[2]);
                default:
                    /* Throw an exception or something. */
                    data.AddRange(new string[] { "Couldn't Handle Command =>", vmCommand });
                    break;
            }

            //var data = new List<string> { $"@{splitCommand[2]}", "D=A" };
            data.AddRange(_translator.TranslateMemoryUpdateCommand());
            data.AddRange(_translator.TranslateStackPointerCommand());

            return data;
        }

        public List<string> PopCommand(string vmCommand)
        {
            string[] commands = CommandHelper.SplitCommand(vmCommand);

            var data = new List<string>();

            switch (commands[1])
            {
                case "":
                    data.AddRange(new string[] { $"@{MAPPED_STATIC + commands[2]}", "D=A", $"@R{MAPPED_TEMPORARY_FREE}", "M=D" });
                    break;
                case "local":
                case "argument":
                case "this":
                case "that":
                case "pointer":
                    var mappedCommand = CommandHelper.MapCommandArgument(commands[1], commands[2]);

                    if (string.IsNullOrEmpty(mappedCommand))
                    {
                        data.AddRange(new string[] { "Coudln't Handle Command Argument =>", vmCommand });
                        break;
                    }

                    data.AddRange(new string[] { mappedCommand, "D=M", $"@{commands[2]}", "D=D+A", $"@R{MAPPED_TEMPORARY_FREE}", "M=D" });
                    break;
                case "temp":
                    return TempCommand(commands[0], commands[2]);
                default:
                    break;
            }

            data.AddRange(_translator.TranslateStackPointerCommand(false, "AM", "D=M"));
            //data.AddRange(new string[] { "@R13", "A=M", "M=D" });
            data.AddRange(_translator.TranslateMemoryUpdateCommand("@R13"));

            return data;
        }

        /// <summary>
        /// Adds two values together.
        /// </summary>
        /// <returns>A <see cref="List{T}"/> of instructions in assembly format.</returns>
        public List<string> AddCommand()
        {
            /* Example Add. 
             * @SP
             * AM=M-1
             * D=M
             * A=A+-1
             * M=M+D
             */

            var result = new List<string>();

            var getFirstValueInMemory = _translator.TranslateStackPointerCommand(
                increment: false,
                argLeftOfEqualSign: "AM",
                argRightOfEqualSign: "M",
                "D=M");

            var getSecondValueInMemory = _translator.TranslateStackPointerCommand(
                increment: false,
                argLeftOfEqualSign: "A",
                argRightOfEqualSign: "A");

            var addValues = "M=M+D";

            result.AddRange(getFirstValueInMemory);
            result.AddRange(getSecondValueInMemory);
            result.Add(addValues);

            return result;
        }

        public List<string> SubCommand()
        {
            return _translator.TranslateStackPointerCommand(false, "AM", "D=M", _translator.TranslateStackPointerPositionCommand(), "M=M-D");
        }

        public List<string> NegCommand()
        {
            var data = new List<string> { "D=0" };
            data.AddRange(_translator.TranslateStackPointerCommand(false, "A", "M=M-D"));
            return data;
        }

        public List<string> AndCommand()
        {
            return _translator.TranslateStackPointerCommand(false, "AM", "D=M", _translator.TranslateStackPointerPositionCommand(), "M=M&D");
        }

        public List<string> OrCommand()
        {
            return _translator.TranslateStackPointerCommand(false, "AM", "D=M", _translator.TranslateStackPointerPositionCommand(), "M=M|D");
        }

        public List<string> NotCommand()
        {
            return _translator.TranslateStackPointerCommand(false, "A", "M=!M");
        }

        public List<string> JmpCommand(string jmpCondition)
        {
            var data = new List<string>();

            string jumper = "";

            switch (jmpCondition)
            {
                case "eq":
                    jumper = "D;JNE";
                    break;

                case "lt":
                    jumper = "D;JGE";
                    break;

                case "gt":
                    jumper = "D;JLE";
                    break;
                default:
                    break;
            }

            data.AddRange(
                /* Get to Stack Pointer Location. Count the result. */
                _translator.TranslateStackPointerCommand(
                    false,
                    "AM",
                    "D=M", _translator.TranslateStackPointerPositionCommand(), "D=M-D"));

            /* Add Failure Reference Label. */
            data.Add($"@{LABEL_NAME}_FALSE{_labelStartIndex}");
            /* Add Jump Condition. */
            data.Add(jumper);

            /* Update the Stack Pointer. */
            data.AddRange(_translator.TranslateStackPointerCommand(false, "A", "M=-1"));

            /* Add Continue Reference Label. */
            data.Add($"@{LABEL_NAME}_CONTINUE{_labelStartIndex}");
            /* Add Jump Condition. */
            data.Add("0;JMP");

            /* Add False Label. */
            data.Add($"({LABEL_NAME}_FALSE{_labelStartIndex})");

            /* Update Stack Pointer Reference */
            data.AddRange(_translator.TranslateStackPointerCommand(false, "A", "M=0"));

            /* Add Continue Label. */
            data.Add($"({LABEL_NAME}_CONTINUE{_labelStartIndex})");

            _labelStartIndex++;
            return data;
            //data.AddRange(new string[] { $"@{LABEL_NAME}_FALSE{LABEL_START_INDEX}", "D;JNE" });
        }

        public List<string> TempCommand(string type, string index)
        {
            int temp = int.Parse(index) + MAPPED_TEMP_COMPILER;
            var temporaryData = new List<string> { $"@R{MAPPED_TEMP_COMPILER}", "D=M", $"@R{temp}" };
            switch (type)
            {
                case "pop":
                    temporaryData.Add("D=D+A");
                    temporaryData.Add($"@R{MAPPED_TEMPORARY_FREE}");
                    temporaryData.Add($"M=D");
                    temporaryData.AddRange(_translator.TranslateStackPointerCommand(false, "AM", "D=M"));
                    temporaryData.AddRange(_translator.TranslateMemoryUpdateCommand($"@R{MAPPED_TEMPORARY_FREE}"));
                    //temporaryData.Add($"@R{MAPPED_TEMPORARY_FREE}");
                    //temporaryData.Add("M=D");
                    //temporaryData.AddRange(TranslateStackPointerCounterCommand(false, "AM", "D=M", $"@R{MAPPED_TEMPORARY_FREE}", "A=M", "M=D"));
                    break;
                case "push":
                    temporaryData.Add("A=D+A");
                    temporaryData.Add("D=M");
                    temporaryData.AddRange(_translator.TranslateMemoryUpdateCommand());
                    //temporaryData.AddRange(TranslateStackPointerCounterCommand(false, "A", "M=D"));
                    temporaryData.AddRange(_translator.TranslateStackPointerCommand());
                    //temporaryData.Add("D=M");
                    //temporaryData.AddRange(TranslateMemoryUpdateCommand());
                    //temporaryData.AddRange(TranslateStackPointerCounterCommand());
                    break;
            }

            return temporaryData;
        }
    }
}
