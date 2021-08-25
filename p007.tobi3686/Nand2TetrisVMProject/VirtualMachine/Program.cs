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
        public const string POINTER_PREFIX = "@SP";
        public const int SLEEPTIMER_MS = 200;

        static void Main(string[] args)
        {
            Console.WriteLine("Loading some VM Data..");
            string path = @"A:\Nand2Tetris\nand2tetris\projects\07\StackArithmetic\SimpleAdd\SimpleAdd.vm";
            string outputPath = @"C:\Users\Tobias Rosenvinge\Documents\GitHub\repos\nand-2-tetris\tests\p007.tests\";

            var vmFile = TruncateLoadedFile(File.ReadAllLines(path));

            Console.WriteLine("Loaded VM Data.");

            List<string> assemblyCode = new();

            Console.WriteLine("Generating Assembly Code.");
            foreach (string vmCommand in vmFile)
            {
                var vmCommandPrefix = vmCommand.Split(" ");

                if (ContainsCommandPrefix(vmCommand, vmCommandPrefix[0]))
                {
                    switch (vmCommandPrefix[0])
                    {
                        case "push":
                            assemblyCode.AddRange(PushCommand(vmCommand));
                            continue;
                        case "add":
                            assemblyCode.AddRange(AddCommand());
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
            File.WriteAllLines($"{outputPath}SimpleAdd.asm", assemblyCode);
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
            string[] splitCommand = vmCommand.Split(" ");
            var data = new List<string> { $"@{splitCommand[2]}", "D=A" };
            data.AddRange(TranslateStackPointerMemoryUpdateCommand());
            data.AddRange(TranslateStackPointerCounterCommand());

            return data;
        }

        /// <summary>
        /// Translates an Add Command into assembly code.
        /// </summary>
        /// <returns>A <see cref="List{T}"/> of instructions in assembly format.</returns>
        private static List<string> AddCommand()
        {
            var data = new List<string>();
            data.AddRange(TranslateStackPointerCounterCommand(false, "AM"));
            data.AddRange(new string[] {
                "D=M",
                TranslateQuickStackPointerCounterCommand(), 
                "M=M+D" 
            });

            return data;
        }

        /// <summary>
        /// Translates the Stack Pointer Location.
        /// </summary>
        /// <remarks>
        /// Example:
        ///     <br>@SP</br>
        ///     <br>A=M</br>    
        ///     <br>M=D</br>
        /// </remarks>
        /// <returns>A <see cref="List{T}"/> of instructions in assembly format.</returns>
        private static List<string> TranslateStackPointerMemoryUpdateCommand()
        {
            Console.WriteLine($"Updating Stack Pointer Memory");

            Thread.Sleep(SLEEPTIMER_MS);

            return new List<string> { POINTER_PREFIX, "A=M", "M=D" };
        }

        /// <summary>
        /// Translates the Stack Pointer Counter Up or Down.
        /// </summary>
        /// <param name="increment">Whether the stack pointer has to increment or decrement, default is true.</param>
        /// <param name="specialRegister">A Special command to define where data is stored.</param>
        /// <returns>A <see cref="List{T}"/> of instructions in assembly format.</returns>
        private static List<string> TranslateStackPointerCounterCommand(bool increment = true, string specialRegister = "")
        {
            Console.WriteLine($"Moving Stack Pointer <{(increment ? "Down ↓" : "Up ↑")}>");
            Thread.Sleep(SLEEPTIMER_MS);

            string specialCommand = string.IsNullOrEmpty(specialRegister) ? "M" : specialRegister;

            return new List<string> { POINTER_PREFIX, increment ? $"{specialCommand}=M+1" : $"{specialCommand}=M-1" };
        }

        /// <summary>
        /// Translates the Stack Pointer quickly directly on the A registry.
        /// </summary>
        /// <param name="increment">Whether the stack pointer has to increment or decrement, default false.</param>
        /// <returns>A <see cref="string"/> containing the instruction in assembly format.</returns>
        private static string TranslateQuickStackPointerCounterCommand(bool increment = false)
        {
            Console.WriteLine($"Moving Stack Pointer <{(increment ? "Down ↓" : "Up ↑")}>");
            Thread.Sleep(SLEEPTIMER_MS);
            return increment ? "A=A+1" : "A=A-1";
        }

        private static bool ContainsCommandPrefix(string lineOfText, string prefix)
        {
            return lineOfText.Contains(prefix);
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
    }
}
