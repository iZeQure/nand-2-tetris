using System.Collections.Generic;

namespace VirtualMachine.Converters
{
    public interface IMachineCodeConverter
    {
        List<string> GenerateMachineCode(List<string> machineCode);
    }
}