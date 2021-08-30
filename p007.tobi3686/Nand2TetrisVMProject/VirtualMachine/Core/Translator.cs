using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace VirtualMachine.Core
{
    public class Translator
    {
        private const string POINTER_PREFIX = "@SP";
        private const int SLEEPTIMER_MS = 0;

        /// <summary>
        /// Updates a set memory sector. Default is @SP.
        /// </summary>
        /// <remarks>
        /// Example:
        ///     <br>@SP</br>
        ///     <br>A=M</br>    
        ///     <br>M=D</br>
        /// </remarks>
        /// <returns>A <see cref="List{T}"/> of instructions in assembly format.</returns>
        public List<string> TranslateMemoryUpdateCommand(string sector = "")
        {
            Console.WriteLine($"Updating Stack Pointer Memory");

            Thread.Sleep(SLEEPTIMER_MS);

            return new List<string> { string.IsNullOrEmpty(sector) ? POINTER_PREFIX : sector, "A=M", "M=D" };
        }

        /// <summary>
        /// Translates the Stack Pointer Up or Down.
        /// </summary>
        /// <remarks>
        /// Example:
        /// <br>@SP</br>
        /// <br>M=M+1</br>
        /// <br></br>
        /// 
        /// <br>Extra Commands:</br>
        /// <br>@SP</br>
        /// <br>AM=M-1</br>
        /// <br>D=M</br>
        /// </remarks>
        /// <param name="increment">Whether the stack pointer has to increment or decrement, default is counting down (1+).</param>
        /// <param name="argLeftOfEqualSign">An argument to specify what should be put into the left of the equal sign, default is M.</param>
        /// <param name="argRightOfEqualSign">An argument to specify what should be put into the right of the euqal sign, default is M.</param>
        /// <param name="extraCommands">Defines extra arguments that has to be followed by the stack pointer.</param>
        /// <returns>A <see cref="List{T}"/> of instructions in assembly format.</returns>
        public List<string> TranslateStackPointerCommand(bool increment = true, string argLeftOfEqualSign = "", string argRightOfEqualSign = "", params string[] extraCommands)
        {
            Console.WriteLine($"Moving Stack Pointer <{(increment ? "Down ↓" : "Up ↑")}>");
            Thread.Sleep(SLEEPTIMER_MS);

            string leftCommand = string.IsNullOrEmpty(argLeftOfEqualSign) ? "M" : argLeftOfEqualSign;
            string rightCommand = string.IsNullOrEmpty(argRightOfEqualSign) ? "M" : argRightOfEqualSign;

            var data = new List<string> { POINTER_PREFIX, increment ? $"{leftCommand}={rightCommand}+1" : $"{leftCommand}={rightCommand}-1" };

            if (extraCommands.Length != 0)
            {
                data.AddRange(extraCommands);
            }

            return data;
        }

        /// <summary>
        /// Translates the Stack Pointer up or down.
        /// </summary>
        /// <remarks>
        /// Example:
        /// <br>A=A+1</br>
        /// <br></br>
        /// </remarks>
        /// <param name="increment">Whether the stack pointer has to increment or decrement, default false.</param>
        /// <param name="specialRegister">An argument to specify what should be to the right of the equal sign.</param>
        /// <returns>A <see cref="string"/> containing the instruction in assembly format.</returns>
        //public string TranslateStackPointerPositionCommand(bool increment = false, string specialRegister = "")
        //{
        //    Console.WriteLine($"Moving Stack Pointer <{(increment ? "Down ↓" : "Up ↑")}>");
        //    Thread.Sleep(SLEEPTIMER_MS);

        //    string specialCommand = string.IsNullOrEmpty(specialRegister) ? "A" : specialRegister;

        //    return increment ? $"A={specialCommand}+1" : $"A={specialCommand}-1";
        //}
    }
}
