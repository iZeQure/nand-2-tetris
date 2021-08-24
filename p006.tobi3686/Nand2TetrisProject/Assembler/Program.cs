using System.Threading.Tasks;

namespace Assembler
{
    class Program
    {
        static async Task Main(string[] args) => await new Startup().RunAssembler();
    }
}
