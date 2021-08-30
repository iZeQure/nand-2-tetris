using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using System.Linq;
using System.Threading;
using System.Diagnostics;
using VirtualMachine.Handlers;
using VirtualMachine.Core;

namespace VirtualMachine
{
    public class Program
    {
        private static readonly CommandHandler _commandHandler = new();

        static void Main(string[] args)
        {
            /* Initialize Variables */
            var directoryPath = @"A:\Nand2Tetris\nand2tetris\projects\07\";
            var outputDirectoryPath = @"C:\Users\Tobias Rosenvinge\Documents\GitHub\repos\nand-2-tetris\tests\p007.tests\";
            var assemblyData = new Dictionary<string, List<string>>();
            var benchmarks = new Dictionary<string, Benchmark>();
            var sw = new Stopwatch();

            Console.WriteLine("Loading files from directory...");

            /* Load VM files from the directory and sub directories. */
            var directoryFiles = Directory.GetFiles(directoryPath, "*.vm", SearchOption.AllDirectories);

            /* Check for any files.
             * If no files, exit program, otherwise continue flow. 
             */
            if (!directoryFiles.Any())
            {
                Console.WriteLine("No files found in root path.");
                Environment.Exit(0);
            }


            /* Interate through VM data files. */
            Console.WriteLine("Directory files loaded. Transalting VM code . . .");
            foreach (var filePath in directoryFiles)
            {
                var fileName = Path.GetFileName(filePath);
                sw.Start();

                // Remove whitespaces and comments in the files, when loading all lines.
                Console.WriteLine($"Loading {fileName} into memory..");
                var vmCode = TruncateLoadedFile(File.ReadAllLines(filePath));

                // Generate assembly code from clean VM file.
                Console.WriteLine($"Generating assembly code from => {fileName}");
                var generatedAsmCode = GenerateAsmCode(vmCode);

                sw.Stop();
                // Add benchmark to the dictionary.
                benchmarks.Add(fileName, new Benchmark(sw.Elapsed, "", ""));
                sw.Reset();

                // Add assembly data to dictionary.
                assemblyData.Add(fileName, generatedAsmCode);
            }

            /* Check for any assembly data.
             * If none is found, then exit program, otherwise continue flow. 
             */
            var anyAssemblyData = assemblyData.Any();
            if (!anyAssemblyData)
            {
                Console.WriteLine("No VM code was translated into ASM code.");
                Environment.Exit(0);
            }

            /* Interate through the assembly data
             * Write lines to output text files in the output directory. 
             */
            Console.WriteLine($"Generating output sources in => {outputDirectoryPath}");
            foreach (var data in assemblyData)
            {
                var fileName = $"{outputDirectoryPath}{data.Key}.test.asm";
                //File.WriteAllLines($"{outputPath}basictest.test.asm", assemblyCode);
                Console.WriteLine($"Writing ASM code to => {fileName}");

                File.WriteAllLines(fileName, data.Value);


                // Get file info for benchmark.
                FileInfo info = new(fileName);
                benchmarks[data.Key].FileSize = ConvertFileSizeToReadableNumbers(info.Length);
                benchmarks[data.Key].Location = info.FullName;
            }

            /* Check for any created benchmark
             * If any benchmarks, iterate the benchmark collection then write output to directory, otherwise skip. 
             */
            if (benchmarks.Any())
            {
                Console.WriteLine($"Generating benchmark source in => {outputDirectoryPath}");

                using var file = new StreamWriter(outputDirectoryPath + "benchmarks.txt");

                file.WriteLine($"{"Name", -20} {"Time", -20} {"Size", -5} {"Location", -20}");
                foreach (var benchmark in benchmarks)
                {
                    file.WriteLine($"{benchmark.Key, -20} {benchmark.Value.Time, -20} {benchmark.Value.FileSize, -5} {benchmark.Value.Location, -20}");
                }
            }

            Console.WriteLine("Exiting translator in 5 seconds . . .");
            Thread.Sleep(5000);
            Environment.Exit(0);
        }

        private static string ConvertFileSizeToReadableNumbers(long length)
        {
            return length switch
            {
                >= 1 << 30 => $"{length >> 30}Gb",
                >= 1 << 20 => $"{length >> 20}Mb",
                >= 1 << 10 => $"{length >> 10}Kb",
                _ => $"{length}B"
            };
        }

        private static List<string> GenerateAsmCode(List<string> vmCode)
        {
            List<string> generatedAsmCode = new();

            foreach (var vmCommand in vmCode)
            {
                var vmCommandPrefix = CommandHelper.SplitCommand(vmCommand)[0];

                if (CommandHelper.ContainsCommandPrefix(vmCommand, vmCommandPrefix))
                {
                    switch (vmCommandPrefix)
                    {
                        case "push":
                            generatedAsmCode.AddRange(_commandHandler.PushCommand(vmCommand));
                            continue;
                        case "pop":
                            generatedAsmCode.AddRange(_commandHandler.PopCommand(vmCommand));
                            continue;
                        case "add":
                            generatedAsmCode.AddRange(_commandHandler.AddCommand());
                            continue;
                        case "sub":
                            generatedAsmCode.AddRange(_commandHandler.SubCommand());
                            continue;
                        case "neg":
                            generatedAsmCode.AddRange(_commandHandler.NegCommand());
                            continue;
                        case "and":
                            generatedAsmCode.AddRange(_commandHandler.AndCommand());
                            continue;
                        case "or":
                            generatedAsmCode.AddRange(_commandHandler.OrCommand());
                            continue;
                        case "not":
                            generatedAsmCode.AddRange(_commandHandler.NotCommand());
                            continue;
                        case "eq":
                        case "lt":
                        case "gt":
                            generatedAsmCode.AddRange(_commandHandler.JmpCommand(vmCommandPrefix));
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
