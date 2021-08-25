using System;

namespace _16BitAssembler
{
    class BinaryConverter : IConverter
    {
        const int BIT_SIZE = 16;
        const char PADDING_CHAR = '0';
        readonly SymbolTable _symbolTable = new();

        public string AInstruction(string value)
        {
            return ConvertToBinary(value);
        }

        public string Comp(string comp)
        {
            var resultZero = _symbolTable.CompZero.TryGetValue(comp, out string binaryValueZero);

            if (resultZero)
            {
                return $"0{binaryValueZero}";
            }

            var resultOne = _symbolTable.CompOne.TryGetValue(comp, out string binaryValueOne);

            if (resultOne)
            {
                return $"1{binaryValueOne}";
            }

            return string.Empty;
        }

        public string Dest(string dest)
        {
            var result = _symbolTable.Dest.TryGetValue(dest, out string binaryValue);

            if (result)
            {
                return binaryValue;
            }

            return string.Empty;
        }

        public string Jump(string jump)
        {
            var result = _symbolTable.Jump.TryGetValue(jump, out string binaryValue);

            return binaryValue;
        }

        private string ConvertToBinary(string value)
        {
            return Convert.ToString(ushort.Parse(value), 2).PadLeft(BIT_SIZE, PADDING_CHAR);
        }
    }
}
