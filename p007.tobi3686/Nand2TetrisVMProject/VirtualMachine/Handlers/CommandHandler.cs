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
                case "static":
                    return StaticPushCommand(commands[2]);
                case "constant":
                    data.AddRange(new string[] { $"@{commands[2]}", "D=A" });
                    break;
                case "local":
                case "argument":
                case "this":
                case "that":
                    var mappedCommand = CommandHelper.MapCommandArgument(commands[1], commands[2]);

                    if (string.IsNullOrEmpty(mappedCommand))
                    {
                        data.AddRange(new string[] { "Coudln't Handle Command Argument =>", vmCommand });
                        break;
                    }

                    data.AddRange(new string[] { mappedCommand, "D=M", $"@{commands[2]}", "A=D+A", "D=M" });
                    break;
                case "pointer":
                    var pointer = CommandHelper.MapCommandArgument(commands[1], commands[2]);

                    if (string.IsNullOrEmpty(pointer))
                    {
                        data.AddRange(new string[] { "Coudln't Handle Command Argument =>", vmCommand });
                        break;
                    }

                    data.AddRange(new string[] { pointer, "D=M" });
                    break;
                case "temp":
                    return TempPushCommand(commands[2]);
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
                case "static":
                    return StaticPopCommand(commands[2]);
                case "local":
                case "argument":
                case "this":
                case "that":
                    var mappedCommand = CommandHelper.MapCommandArgument(commands[1], commands[2]);

                    if (string.IsNullOrEmpty(mappedCommand))
                    {
                        data.AddRange(new string[] { "Coudln't Handle Command Argument =>", vmCommand });
                        break;
                    }

                    data.AddRange(new string[] { mappedCommand, "D=M", $"@{commands[2]}", "D=D+A", $"@R{MAPPED_TEMPORARY_FREE}", "M=D" });
                    break;
                case "pointer":
                    var pointer = CommandHelper.MapCommandArgument(commands[1], commands[2]);

                    if (string.IsNullOrEmpty(pointer))
                    {
                        data.AddRange(new string[] { "Coudln't Handle Command Argument =>", vmCommand });
                        break;
                    }

                    data.AddRange(new string[] { pointer, "D=A", $"@R{MAPPED_TEMPORARY_FREE}", "M=D" });
                    break;
                case "temp":
                    return TempPopCommand(commands[2]);
                default:
                    break;
            }

            data.AddRange(_translator.TranslateStackPointerCommand(false, "AM", "", "D=M"));
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
             * A=A-1
             * M=M+D
             */

            var result = new List<string>();

            var getFirstValueInMemory = _translator.TranslateStackPointerCommand(
                increment: false,
                argLeftOfEqualSign: "AM",
                argRightOfEqualSign: "M",
                "D=M", "A=A-1");

            var addValues = "M=M+D";

            result.AddRange(getFirstValueInMemory);
            result.Add(addValues);

            return result;
        }

        /// <summary>
        /// Substracts two values from one another.
        /// </summary>
        /// <returns>A <see cref="List{T}"/> of instructions in assembly format.</returns>
        public List<string> SubCommand()
        {
            /* Example Sub:
             * @SP
             * AM=M-1
             * D=M
             * A=A-1
             * M=M-D
             */

            var subtractCommand = _translator.TranslateStackPointerCommand(
                increment: false,
                argLeftOfEqualSign: "AM",
                argRightOfEqualSign: "M",
                "D=M", "A=A-1", "M=M-D");

            return subtractCommand;
        }

        /// <summary>
        /// Negates the most resent value.
        /// </summary>
        /// <returns>A <see cref="List{T}"/> of instructions in assembly format.</returns>
        public List<string> NegCommand()
        {
            /* Example Sub:
             * D=0
             * @SP
             * A=M-1
             * M=D-M
             */

            var result = new List<string>();

            var setDataReg = "D=0";
            var negateFirstValue = _translator.TranslateStackPointerCommand(
                increment: false,
                argLeftOfEqualSign: "A",
                argRightOfEqualSign: "M",
                extraCommands: "M=D-M");

            result.Add(setDataReg);
            result.AddRange(negateFirstValue);

            return result;
        }

        /// <summary>
        /// Produces the conjunction of two values.
        /// </summary>
        /// <returns>A <see cref="List{T}"/> of instructions in assembly format.</returns>
        public List<string> AndCommand()
        {
            /* Example:
             * @SP
             * AM=M-1
             * D=M
             * A=A-1
             * M=M&D
             */

            var conjunctionResult = _translator.TranslateStackPointerCommand(
                increment: false,
                argLeftOfEqualSign: "AM",
                argRightOfEqualSign: "M",
                "D=M", "A=A-1", "M=M&D");

            return conjunctionResult;
        }

        /// <summary>
        /// Produces the union of two values.
        /// </summary>
        /// <returns>A <see cref="List{T}"/> of instructions in assembly format.</returns>
        public List<string> OrCommand()
        {
            /* Example:
             *  @SP
             *  AM=M-1
             *  D=M
             *  A=A-1
             *  M=M|D
             */

            var unionResult = _translator.TranslateStackPointerCommand(
                increment: false,
                argLeftOfEqualSign: "AM",
                argRightOfEqualSign: "M",
                "D=M", "A=A-1", "M=M|D");

            return unionResult;
        }

        /// <summary>
        /// Produces the exact opposite of everything about it.
        /// </summary>
        /// <returns>A <see cref="List{T}"/> of instructions in assembly format.</returns>
        public List<string> NotCommand()
        {
            /* Example: 
             * @SP
             * A=M-1
             * M=!M
             */

            var exactOppositeResult = _translator.TranslateStackPointerCommand(
                increment: false,
                argLeftOfEqualSign: "A",
                argRightOfEqualSign: "M",
                "M=!M");

            return exactOppositeResult;
        }

        /// <summary>
        /// Produces the jump condition dependant on input.
        /// </summary>
        /// <param name="jmpCondition">Describes the jump condition.</param>
        /// <returns>A <see cref="List{T}"/> of instructions in assembly format.</returns>
        public List<string> JmpCommand(string jmpCondition)
        {
            var jumpResult = new List<string>();

            string jumpCondition = "";

            switch (jmpCondition)
            {
                case "eq":
                    jumpCondition = "D;JNE";
                    break;

                case "lt":
                    jumpCondition = "D;JGT";
                    break;

                case "gt":
                    jumpCondition = "D;JLT";
                    break;
                default:
                    break;
            }

            string falseLabel = $"{LABEL_NAME}_FALSE{_labelStartIndex}";
            string continueLabel = $"{LABEL_NAME}_CONTINUE{_labelStartIndex}";

            var evaluateFalseCondition = _translator.TranslateStackPointerCommand(
                increment: false,
                argLeftOfEqualSign: "AM",
                argRightOfEqualSign: "M",
                "D=M", "A=A-1", "D=M-D", $"@{falseLabel}", jumpCondition);

            var evaluateContinueCondition = _translator.TranslateStackPointerCommand(
                increment: false,
                argLeftOfEqualSign: "A",
                argRightOfEqualSign: "M",
                "M=-1", $"@{continueLabel}", "0;JMP", $"({falseLabel})");

            var moveNext = _translator.TranslateStackPointerCommand(
                increment: false,
                argLeftOfEqualSign: "A",
                argRightOfEqualSign: "M",
                "M=0", $"({continueLabel})");

            jumpResult.AddRange(evaluateFalseCondition);
            jumpResult.AddRange(evaluateContinueCondition);
            jumpResult.AddRange(moveNext);

            return jumpResult;
        }

        public List<string> TempPushCommand(string index)
        {
            var tempResult = new List<string>();

            var mapTempLocation = new string[] {
                $"@R{MAPPED_TEMP_COMPILER}",
                "D=M",
                $"@R{GetIndex(index, MAPPED_TEMP_COMPILER)}",
                "A=D+A",
                "D=M"
            };

            var updateMemory = _translator.TranslateMemoryUpdateCommand();
            var updateStackPointer = _translator.TranslateStackPointerCommand();

            tempResult.AddRange(mapTempLocation);
            tempResult.AddRange(updateMemory);
            tempResult.AddRange(updateStackPointer);

            return tempResult;
        }

        public List<string> TempPopCommand(string index)
        {
            var tempResult = new List<string>();

            var mapTempLocation = new string[] {
                $"@R{MAPPED_TEMP_COMPILER}",
                "D=M",
                $"@R{GetIndex(index, MAPPED_TEMP_COMPILER)}",
                "D=D+A",
                $"@R{MAPPED_TEMPORARY_FREE}",
                "M=D"
            };

            var moveNext = _translator.TranslateStackPointerCommand(
                increment: false,
                argLeftOfEqualSign: "AM",
                argRightOfEqualSign: "M",
                "D=M");

            var updateTempAddress = _translator.TranslateMemoryUpdateCommand($"@R{MAPPED_TEMPORARY_FREE}");

            tempResult.AddRange(mapTempLocation);
            tempResult.AddRange(moveNext);
            tempResult.AddRange(updateTempAddress);

            return tempResult;
        }

        public List<string> StaticPopCommand(string index)
        {
            var staticResult = new List<string>();

            var mapStaticAddress = new string[] {
                $"@{GetIndex(index, MAPPED_STATIC)}", "D=A", $"@R{MAPPED_TEMPORARY_FREE}", "M=D"
            };

            var moveNext = _translator.TranslateStackPointerCommand(
                increment: false,
                argLeftOfEqualSign: "AM",
                argRightOfEqualSign: "M",
                "D=M");

            var updateMemory = _translator.TranslateMemoryUpdateCommand($"@R{MAPPED_TEMPORARY_FREE}");

            staticResult.AddRange(mapStaticAddress);
            staticResult.AddRange(moveNext);
            staticResult.AddRange(updateMemory);

            return staticResult;
        }

        public List<string> StaticPushCommand(string index)
        {
            var staticResult = new List<string>();

            var mapStaticAddress = new string[] {
                $"@{GetIndex(index, MAPPED_STATIC)}", "D=M"
            };

            var updateMemory = _translator.TranslateMemoryUpdateCommand();
            var moveNext = _translator.TranslateStackPointerCommand();

            staticResult.AddRange(mapStaticAddress);
            staticResult.AddRange(moveNext);
            staticResult.AddRange(updateMemory);

            return staticResult;
        }

        /// <summary>
        /// Returns the index of the correct address.
        /// </summary>
        /// <param name="index"></param>
        /// <returns>An integer representing the address location.</returns>
        private int GetIndex(string index, int valueToMapWith)
        {
            return ParseIndexToInt32(index) + valueToMapWith;
        }

        /// <summary>
        /// Attempts to parse value to <see cref="int"/>.
        /// </summary>
        /// <param name="value">A value representing an integer as a string.</param>
        /// <returns>A parsed string to an integer otherwise 0.</returns>
        private int ParseIndexToInt32(string value)
        {
            return int.TryParse(value, out int result) ? result : 0;
        }
    }
}
