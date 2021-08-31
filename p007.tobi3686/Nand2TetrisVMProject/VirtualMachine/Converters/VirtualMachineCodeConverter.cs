using System.Collections.Generic;
using VirtualMachine.Enums;
using VirtualMachine.Handlers;
using VirtualMachine.Helpers;
using VirtualMachine.UI;

namespace VirtualMachine.Converters
{
    public class VirtualMachineCodeConverter : IMachineCodeConverter
    {
        private readonly CommandHandler _commandHandler;

        public VirtualMachineCodeConverter(CommandHandler commandHandler)
        {
            _commandHandler = commandHandler;
        }

        public List<string> GenerateMachineCode(List<string> vmCode)
        {
            List<string> generatedAsmCode = new();

            foreach (var vmCommand in vmCode)
            {
                var vmCommandPrefix = CommandHelper.SplitCommand(vmCommand)[0];

                if (CommandHelper.ContainsCommandPrefix(vmCommand, vmCommandPrefix))
                {
                    switch (vmCommandPrefix)
                    {
                        case "push":
                            generatedAsmCode.AddRange(_commandHandler.PushCommand(vmCommand));
                            continue;
                        case "pop":
                            generatedAsmCode.AddRange(_commandHandler.PopCommand(vmCommand));
                            continue;
                        case "add":
                            generatedAsmCode.AddRange(_commandHandler.AddCommand());
                            continue;
                        case "sub":
                            generatedAsmCode.AddRange(_commandHandler.SubCommand());
                            continue;
                        case "neg":
                            generatedAsmCode.AddRange(_commandHandler.NegCommand());
                            continue;
                        case "and":
                            generatedAsmCode.AddRange(_commandHandler.AndCommand());
                            continue;
                        case "or":
                            generatedAsmCode.AddRange(_commandHandler.OrCommand());
                            continue;
                        case "not":
                            generatedAsmCode.AddRange(_commandHandler.NotCommand());
                            continue;
                        case "eq":
                        case "lt":
                        case "gt":
                            generatedAsmCode.AddRange(_commandHandler.JmpCommand(vmCommandPrefix));
                            continue;
                        default:
                            ConsoleUIManager.Beautify(Color.Yellow, "Command not Found!");
                            break;
                    }
                }
            }

            return generatedAsmCode;
        }
    }
}
