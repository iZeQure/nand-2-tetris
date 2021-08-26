using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using System.Linq;
using System.Threading;

namespace VirtualMachine
{
    public class Program
    {
        public const string LABEL_NAME = "MUSHROOM";
        public const string POINTER_PREFIX = "@SP";
        public const int SLEEPTIMER_MS = 50;
        public const int MAPPED_TEMPORARY_FREE = 13;
        public const int MAPPED_STATIC = 16;

        private static int _labelStartIndex = 33;

        static void Main(string[] args)
        {
            Console.WriteLine("Loading some VM Data..");
            string path = @"A:\Nand2Tetris\nand2tetris\projects\07\MemoryAccess\BasicTest\BasicTest.vm";
            string outputPath = @"C:\Users\Tobias Rosenvinge\Documents\GitHub\repos\nand-2-tetris\tests\p007.tests\";

            var vmFile = TruncateLoadedFile(File.ReadAllLines(path));

            Console.WriteLine("Loaded VM Data.");

            List<string> assemblyCode = new();

            Console.WriteLine("Generating Assembly Code.");
            foreach (string vmCommand in vmFile)
            {
                var vmCommandPrefix = SplitVMCommand(vmCommand)[0];

                if (ContainsCommandPrefix(vmCommand, vmCommandPrefix))
                {
                    switch (vmCommandPrefix)
                    {
                        case "push":
                            assemblyCode.AddRange(PushCommand(vmCommand));
                            continue;
                        case "pop":
                            assemblyCode.AddRange(PopCommand(vmCommand));
                            continue;
                        case "add":
                            assemblyCode.AddRange(AddCommand());
                            continue;
                        case "sub":
                            assemblyCode.AddRange(SubCommand());
                            continue;
                        case "neg":
                            assemblyCode.AddRange(NegCommand());
                            continue;
                        case "and":
                            assemblyCode.AddRange(AndCommand());
                            continue;
                        case "or":
                            assemblyCode.AddRange(OrCommand());
                            continue;
                        case "not":
                            assemblyCode.AddRange(NotCommand());
                            continue;
                        case "eq":
                        case "lt":
                        case "gt":
                            assemblyCode.AddRange(JmpCommand(vmCommandPrefix));
                            continue;
                        default:
                            Console.WriteLine("Command not Found!");
                            break;
                    }
                }

                //var containsPrefix = ContainsCommandPrefix(vmCommand, "push");

                //if (containsPrefix)
                //{
                //    var assemblyData = PushCommand(vmCommand);

                //    assemblyCode.AddRange(assemblyData);

                //    continue;
                //}
            }

            Console.WriteLine($"Writing Output to {outputPath}");
            File.WriteAllLines($"{outputPath}basictest.test.asm", assemblyCode);
            Console.WriteLine("Done.");
        }

        /// <summary>
        /// Translates a Push Command into assembly code.
        /// </summary>
        /// <remarks>
        /// Example: 
        /// </remarks>
        /// <param name="vmCommand">The VM data to push to the memory.</param>
        /// <returns>A <see cref="List{T}"/> of instructions in assembly format.</returns>
        private static List<string> PushCommand(string vmCommand)
        {
            string[] commands = SplitVMCommand(vmCommand);

            var data = new List<string>();

            switch (commands[1])
            {
                case "static":
                    /* Translate a static command. Location is RAM[117]. */
                    data.AddRange(new string[] { $"@{MAPPED_STATIC + commands[2]}", "D=A" });
                    break;
                case "constant":
                    data.AddRange(new string[] { $"@{commands[2]}", "D=A" });
                    break;
                case "local":
                case "argument":
                case "this":
                case "that":
                    data.AddRange(new string[] { $"@{commands[1]}", "D=M", $"@{commands[2]}", "A=D+A", "D=M" });
                    break;
                default:
                    /* Throw an exception or something. */
                    data.AddRange(new string[] { "Couldn't Handle Command =>", vmCommand });
                    break;
            }

            //var data = new List<string> { $"@{splitCommand[2]}", "D=A" };
            data.AddRange(TranslateMemoryUpdateCommand());
            data.AddRange(TranslateStackPointerCounterCommand());

            return data;
        }

        private static List<string> PopCommand(string vmCommand)
        {
            string[] commands = SplitVMCommand(vmCommand);

            var data = new List<string>();

            switch (commands[1])
            {
                case "static":
                    data.AddRange(new string[] { $"@{MAPPED_STATIC + commands[2]}", "D=A", $"@R{MAPPED_TEMPORARY_FREE}", "M=D" });
                    break;
                case "local":
                case "argument":
                case "this":
                case "that":
                    data.AddRange(new string[] { $"@{commands[1]}", "D=A", $"@{commands[2]}", "D=D+A", $"R{MAPPED_TEMPORARY_FREE}", "M=D" });
                    break;
                default:
                    break;
            }

            data.AddRange(TranslateStackPointerCounterCommand(false, "AM", "D=M"));
            //data.AddRange(new string[] { "@R13", "A=M", "M=D" });
            data.AddRange(TranslateMemoryUpdateCommand("@R13"));

            return data;
        }

        /// <summary>
        /// Translates an Add Command into assembly code.
        /// </summary>
        /// <returns>A <see cref="List{T}"/> of instructions in assembly format.</returns>
        private static List<string> AddCommand()
        {
            //var data = new List<string>();
            //data.AddRange(TranslateStackPointerCounterCommand(false, "AM"));
            //data.AddRange(new string[] {
            //    "D=M",
            //    TranslateQuickStackPointerCounterCommand(), 
            //    "M=M+D" 
            //});

            //return data;

            return TranslateStackPointerCounterCommand(false, "AM", "D=M", TranslateQuickStackPointerCounterCommand(), "M=M+D");
        }

