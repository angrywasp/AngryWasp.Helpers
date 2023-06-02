using System;
using System.Linq;
using System.Numerics;
using Org.BouncyCastle.Crypto.Digests;

namespace AngryWasp.Helpers
{
    public class Encoder
    {
        private string charSet;
        private int count;

        public Encoder(string charSet)
        {
            this.charSet = charSet;
            this.count = charSet.Length;
        }

        public bool VerifyAndRemoveCheckSum(byte[] data, out byte[] result, out byte[] checksum, int checksumSize = 4)
        {
            result = checksum = null;
            if (data == null) return false;
            if (data.Length < checksumSize) return false;

            result = SubArray(data, 0, data.Length - checksumSize);
            checksum = SubArray(data, data.Length - checksumSize);
            bool ok = checksum.SequenceEqual(CalculateCheckSum(result, checksumSize));

            if (!ok) result = checksum = null;

            return ok;
        }

        public string Encode(byte[] data)
        {
            BigInteger intData = 0;
            for (int i = 0; i < data.Length; i++)
                intData = intData * 256 + data[i];

            var result = "";
            while (intData > 0)
            {
                var remainder = (int)(intData % count);
                intData /= count;
                result = charSet[remainder] + result;
            }

            for (int i = 0; i < data.Length && data[i] == 0; i++)
                result = '1' + result;
            
            return result;
        }

        public string EncodeWithCheckSum(byte[] data, int checksumSize = 4) =>
            Encode(data.Concat(CalculateCheckSum(data, checksumSize)).ToArray());

        public bool Decode(string s, out byte[] result)
        {
            BigInteger intData = 0;
            for (int i = 0; i < s.Length; i++)
            {
                var digit = charSet.IndexOf(s[i]);
                if (digit < 0)
                {
                    result = null;
                    return false;
                }
                intData = intData * count + digit;
            }

            var leadingZeroCount = s.TakeWhile(c => c == '1').Count();
            var leadingZeros = Enumerable.Repeat((byte)0, leadingZeroCount);
            var be = intData.ToByteArray().Reverse();
            var bytesWithoutLeadingZeros = be.SkipWhile(b => b == 0);
            result = leadingZeros.Concat(bytesWithoutLeadingZeros).ToArray();
            return true;
        }

        public bool DecodeWithCheckSum(string s, out byte[] result, out byte[] checksum, int checksumSize = 4)
        {
            result = checksum = null;
            if (!Decode(s, out result))
                return false;

            return VerifyAndRemoveCheckSum(result, out result, out checksum, checksumSize);
        }

        public static byte[] CalculateCheckSum(byte[] data, int checksumSize = 4) => Hash(Hash(data)).Take(checksumSize).ToArray();

        private byte[] SubArray(byte[] arr, int start, int length)
        {
            var result = new byte[length];
            Buffer.BlockCopy(arr, start, result, 0, length);
            return result;
        }

        private byte[] SubArray(byte[] arr, int start) => SubArray(arr, start, arr.Length - start);

        public static byte[] Hash(byte[] input)
        {
            var digest = new KeccakDigest(256);
            var output = new byte[digest.GetDigestSize()];
            digest.BlockUpdate(input, 0, input.Length);
            digest.DoFinal(output, 0);
            return output;
        }
    }
}