using System.Threading.Tasks;

namespace _16BitAssembler
{
    interface IFileReader
    {
        string[] FileText { get; }

        Task<IFileReader> ReadAsync();

        Task WriteOutputAsync(string[] text);
    }
}
