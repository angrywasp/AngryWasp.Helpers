using System.Linq;

namespace AngryWasp.Helpers
{
    public static class Base49
    {
        private const string CHARSET = "ABCDEFGHJKLMNPQRSTUVWXYZabcdefghijkmnopqrstuvwxyz";

        public static bool VerifyAndRemoveCheckSum(byte[] data, out byte[] result, out byte[] checksum, int checksumSize = 4) =>
            new Encoder(CHARSET).VerifyAndRemoveCheckSum(data, out result, out checksum, checksumSize);

        public static string Encode(byte[] data) => new Encoder(CHARSET).Encode(data);

        public static string EncodeWithCheckSum(byte[] data, int checksumSize = 4) =>
            Encode(data.Concat(Encoder.CalculateCheckSum(data, checksumSize)).ToArray());

        public static bool Decode(string s, out byte[] result) => new Encoder(CHARSET).Decode(s, out result);

        public static bool DecodeWithCheckSum(string s, out byte[] result, out byte[] checksum, int checksumSize = 4) =>
            new Encoder(CHARSET).DecodeWithCheckSum(s, out result, out checksum, checksumSize);
    }
}