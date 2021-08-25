using System;

namespace Assembler.Handlers
{
    public class Parser
    {
        private readonly Converter _converter = new();

        public void AddInstructionToSymbolTable(string label, string value)
        {
            _converter.StoreInstruction(label, value);
        }

        /// <summary>
        /// If Label is found in the data, return true and it's assigned value.
        /// </summary>
        /// <param name="data"></param>
        /// <param name="instruction"></param>
        /// <returns></returns>
        public (bool, string) ContainsLabel(string data, string instruction)
        {
            var isLabel = ContainsInstruction(data, instruction);

            if (isLabel)
            {
                var label = data.Split('(', ')');

                return (isLabel, label[1]);
            }

            return (false, "");
        }

        public string ContainsSymbol(string symbol)
        {
            var isSymbol = ContainsInstruction(symbol, "@");

            if (isSymbol)
            {
                var s = symbol.Split("@")[1];

                var isNumber = int.TryParse(s, out int number);

                if (isNumber)
                {
                    return "";
                }

                return s;
            }

            return "";
        }

        public (bool, string[]) GetInstructionSetValues(string data, string instruction)
        {
            switch (instruction)
            {
                case "@":
                    if (ContainsInstruction(data, instruction))
                    {
                        var splitData = data.Split(instruction)[1];
                        string binaryResult = _converter.ConvertAInstruction(splitData);

                        return (true, new string[] { binaryResult });
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
