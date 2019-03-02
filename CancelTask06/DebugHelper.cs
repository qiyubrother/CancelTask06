#define  DEBUG_MODE
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CancelTask06
{
    public class DebugHelper
    {
        public static void PrintTxMessage(string s)
        {
#if DEBUG_MODE
            s = $"[{DateTime.Now}]Tx::{s}";
            var fc = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.Blue;
            Console.WriteLine(s);
            Console.ForegroundColor = fc;
#endif
        }

        public static void PrintRxMessage(string s)
        {
#if DEBUG_MODE
            s = $"[{DateTime.Now}]Rx::{s}";
            var fc = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine(s);
            Console.ForegroundColor = fc;
#endif
        }

        public static void PrintDebugMessage(byte[] buffer)
        {
#if DEBUG_MODE
            var sb = new StringBuilder();
            foreach (var b in buffer)
            {
                sb.Append($"{Convert.ToString(b, 16).PadLeft(2, '0').ToUpper()} ");
            }
            var s = $"[{DateTime.Now}]{sb}";

            var fc = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.DarkCyan;
            Console.WriteLine(s);
            Console.ForegroundColor = fc;
#endif
        }

        public static void PrintTraceMessage(string s)
        {
#if DEBUG_MODE
            var fc = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"[{DateTime.Now}][Trace]::{s}");
            Console.ForegroundColor = fc;
#endif
        }

        public static void PrintErrorMessage(string s)
        {
#if DEBUG_MODE
            var fc = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"[{DateTime.Now}][Error]::{s}");
            Console.ForegroundColor = fc;
#endif
        }
    }
}
