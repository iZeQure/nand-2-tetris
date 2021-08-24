using Assembler.Core;
using System;

namespace Assembler.Handlers
{
    public class Converter
    {
        private readonly SymbolManager _symbolManager = new();
        private readonly int _bitSize = 16;
        private readonly char _paddingChar = '0';
        private int _ramPosition = 16;

        public void StoreInstruction(string key, string value = "")
        {
            if (string.IsNullOrEmpty(value))
            {
                if (!_symbolManager.PredefinedSymbols.ContainsKey(key))
                {
                    _symbolManager.PredefinedSymbols.Add(key, $"{_ramPosition}");
                    _ramPosition++;
                }

                return;
            }

            if (!_symbolManager.PredefinedSymbols.ContainsKey(key))
            {
                _symbolManager.PredefinedSymbols.Add(key, value);
            }
        }

        public string ConvertAInstruction(string data)
        {
            var isValue = int.TryParse(data, out int value);

            if (isValue)
            {
                var binaryValue = GetBinaryValue(value);
                return binaryValue;
            }

            var isSymbolFound = _symbolManager.PredefinedSymbols.TryGetValue(data, out string key);

            if (isSymbolFound)
            {
                var isNumber = int.TryParse(key, out int symbolValue);

                if (isNumber)
                {
                    /* Convert key to binary value. */
                    var binaryValue = GetBinaryValue(symbolValue);
                    return binaryValue;
                }
            }

            return GetBinaryValue(0);

            ///* A Instruction is a value. */
            //if (isValue)
            //{
            //    var binaryValue = GetBinaryValue(value);
            //    return binaryValue;
            //}

            ///* Check for symbol, could be a label, symbol or custom symbol. */
            //var isSymbolFound = _symbolManager.PredefinedSymbols.TryGetValue(data, out string key);

            //if (isSymbolFound)
            //{
            //    int.TryParse(data, out int symbolValue);

            //    /* Convert key to binary value. */
            //    var binaryValue = GetBinaryValue(symbolValue);
            //    return binaryValue;
            //}

            //return GetBinaryValue(0);
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

        private string GetBinaryValue(int value)
        {
            return Convert.ToString(value, 2).PadLeft(_bitSize, _paddingChar);
        }
    }
}
