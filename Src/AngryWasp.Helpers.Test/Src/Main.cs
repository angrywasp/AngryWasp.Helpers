using System;
using System.Diagnostics;
using System.Text;

namespace AngryWasp.Helpers.Test
{
    internal class MainClass
    {
        private static void Main(string[] rawArgs)
        {
            Console.WriteLine("");
            string testData = "Hello World. This is a test string";
            var encoded = Base49.EncodeWithCheckSum(Encoding.UTF8.GetBytes(testData));
            Base49.DecodeWithCheckSum(encoded, out var decoded, out var checksum);
            Console.WriteLine(Encoding.UTF8.GetString(decoded));
            
            //Console.WriteLine(a);
            //Console.WriteLine(result.Concat(checksum).ToArray().ToPrefixedHex());
        }
    }
}