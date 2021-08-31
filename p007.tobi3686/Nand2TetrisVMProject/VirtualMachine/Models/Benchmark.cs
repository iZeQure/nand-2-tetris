using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VirtualMachine.Models
{
    public class Benchmark
    {
        private TimeSpan _time;
        private string _fileSize;
        private string _location;

        public Benchmark(TimeSpan time, string fileSize, string location)
        {
            Time = time;
            FileSize = fileSize;
            Location = location;
        }

        public TimeSpan Time { get => _time; set => _time = value; }
        public string FileSize { get => _fileSize; set => _fileSize = value; }
        public string Location { get => _location; set => _location = value; }
    }
}
