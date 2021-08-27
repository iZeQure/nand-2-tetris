using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using System.Linq;
using System.Threading;
using System.Diagnostics;

namespace VirtualMachine
{
    public class Program
    {
        public const string LABEL_NAME = "MUSHROOM";
        public const string POINTER_PREFIX = "@SP";
        public const int SLEEPTIMER_MS = 0;
        public const int MAPPED_TEMP_COMPILER = 5;
        public const int MAPPED_TEMPORARY_FREE = 13;
        public const int MAPPED_STATIC = 16;

        private static int _labelStartIndex = 33;

        static void Main(string[] args)
        {
            var directoryPath = @"A:\Nand2Tetris\nand2tetris\projects\07\";
            var outputDirectoryPath = @"C:\Users\Tobias Rosenvinge\Documents\GitHub\repos\nand-2-tetris\tests\p007.tests\";
            var assemblyData = new Dictionary<string, List<string>>();
            var benchmarks = new Dictionary<string, TimeSpan>();
            var sw = new Stopwatch();

            Console.WriteLine("Loading files from directory...");
            var directoryFiles = Directory.GetFiles(directoryPath, "*.vm", SearchOption.AllDirectories);

            if (!directoryFiles.Any())
            {
                Console.WriteLine("No files found in root path.");
                Environment.Exit(0);
            }

            Console.WriteLine("Directory files loaded. Transalting VM code . . .");

            foreach (var filePath in directoryFiles)
            {
                var fileName = Path.GetFileName(filePath);
                sw.Start();

                Console.WriteLine($"Loading {fileName} into memory..");
                var vmCode = TruncateLoadedFile(File.ReadAllLines(filePath));

                Console.WriteLine($"Generating assembly code from => {fileName}");
                var generatedAsmCode = GenerateAsmCode(vmCode);

                sw.Stop();
                benchmarks.Add(fileName, sw.Elapsed);
                sw.Reset();
                assemblyData.Add(fileName, generatedAsmCode);
            }

            var anyAssemblyData = assemblyData.Any();

            if (!anyAssemblyData)
            {
                Console.WriteLine("No VM code was translated into ASM code.");
                Environment.Exit(0);
            }

            Console.WriteLine($"Generating output sources in => {outputDirectoryPath}");
            foreach (var data in assemblyData)
            {
                var fileName = $"{data.Key}.test.asm";
                //File.WriteAllLines($"{outputPath}basictest.test.asm", assemblyCode);
                Console.WriteLine($"Writing ASM code to => {fileName}");

                File.WriteAllLines($"{outputDirectoryPath}{fileName}", data.Value);
            }

            if (benchmarks.Any())
            {
                Console.WriteLine($"Generating benchmark source in => {outputDirectoryPath}");

                using var file = new StreamWriter(outputDirectoryPath + "benchmarks.txt");

                foreach (var benchmark in benchmarks)
                {
                    file.WriteLine($"{benchmark.Key,-20} => {benchmark.Value}");
                }
            }

            Console.WriteLine("Exiting translator in 5 seconds . . .");
            Thread.Sleep(5000);
            Environment.Exit(0);
        }

        private static List<string> GenerateAsmCode(List<string> vmCode)
        {
            List<string> generatedAsmCode = new();

            foreach (var vmCommand in vmCode)
            {
                var vmCommandPrefix = SplitVMCommand(vmCommand)[0];

                if (ContainsCommandPrefix(vmCommand, vmCommandPrefix))
                {
                    switch (vmCommandPrefix)
                    {
                        case "push":
                            generatedAsmCode.AddRange(PushCommand(vmCommand));
                            continue;
                        case "pop":
                            generatedAsmCode.AddRange(PopCommand(vmCommand));
                            continue;
                        case "add":
                            generatedAsmCode.AddRange(AddCommand());
                            continue;
                        case "sub":
                            generatedAsmCode.AddRange(SubCommand());
                            continue;
                        case "neg":
                            generatedAsmCode.AddRange(NegCommand());
                            continue;
                        case "and":
                            generatedAsmCode.AddRange(AndCommand());
                            continue;
                        case "or":
                            generatedAsmCode.AddRange(OrCommand());
                            continue;
                        case "not":
                            generatedAsmCode.AddRange(NotCommand());
                            continue;
                        case "eq":
                        case "lt":
                        case "gt":
                            generatedAsmCode.AddRange(JmpCommand(vmCommandPrefix));
                            continue;
                        default:
                            Console.WriteLine("Command not Found!");
                            break;
                    }
                }
            }

            return generatedAsmCode;
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
                case "pointer":
                    var mappedCommand = MapCommandArgument(commands[1], commands[2]);

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
                case "pointer":
                    var mappedCommand = MapCommandArgument(commands[1], commands[2]);

                    if (string.IsNullOrEmpty(mappedCommand))
                    {
                        data.AddRange(new string[] { "Coudln't Handle Command Argument =>", vmCommand });
                        break;
                    }

                    data.AddRange(new string[] { mappedCommand, "D=M", $"@{commands[2]}", "D=D+A", $"@R{MAPPED_TEMPORARY_FREE}", "M=D" });
                    break;
                case "temp":
                    return TempCommand(commands[0] ,commands[2]);
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

        private static List<string> TempCommand(string type, string index)
        {
            int temp = int.Parse(index) + MAPPED_TEMP_COMPILER;
            var temporaryData = new List<string> { $"@R{MAPPED_TEMP_COMPILER}", "D=M", $"@R{temp}" };
            switch (type)
            {
                case "pop":
                    temporaryData.Add("D=D+A");
                    temporaryData.Add($"@R{MAPPED_TEMPORARY_FREE}");
                    temporaryData.Add($"M=D");
                    temporaryData.AddRange(TranslateStackPointerCounterCommand(false, "AM", "D=M"));
                    temporaryData.AddRange(TranslateMemoryUpdateCommand($"@R{MAPPED_TEMPORARY_FREE}"));
                    //temporaryData.Add($"@R{MAPPED_TEMPORARY_FREE}");
                    //temporaryData.Add("M=D");
                    //temporaryData.AddRange(TranslateStackPointerCounterCommand(false, "AM", "D=M", $"@R{MAPPED_TEMPORARY_FREE}", "A=M", "M=D"));
                    break;
                case "push":
                    temporaryData.Add("A=D+A");
                    temporaryData.Add("D=M");
                    temporaryData.AddRange(TranslateMemoryUpdateCommand());
                    //temporaryData.AddRange(TranslateStackPointerCounterCommand(false, "A", "M=D"));
                    temporaryData.AddRange(TranslateStackPointerCounterCommand());
                    //temporaryData.Add("D=M");
                    //temporaryData.AddRange(TranslateMemoryUpdateCommand());
                    //temporaryData.AddRange(TranslateStackPointerCounterCommand());
                    break;
            }
            
            return temporaryData;
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

        private static string MapCommandArgument(string arg, string offset)
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
