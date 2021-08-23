using Assembler.Core;
using System;

namespace Assembler.Handlers
{
    public class Converter
    {
        private readonly SymbolManager _symbolManager = new();
        private readonly int _bitSize = 16;
        private readonly char _paddingChar = '0';

        public string ConvertAInstruction(int data)
        {
            var binaryValue = Convert.ToString(data, 2).PadLeft(_bitSize, _paddingChar);
            return binaryValue;
        }

        public string ConvertDestInstruction(string data)
        {
            if (_symbolManager.DestInstructions.TryGetValue(data, out string value))
            {
                return value;
            }

            return "000";
        }

        public string ConvertCompInstruction(string data)
        {
            if (!_symbolManager.CompZeroInstructions.TryGetValue(data, out string compZero))
            {
                if (_symbolManager.CompOneInstructions.TryGetValue(data, out string compOne))
                {
                    return $"1{compOne}";
                }
            }

            return $"0{compZero}";
        }

        public string ConvertJumpInstruction(string data)
        {
            if (_symbolManager.JumpInstructions.TryGetValue(data, out string value))
            {
                return value;
            }

            return "000";
        }
    }
}
