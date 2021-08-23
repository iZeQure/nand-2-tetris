using System;

namespace Assembler.Handlers
{
    public class Parser
    {
        private readonly Converter _converter = new();

        public (bool, string[]) GetInstructionSetValues(string data, string instruction)
        {
            switch (instruction)
            {
                case "@":
                    if (ContainsInstruction(data, instruction))
                    {
                        var splitData = data.Split(instruction)[1];
                        var isParsed = int.TryParse(splitData, out int value);
                        string binaryResult = _converter.ConvertAInstruction(value);

                        return (isParsed, new string[] { binaryResult });
                    }

                    return (false, Array.Empty<string>());

                case ";":
                    if (ContainsInstruction(data, instruction))
                    {
                        var splitData = data.Split(instruction);
                        var binaryDest = _converter.ConvertDestInstruction("null");
                        var binaryComp = _converter.ConvertCompInstruction(splitData[0]);
                        var binaryJump = _converter.ConvertJumpInstruction(splitData[1]);
                        

                        return (true, new string[] { binaryComp, binaryDest, binaryJump });
                    }

                    return (false, Array.Empty<string>());
                case "=":
                    if (ContainsInstruction(data, instruction))
                    {
                        var splitData = data.Split(instruction);
                        var binaryDest = _converter.ConvertDestInstruction(splitData[0]);
                        var binaryComp = _converter.ConvertCompInstruction(splitData[1]);
                        var binaryJmp = _converter.ConvertJumpInstruction("null");

                        return (true, new string[] { binaryComp, binaryDest, binaryJmp });
                    }

                    return (false, Array.Empty<string>());
                default:
                    throw new Exception("Yo, that's some magic. We don't support that type of language.");
            }
        }

        private static bool ContainsInstruction(string data, string instruction)
        {
            return data.Contains(instruction);
        }
    }
}
