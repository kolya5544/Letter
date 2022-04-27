using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace letterCompression
{
    public class Letter
    {
        public static List<char> inputAlpha = new char[] { 'a', 'b', 'c', 'd', 'e', 'f', 'g', 'h', 'i', 'j', 'k', 'l', 'm', 'n', 'o', 'p', 'q', 'r', 's', 't', 'u', 'v', 'w', 'x', 'y', 'z',
    'A', 'B', 'C', 'D', 'E', 'F', 'G', 'H', 'I', 'J', 'K', 'L', 'M', 'N', 'O', 'P', 'Q', 'R', 'S', 'T', 'U', 'V', 'W', 'X', 'Y', 'Z', '0', '1', '2', '3', '4', '5', '6', '7', '8', '9', ' ', '.', ',', '\'','"', '!', '?', '\n'}.ToList();

        public static int blockSize = 9; //9 bits. Don't change - might break (never tested)

        public static List<Combo> dataset = JsonConvert.DeserializeObject<List<Combo>>(File.ReadAllText("datasets/English.json"));

        public static void Init()
        {
            //prepare the dataset
            dataset = dataset.Take((int)Math.Pow(2, blockSize) - inputAlpha.Count).ToList();
        }

        public static byte[] Compress(string toCompress)
        {
            List<short> compressed = new List<short>();
            for (int i = 0; i < toCompress.Length; i++)
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

            BitArray bitArray = new BitArray(compressed.Count * blockSize);
            for (int i = 0; i < compressed.Count; i++)
            {
                int mask = 1;
                for (int j = 0; j < blockSize; j++)
                {
                    bitArray[i * blockSize + j] = Convert.ToBoolean(compressed[i] & mask);
                    mask <<= 1;
                }
            }

            byte[] final = new byte[(int)Math.Ceiling((double)compressed.Count * blockSize / 8)];
            bitArray.CopyTo(final, 0);
            return final;
        }

        public static string Decompress(byte[] toDecomp)
        {
            BitArray bitArray = new BitArray(toDecomp);
            StringBuilder stringBuilder = new StringBuilder();
            for (int i = 0; i < bitArray.Length / blockSize; i++)
            {
                int block = 0;
                for (int j = 0; j < blockSize; j++)
                {
                    block |= Convert.ToInt32(bitArray[i * blockSize + j]) << j;
                }
                stringBuilder.Append(block < inputAlpha.Count ? inputAlpha[block] : dataset[block - inputAlpha.Count].combo);
            }
            return stringBuilder.ToString();
        }
    }
    public class Combo
    {
        public string combo;
        public int occ;
    }
}
