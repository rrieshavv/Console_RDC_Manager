using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleRdpManager
{
    public static class Logger
    {
        public static void Log(string filePath, string message)
        {
            File.AppendAllText(filePath, $"{DateTime.Now}: {message}\n");
        }
    }

}
