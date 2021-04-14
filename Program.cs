using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Text.RegularExpressions;

namespace letterCompression
{
    class Combo
    {
        public string combo;
        public int occ;
    }
    class Program
    {
        public static int blockSize = 9; //9 bits. Don't change - might break (never tested)

        public static List<char> inputAlpha = new char[] { 'a', 'b', 'c', 'd', 'e', 'f', 'g', 'h', 'i', 'j', 'k', 'l', 'm', 'n', 'o', 'p', 'q', 'r', 's', 't', 'u', 'v', 'w', 'x', 'y', 'z',
    'A', 'B', 'C', 'D', 'E', 'F', 'G', 'H', 'I', 'J', 'K', 'L', 'M', 'N', 'O', 'P', 'Q', 'R', 'S', 'T', 'U', 'V', 'W', 'X', 'Y', 'Z', '0', '1', '2', '3', '4', '5', '6', '7', '8', '9', ' ', '.', ',', '\'','"', '!', '?'}.ToList();

        public static List<Combo> dataset = JsonConvert.DeserializeObject<List<Combo>>(File.ReadAllText("dataset.json"));

        static void Main(string[] args)
        {
            //prepare the dataset
            dataset = dataset.Take((int)Math.Pow(2, blockSize) - inputAlpha.Count).ToList();
            Console.WriteLine("Hello. Press 1 to compress and 2 to decompress.");
            Console.WriteLine("===============================================");
            Console.Write(">");
            var z = Console.ReadKey();
            Console.WriteLine();
            if (z.Key == ConsoleKey.D1)
            {
                Console.Write("[Text to compress (a-Z 0-9 .,'\"!?)]=");
                string text = Console.ReadLine().Replace("\r", "").Replace("\n", " ");
                byte[] compressed = Compress(text, dataset);
                Console.WriteLine($"[Result (in base64)]={Convert.ToBase64String(compressed)}");
            } else
            {
                Console.Write("[Text to decompress (in base64)]=");
                string text = Console.ReadLine();
                string decomp = Decompress(Convert.FromBase64String(text), dataset);
                Console.WriteLine($"[Result]={decomp}");
            }
        }

        private static byte[] Compress(string toCompress, List<Combo> dataset)
        {
            List<short> compressed = new List<short>();
            for (int i = 0; i<toCompress.Length; i++)
            {
                string combo3 = "";
                string combo2 = "";
                if (i + 1 < toCompress.Length)
                {
                    combo2 = toCompress[i].ToString() + toCompress[i + 1].ToString();
                    if (i + 2 < toCompress.Length)
                    {
                        combo3 = toCompress[i].ToString() + toCompress[i + 1].ToString() + toCompress[i + 2].ToString();
                    }
                }
                string combo1 = toCompress[i].ToString();

                short c = (short)dataset.FindIndex(z => z.combo == combo3);
                if (c != -1) { compressed.Add((short)(c + inputAlpha.Count)); i += 2; continue; }
                c = (short)dataset.FindIndex(z => z.combo == combo2);
                if (c != -1) { compressed.Add((short)(c + inputAlpha.Count)); i += 1; continue; }

                c = (short)inputAlpha.FindIndex(z => z == combo1[0]);
                if (c != -1) { compressed.Add(c); } else { throw new Exception($"Unexpected character at {i}"); }
            }
            byte[] final = new byte[compressed.Count * blockSize]; //PLEASE FIND A BETTER WAY TO 9 BIT BLOCKS -> BYTE ARRAY 
            for (int i = 0; i < compressed.Count; i++)
            {
                short res = compressed[i];
                string s = Convert.ToString(res, 2).PadLeft(blockSize, '0'); //PLEASE FIND A BETTER WAY TO 9 BIT BLOCKS -> BYTE ARRAY
                for (int z = 0; z<blockSize; z++)
                {
                    final[i * blockSize + z] = (byte)s[z]; //PLEASE FIND A BETTER WAY TO 9 BIT BLOCKS -> BYTE ARRAY
                }
            }
            BigInteger bi = BaseToBI(Encoding.UTF8.GetString(final)); //PLEASE FIND A BETTER WAY TO 9 BIT BLOCKS -> BYTE ARRAY
            return bi.ToByteArray(); 
        }

        public static string Decompress(byte[] toDecomp, List<Combo> dataset)
        {
            List<short> compressed = new List<short>();
            BigInteger bi = new BigInteger(toDecomp); //PLEASE FIND A BETTER WAY TO 9 BIT BLOCKS -> BYTE ARRAY
            string s = getE(bi); //PLEASE FIND A BETTER WAY TO 9 BIT BLOCKS -> BYTE ARRAY
            s = s.PadLeft(((s.Length - 1) / blockSize + 1) * blockSize, '0'); //PLEASE FIND A BETTER WAY TO 9 BIT BLOCKS -> BYTE ARRAY
            List<short> units = new List<short>(); //PLEASE FIND A BETTER WAY TO 9 BIT BLOCKS -> BYTE ARRAY
            for (int i = 0; i<s.Length / blockSize; i++)
            { //PLEASE FIND A BETTER WAY TO 9 BIT BLOCKS -> BYTE ARRAY
                string bits = s.Substring(i * blockSize, blockSize);
                short res = (short)BaseToBI(bits);
                units.Add(res);
            }
            string result = "";
            foreach (short us in units)
            {
                result += us < inputAlpha.Count ? inputAlpha[us] : dataset[us - inputAlpha.Count].combo;
            }
            return result;
        }

        public static char[] binAlpha = new char[] { '0', '1' };

        public static BigInteger BaseToBI(string number)
        {
            var CharValues = binAlpha
           .Select((c, i) => new { Char = c, Index = i })
           .ToDictionary(c => c.Char, c => c.Index);
            char[] chrs = number.ToCharArray();
            int m = chrs.Length - 1;
            int n = binAlpha.Length, x;
            BigInteger result = 0;
            for (int i = 0; i < chrs.Length; i++)
            {
                x = CharValues[chrs[i]];
                result += x * BigInteger.Pow(n, m--); //PLEASE FIND A BETTER WAY THAT DOESN'T INCLUDE POW(2, HUGE NUMBER)
            }
            return result;
        }

        public static string getE(BigInteger entry)
        {
            if (entry == 0)
            {
                return binAlpha[0].ToString();
            }

            List<char> res = new List<char>();
            while (entry != 0)
            {
                res.Insert(0, binAlpha[(int)BigInteger.ModPow(entry, 1, binAlpha.Length)]);
                entry /= binAlpha.Length;
            }

            return new string(res.ToArray());
        }
    }
}
