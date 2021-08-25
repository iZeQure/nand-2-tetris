using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _16BitAssembler
{
    class Program
    {
        private const string FILE_BASE_PATH = "A:\\Nand2Tetris\\nand2tetris\\projects\\06\\";

        static async Task Main(string[] args)
        {
            Console.WriteLine($"Current Directory: {FILE_BASE_PATH}");

            Console.Write("Enter file path: ");
            string filePath = Console.ReadLine();

            filePath = Path.Combine(FILE_BASE_PATH, filePath);

            if (string.IsNullOrEmpty(filePath))
            {
                Environment.Exit(0);
            }

            Console.WriteLine($"Loading file.. {filePath}");
            IFileReader fileReader = new FileReader(filePath);
            IConverter converter = new BinaryConverter();

            try
            {
                var reader = await fileReader.ReadAsync();

                Console.WriteLine($"File is loaded!");

                List<string> binaryText = new();

                Console.WriteLine($"Compiling binary values..");

                for (int i = 0; i < reader.FileText.Length; i++)
                {
                    string dest = string.Empty;
                    string comp = string.Empty;
                    string jump = string.Empty;

                    if (reader.IsAInstruction(i))
                    {
                        var binaryInstruction = converter.AInstruction(reader.FileText[i][1..]);
                        binaryText.Add(binaryInstruction);

                        continue;
                    }

                    if (reader.IsCInstruction(i))
                    {
                        var values = fileReader.FileText.ElementAt(i).Split("=");
                        dest = converter.Dest(values[0]);
                        comp = converter.Comp(values[1]);
                        jump = converter.Jump("null");
                    }

                    if (reader.IsJumping(i))
                    {
                        var values = fileReader.FileText.ElementAt(i).Split(";");
                        dest = converter.Dest(values[0]);
                        jump = converter.Jump(values[1]);
                        comp = converter.Comp("null");
                    }

                    //var dest = converter.Dest(reader.Dest(i));
                    //var comp = converter.Comp(reader.Comp(i));
                    //var binaryJumper = converter.Jump(reader.Jump(i));

                    string binaryResult = $"111{comp}{dest}{jump}";
                    binaryText.Add(binaryResult);
                }

                Console.WriteLine($"Done. Writing Output file.");

                await reader.WriteOutputAsync(binaryText.ToArray());
                Console.WriteLine($"Successfully compiled binary output.");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }


            Console.ReadKey();
        }        
    }
}
