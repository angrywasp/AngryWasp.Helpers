using System;
using System.Diagnostics;

namespace AngryWasp.Helpers.Test
{
    internal class MainClass
    {
        private static void Main(string[] rawArgs)
        {
            Console.WriteLine("");
            //var a = Base58.EncodeWithCheckSum(new byte[] { Constants.ADDRESS_PREFIX }.Concat(new byte[28]).ToArray(), 3);
            var b = Base58.DecodeWithCheckSum("4uQeVj5tqViQh7yWWGStvkEG1Zmhx6uasJtWCJzxFNY", out var result, out var checksum, 3);
            Debugger.Break();
            //Console.WriteLine(a);
            //Console.WriteLine(result.Concat(checksum).ToArray().ToPrefixedHex());
        }
    }
}