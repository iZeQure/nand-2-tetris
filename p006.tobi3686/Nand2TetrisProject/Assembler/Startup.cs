using Assembler.Core;
using Assembler.Handlers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assembler
{
    public class Startup
    {
        public static void RunAssembler()
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
                string[] hackFile = fileManager.LoadFile(file);
                Console.WriteLine("File loaded! Generating machine code...");

                List<string> machineCode = new();
                Parser parser = new();

                /* Loop through every line in the hack file. */
                foreach (string line in hackFile)
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

                fileManager.WriteFile(@"C:\Users\iZeQure\Desktop\machine-code.txt", machineCode.ToArray());
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
        public const string FILE_BASE_PATH = @"E:\Nand2Tetris\nand2tetris\projects\06\";
    }
}
