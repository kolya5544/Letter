using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace letterCompression
{
    class Program
    {
        static void Main(string[] args)
        {
            Letter.Init();
            Console.WriteLine("Hello. Press 1 to compress and 2 to decompress.");
            Console.WriteLine("===============================================");
            Console.Write(">");
            var z = Console.ReadKey();
            Console.WriteLine();
            if (z.Key == ConsoleKey.D1)
            {
                Console.Write("[Text to compress (a-Z 0-9 .,'\"!?)]=");
                string text = Console.ReadLine().Replace("\r", "");
                byte[] compressed = Letter.Compress(text);
                Console.WriteLine($"[Result (in base64)]={Convert.ToBase64String(compressed)}");
            } else
            {
                Console.Write("[Text to decompress (in base64)]=");
                string text = Console.ReadLine();
                string decomp = Letter.Decompress(Convert.FromBase64String(text));
                Console.WriteLine($"[Result]={decomp}");
            }
        }
    }
}
