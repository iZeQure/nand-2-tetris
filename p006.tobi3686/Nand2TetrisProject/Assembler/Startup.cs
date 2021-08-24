using Assembler.Core;
using Assembler.Handlers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Assembler
{
    public class Startup
    {
        public void RunAssembler()
        {
            Console.WriteLine($"Current Directory: {Helper.FILE_BASE_PATH}");
            Console.Write($"Specify File Location: ");
            string file = Console.ReadLine();

            if (string.IsNullOrEmpty(file))
            {
                Environment.Exit(0);
            }

            file = Path.Combine(Helper.FILE_BASE_PATH, file);

            Console.WriteLine($"Loading file {file} . . .");
            FileManager fileManager = new();

            try
            {
                List<string> hackFile = fileManager.LoadFile(file);
                Console.WriteLine("File loaded! Generating machine code...");

                List<string> machineCode = new();
                Parser parser = new();

                Console.WriteLine("Loading Labels . . .");
                /* Loop through whole file to find all labels. */
                for (int labelIndex = 0; labelIndex < hackFile.Count; labelIndex++)
                {
                    var result = parser.ContainsLabel(hackFile[labelIndex], "(");
                    if (result.Item1 && !string.IsNullOrEmpty(result.Item2))
                    {
                        //Console.WriteLine($"@{result.Item2} - {labelIndex}");
                        parser.AddInstructionToSymbolTable(result.Item2, $"{labelIndex}");
                        hackFile.RemoveAt(labelIndex);
                    }
                }

                Console.WriteLine("Loading symbols . . .");
                /* Loop through file to find unique symbols. */
                foreach (string symbol in hackFile)
                {
                    var containsSymbol = parser.ContainsSymbol(symbol);

                    if (!string.IsNullOrEmpty(containsSymbol))
                    {
                        parser.AddInstructionToSymbolTable(containsSymbol, "");
                    }
                }

                /* Loop through every line in the hack file. */
                foreach (string line in hackFile)
                //for (int i = 0; i < hackFile.Length; i++)
                {
                    var aInstructions = parser.GetInstructionSetValues(line, "@");

                    /* Find every A Instruction. */
                    if (aInstructions.Item1)
                    {
                        machineCode.Add(aInstructions.Item2[0]);
                        continue;
                    }

                    /* Find every C Instruction. */
                    var cInstructions = parser.GetInstructionSetValues(line, "=");
                    if (cInstructions.Item1)
                    {
                        string cInstruction = $"111{cInstructions.Item2[0]}{cInstructions.Item2[1]}{cInstructions.Item2[2]}";
                        machineCode.Add(cInstruction);
                        continue;
                    }

                    /* Find Jumpers. */
                    var jmpInstructions = parser.GetInstructionSetValues(line, ";");
                    if (jmpInstructions.Item1)
                    {
                        string jmpInstruction = $"111{jmpInstructions.Item2[0]}{jmpInstructions.Item2[1]}{jmpInstructions.Item2[2]}";
                        machineCode.Add(jmpInstruction);
                        continue;
                    }
                }

                Console.WriteLine($"Done. Writing Output file.");

                fileManager.WriteFile(Helper.FILE_OUTPUT_PATH, machineCode.ToArray());
                Console.WriteLine($"Successfully compiled binary output.");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

            Console.ReadKey();
        }
    }

    public static class Helper
    {
        public const string FILE_BASE_PATH = @"A:\Nand2Tetris\nand2tetris\projects\06\";
        //public const string FILE_BASE_PATH = @"E:\Nand2Tetris\nand2tetris\projects\06\";
        public const string FILE_OUTPUT_PATH = @"C:\Users\Tobias Rosenvinge\Desktop\machine-code.hack";
        //public const string FILE_OUTPUT_PATH = @"C:\Users\iZeQure\Desktop\machine-code.txt";
    }
}
