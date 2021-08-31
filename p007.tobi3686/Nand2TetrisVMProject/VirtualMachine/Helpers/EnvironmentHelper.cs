using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VirtualMachine.Helpers
{
    public class EnvironmentHelper
    {
        public static void ExitEnvironment()
        {
            Environment.Exit(0);
        }

        public static string GetAspNetCoreEnvironmentVariable()
        {
            return Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
        }
    }
}
