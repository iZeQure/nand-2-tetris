using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using VirtualMachine.Core;
using VirtualMachine.Enums;
using VirtualMachine.Handlers;
using VirtualMachine.Helpers;
using VirtualMachine.Models;
using VirtualMachine.UI;

namespace VirtualMachine
{
    public class Startup
    {
        private readonly IMachineCodeConverter _vmConverter;
        private readonly FileHandler _fileHandler;
        private readonly Stopwatch _stopwatch = new();

        private Dictionary<string, Benchmark> _benchmarks = new();
        private Dictionary<string, List<string>> _asmFileData = new();

        public Startup(IMachineCodeConverter vmConverter, FileHandler fileHandler)
        {
            _vmConverter = vmConverter;
            _fileHandler = fileHandler;
        }

        public void RunTranslator(string rootDir, string rootOutputDir, string fileSearchPattern)
        {
            var dirFiles = _fileHandler.GetDirectoryFiles(rootDir, fileSearchPattern);

            if (!dirFiles.Any())
            {
                ConsoleUIManager.Beautify(Color.Default, "No files found in root dir.");
                EnvironmentHelper.ExitEnvironment();
            }

            GenerateAssemblyMachineCode(dirFiles);

            if (!_asmFileData.Any())
            {
                ConsoleUIManager.Beautify(Color.Default, "No data was generated into assembly machine code.");
                EnvironmentHelper.ExitEnvironment();
            }

            CreateOutputFiles(rootOutputDir);

            if (_benchmarks.Any())
            {
                ConsoleUIManager.Beautify(Color.Default, $"Creating Benchmark source in => {rootOutputDir}");

                CreateBenchmarkFile(rootOutputDir);                
            }
        }

        private void CreateBenchmarkFile(string rootOutputDir)
        {
            var headers = $"{"Name",-20} {"Time",-20} {"Size",-5} {"Location",-20}";
            var formattedBenchmarks = new List<string>();

            foreach (var benchmark in _benchmarks)
            {
                formattedBenchmarks.Add($"{benchmark.Key,-20} {benchmark.Value.Time,-20} {benchmark.Value.FileSize,-5} {benchmark.Value.Location,-20}");
            }

            _fileHandler.WriteFile(rootOutputDir + "benchmark.txt", formattedBenchmarks, headers);
        }

        private void CreateOutputFiles(string rootOutputDir)
        {
            foreach (var asmData in _asmFileData)
            {
                string outputFileName = $"{rootOutputDir}{asmData.Key}.test.asm";

                ConsoleUIManager.Beautify(Color.Default, $"Writing generated assembly machine code to => {outputFileName}");
                _fileHandler.WriteFile(outputFileName, asmData.Value);

                var info = FileHelper.GetFileInfo(outputFileName);
                _benchmarks[asmData.Key].FileSize = FileHelper.ConvertFileSizeToReadableNumbers(info.Length);
                _benchmarks[asmData.Key].Location = info.FullName;
            }
        }

        private void GenerateAssemblyMachineCode(string[] dirFiles)
        {
            foreach (var file in dirFiles)
            {
                string fileName = FileHelper.GetFileName(file);

                _stopwatch.Start(); // Start Benchmark timer.

                var dataFile = _fileHandler.LoadFile(file);
                var generatedDataFile = _vmConverter.GenerateMachineCode(dataFile);

                _stopwatch.Stop(); // Stop Benchmark timer. Then Reset.
                _benchmarks.Add(fileName, new Benchmark(_stopwatch.Elapsed, "", ""));
                _stopwatch.Reset();

                _asmFileData.Add(fileName, generatedDataFile);
            }
        }
    }
}