        private static List<string> SubCommand()
        {
            return TranslateStackPointerCounterCommand(false, "AM", "D=M", TranslateQuickStackPointerCounterCommand(), "M=M-D");
        }

        private static List<string> NegCommand()
        {
            var data = new List<string> { "D=0" };
            data.AddRange(TranslateStackPointerCounterCommand(false, "A", "M=M-D"));
            return data;
        }

        private static List<string> AndCommand()
        {
            return TranslateStackPointerCounterCommand(false, "AM", "D=M", TranslateQuickStackPointerCounterCommand(), "M=M&D");
        }

        private static List<string> OrCommand()
        {
            return TranslateStackPointerCounterCommand(false, "AM", "D=M", TranslateQuickStackPointerCounterCommand(), "M=M|D");
        }

        private static List<string> NotCommand()
        {
            return TranslateStackPointerCounterCommand(false, "A", "M=!M");
        }

        private static List<string> JmpCommand(string jmpCondition)
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
                TranslateStackPointerCounterCommand(
                    false, 
                    "AM", 
                    "D=M", TranslateQuickStackPointerCounterCommand(), "D=M-D"));

            /* Add Failure Reference Label. */
            data.Add($"@{LABEL_NAME}_FALSE{_labelStartIndex}");
            /* Add Jump Condition. */
            data.Add(jumper);

            /* Update the Stack Pointer. */
            data.AddRange(TranslateStackPointerCounterCommand(false, "A", "M=-1"));

            /* Add Continue Reference Label. */
            data.Add($"@{LABEL_NAME}_CONTINUE{_labelStartIndex}");
            /* Add Jump Condition. */
            data.Add("0;JMP");

            /* Add False Label. */
            data.Add($"({LABEL_NAME}_FALSE{_labelStartIndex})");

            /* Update Stack Pointer Reference */
            data.AddRange(TranslateStackPointerCounterCommand(false, "A", "M=0"));

            /* Add Continue Label. */
            data.Add($"({LABEL_NAME}_CONTINUE{_labelStartIndex})");

            _labelStartIndex++;
            return data;
            //data.AddRange(new string[] { $"@{LABEL_NAME}_FALSE{LABEL_START_INDEX}", "D;JNE" });
        }

        /// <summary>
        /// Updates a set memory sector. Default is @SP.
        /// </summary>
        /// <remarks>
        /// Example:
        ///     <br>@SP</br>
        ///     <br>A=M</br>    
        ///     <br>M=D</br>
        /// </remarks>
        /// <returns>A <see cref="List{T}"/> of instructions in assembly format.</returns>
        private static List<string> TranslateMemoryUpdateCommand(string sector = "")
        {
            Console.WriteLine($"Updating Stack Pointer Memory");

            Thread.Sleep(SLEEPTIMER_MS);

            return new List<string> { string.IsNullOrEmpty(sector) ? POINTER_PREFIX : sector, "A=M", "M=D" };
        }

        /// <summary>
        /// Translates the Stack Pointer Counter Up or Down.
        /// </summary>
        /// <param name="increment">Whether the stack pointer has to increment or decrement, default is true.</param>
        /// <param name="specialRegister">A Special command to define where data is stored.</param>
        /// <returns>A <see cref="List{T}"/> of instructions in assembly format.</returns>
        private static List<string> TranslateStackPointerCounterCommand(bool increment = true, string specialRegister = "", params string[] extraCommands)
        {
            Console.WriteLine($"Moving Stack Pointer <{(increment ? "Down ↓" : "Up ↑")}>");
            Thread.Sleep(SLEEPTIMER_MS);

            string specialCommand = string.IsNullOrEmpty(specialRegister) ? "M" : specialRegister;
            var data = new List<string> { POINTER_PREFIX, increment ? $"{specialCommand}=M+1" : $"{specialCommand}=M-1" };

            if (extraCommands.Length != 0)
            {
                data.AddRange(extraCommands);
            }

            return data;
        }

        /// <summary>
        /// Translates the Stack Pointer quickly directly on the A registry.
        /// </summary>
        /// <param name="increment">Whether the stack pointer has to increment or decrement, default false.</param>
        /// <returns>A <see cref="string"/> containing the instruction in assembly format.</returns>
        private static string TranslateQuickStackPointerCounterCommand(bool increment = false, string specialRegister = "")
        {
            Console.WriteLine($"Moving Stack Pointer <{(increment ? "Down ↓" : "Up ↑")}>");
            Thread.Sleep(SLEEPTIMER_MS);

            string specialCommand = string.IsNullOrEmpty(specialRegister) ? "A" : specialRegister;

            return increment ? $"A={specialCommand}+1" : $"A={specialCommand}-1";
        }        

        /// <summary>
        /// Removes comments, then trims the file.
        /// </summary>
        /// <param name="file">A file containing data be to truncated.</param>
        /// <returns>A clean new file as a <see cref="List{T}"/>.</returns>
        private static List<string> TruncateLoadedFile(string[] file)
        {
            Console.WriteLine("Cleaning File");
            List<string> lines = new();

            for (int i = 0; i < file.Length; i++)
            {
                string line = Regex.Replace(file[i], "//" + ".+", string.Empty).Trim();

                if (string.IsNullOrEmpty(line))
                {
                    continue;
                }

                lines.Add(line);
            }

            return lines;
        }

        private static bool ContainsCommandPrefix(string vmCommand, string prefix)
        {
            return vmCommand.Contains(prefix);
        }

        private static string[] SplitVMCommand(string vmCommand)
        {
            return vmCommand.Split(" ");
        }
    }
}
