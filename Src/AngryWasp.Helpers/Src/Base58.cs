using System;
using System.Linq;
using System.Numerics;
using Org.BouncyCastle.Crypto.Digests;

namespace AngryWasp.Helpers
{
    public static class Base58
    {
        private const string Digits = "123456789ABCDEFGHJKLMNPQRSTUVWXYZabcdefghijkmnopqrstuvwxyz";

        public static bool VerifyAndRemoveCheckSum(byte[] data, out byte[] result, int checksumSize = 4)
        {
            result = null;
            if (data == null) return false;
            if (data.Length < checksumSize) return false;

            result = SubArray(data, 0, data.Length - checksumSize);
            var givenCheckSum = SubArray(data, data.Length - checksumSize);
            bool ok = givenCheckSum.SequenceEqual(CalculateCheckSum(result, checksumSize));

            if (!ok) result = null;

            return ok;
        }

        public static string Encode(byte[] data)
        {
            BigInteger intData = 0;
            for (int i = 0; i < data.Length; i++)
                intData = intData * 256 + data[i];

            var result = "";
            while (intData > 0)
            {
                var remainder = (int)(intData % 58);
                intData /= 58;
                result = Digits[remainder] + result;
            }

            for (int i = 0; i < data.Length && data[i] == 0; i++)
                result = '1' + result;
            
            return result;
        }

        public static string EncodeWithCheckSum(byte[] data, int checksumSize = 4) => Encode(ConcatArrays(data, CalculateCheckSum(data, checksumSize)));

        public static bool Decode(string s, out byte[] result)
        {
            BigInteger intData = 0;
            for (int i = 0; i < s.Length; i++)
            {
                var digit = Digits.IndexOf(s[i]);
                if (digit < 0)
                {
                    result = null;
                    return false;
                }
                intData = intData * 58 + digit;
            }

            var leadingZeroCount = s.TakeWhile(c => c == '1').Count();
            var leadingZeros = Enumerable.Repeat((byte)0, leadingZeroCount);
            var be = intData.ToByteArray().Reverse();
            var bytesWithoutLeadingZeros = be.SkipWhile(b => b == 0);
            result = leadingZeros.Concat(bytesWithoutLeadingZeros).ToArray();
            return true;
        }

        public static bool DecodeWithCheckSum(string s, out byte[] result, int checksumSize = 4)
        {
            if (!Decode(s, out result))
                return false;

            return VerifyAndRemoveCheckSum(result, out result, checksumSize);
        }

        public static byte[] CalculateCheckSum(byte[] data, int checksumSize = 4) => Hash(Hash(data)).Take(4).ToArray();

        private static byte[] ConcatArrays(byte[] a, byte[] b)
        {
            var result = new byte[a.Length + b.Length];
            Buffer.BlockCopy(a, 0, result, 0, a.Length);
            Buffer.BlockCopy(b, 0, result, a.Length, b.Length);
            return result;
        }

        private static byte[] SubArray(byte[] arr, int start, int length)
        {
            var result = new byte[length];
            Buffer.BlockCopy(arr, start, result, 0, length);
            return result;
        }

        private static byte[] SubArray(byte[] arr, int start) => SubArray(arr, start, arr.Length - start);

        private static byte[] Hash(byte[] input)
        {
            var digest = new KeccakDigest(256);
            var output = new byte[digest.GetDigestSize()];
            digest.BlockUpdate(input, 0, input.Length);
            digest.DoFinal(output, 0);
            return output;
        }
    }
}