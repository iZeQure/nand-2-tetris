using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VirtualMachine.Enums;

namespace VirtualMachine.UI
{
    public class ConsoleUIManager
    {
        public static void Beautify(Color color, string text)
        {
            var consoleColor = ConsoleColor.White;

            switch (color)
            {
                case Color.Red:
                    consoleColor = ConsoleColor.Red;
                    break;
                case Color.Green:
                    consoleColor = ConsoleColor.Green;
                    break;
                case Color.Yellow:
                    consoleColor = ConsoleColor.Yellow;
                    break;
                case Color.Blue:
                    consoleColor = ConsoleColor.Blue;
                    break;
                case Color.Turquoise:
                    consoleColor = ConsoleColor.Cyan;
                    break;
                case Color.Default:
                    consoleColor = ConsoleColor.White;
                    break;
            }

            Console.Write($"{"[INFO]",-10}");
            Console.Write($"{DateTime.Now,-25}");
            Console.ForegroundColor = consoleColor;
            Console.WriteLine($"{text,-10}");
            Console.ResetColor();
        }
    }
}
