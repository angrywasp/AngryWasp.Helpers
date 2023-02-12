using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;

namespace AngryWasp.Helpers
{
    public static class StringHelper
    {
        /// <summary>
        /// Splits parameters from a string in the format {a:0 b:1}
        /// </summary>
        /// <returns><c>true</c>, if get type parameters was tryed, <c>false</c> otherwise.</returns>
        /// <param name="s">S.</param>
        /// <param name="namedParameters">Named parameters.</param>
        public static bool TryGetTypeParams(this string s, out Dictionary<string, string> namedParameters)
        {
            s = s.TrimStart(new char[] { '{' }).TrimEnd(new char[] { '}' });

            string[] parameters = s.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

            namedParameters = new Dictionary<string, string>();
            foreach (string p in parameters)
            {
                try
                {
                    string[] np = p.Split(new char[] { ':' }, StringSplitOptions.RemoveEmptyEntries);
                    namedParameters.Add(np[0], np[1]);
                }
                catch (Exception)
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Gets a set of comma delimited values inside a set of parenthesis. i.e. (1, 2, 3)
        /// </summary>
        /// <returns>The values inside parentheses.</returns>
        /// <param name="s">S.</param>
        public static List<string> GetValuesInsideParentheses(this string s)
        {
            int startIndex = s.IndexOf('(') + 1;
            int endIndex = s.LastIndexOf(')');
            s = s.Substring(startIndex, endIndex - startIndex);
            string[] parameters = s.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            List<string> result = new List<string>();
            foreach (string p in parameters)
            {
                string r = p.Trim();
                result.Add(r);
            }

            return result;
        }

        /// <summary>
        /// Replaces all chars in toReplace with the corresponding element in replaceWith. The length of both arrays must be equal
        /// </summary>
        /// <param name="toReplace">array of chars to replace</param>
        /// <param name="replaceWith">array of chars to replace them with</param>
        /// <returns>string.Empty in an error, otherwise the processed string</returns>
        public static string ReplaceAll(this string value, char[] toReplace, char[] replaceWith)
        {
            if (toReplace.Length != replaceWith.Length)
                return string.Empty;

            for (int i = 0; i < toReplace.Length; i++)
                value = value.Replace(toReplace[i], replaceWith[i]);

            return value;
        }

        /// <summary>
        /// Replaces all strings in toReplace with the corresponding element in replaceWith. The length of both arrays must be equal
        /// </summary>
        /// <param name="toReplace">array of strings to replace</param>
        /// <param name="replaceWith">array of strings to replace them with</param>
        /// <returns>string.Empty in an error, otherwise the processed string</returns>
        public static string ReplaceAll(this string value, string[] toReplace, string[] replaceWith)
        {
            if (toReplace.Length != replaceWith.Length)
                return string.Empty;

            for (int i = 0; i < toReplace.Length; i++)
                value = value.Replace(toReplace[i], replaceWith[i]);

            return value;
        }

        /// <summary>
        /// Checks if the supplied string is a valid email address
        /// </summary>
        /// <param name="s">The string to check</param>
        /// <param name="onlineCheck">if true, performs secondary checking for the existence of the address</param>
        /// <returns>true if valid, else false</returns>
        public static bool IsValidEmail(this string s, bool onlineCheck = false)
        {
            if (string.IsNullOrEmpty(s))
                return false;

            bool regexMatch = false;

            string strRegex = @"^([a-zA-Z0-9_\-\.]+)@((\[[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\.)|(([a-zA-Z0-9\-]+\.)+))([a-zA-Z]{2,4}|[0-9]{1,3})(\]?)$";
            Regex re = new Regex(strRegex);
            if (re.IsMatch(s))
                regexMatch = true;

            if (!regexMatch)
                return false;

            //untested
            if (onlineCheck)
            {
                string[] host = (s.Split('@'));
                string hostname = host[1];

                try
                {
                    IPHostEntry IPhst = Dns.GetHostEntry(hostname);
                    IPEndPoint endPt = new IPEndPoint(IPhst.AddressList[0], 25);
                    Socket sock = new Socket(endPt.AddressFamily,
                            SocketType.Stream, ProtocolType.Tcp);
                    sock.Connect(endPt);
                }
                catch (Exception)
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Parses a DateTime object to a file friendly string by removing illegal characters and replacing with substitutes.
        /// substitutions are:
        /// / = -
        /// : = .
        /// </summary>
        /// <param name="dt">The DateTime object to parse</param>
        /// <returns>A string which has file name illegal characters removed</returns>
        public static string ParseDateTimeToFileFriendly(DateTime dt, bool includeMilliseconds = false)
        {
            return ReplaceAll(dt.ToString(), new char[] { '/', ':' }, new char[] { '-', '.' }) + (includeMilliseconds ? dt.Millisecond.ToString() : string.Empty);
        }

        /// <summary>
        /// Converts all the tab characters \t in a string to spaces
        /// </summary>
        /// <param name="s">the string to convert</param>
        /// <param name="spaces">the number of spaces to use for each tab</param>
        /// <returns>the formatted string</returns>
        public static string TabsToSpaces(this string s, int spaces)
        {
            string space = string.Empty;
            for (int i = 0; i < spaces; i++)
                space += " ";

            return ReplaceAll(s, new string[] { "\t" }, new string[] { space });
        }

        /// <summary>
        /// Removes all spaces from a string
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        public static string RemoveWhitespace(this string s)
        {
            s = TabsToSpaces(s, 1);
            s = s.Replace(" ", "");
            return s;
        }

        /// <summary>
        /// checks a path for illegal characters
        /// </summary>
        /// <param name="s">The path to check</param>
        /// <returns>True if illegal characters found, otherwise false</returns>
        public static bool PathContainsIllegalCharacters(this string s)
        {
            if (s.Contains('\\') || s.Contains('/') || s.Contains(':') ||
                s.Contains('*') || s.Contains('?') || s.Contains('"') ||
                s.Contains('<') || s.Contains('>') || s.Contains('|'))
                return true;

            return false;
        }

        public static string NormalizeFilePath(this string s)
        {
            return s.Replace("\\", "/");
        }

        public static string CapitalizeFirstWord(this string value) => value[0].ToString().ToUpper() + value.Substring(1);

        #region Encode/Decode Base64

        public static string EncodeBase64(this string t) => Convert.ToBase64String(Encoding.UTF8.GetBytes(t));

        public static string EncodeBase64(this byte[] t) => Convert.ToBase64String(t);

        public static string DecodeBase64(this string t) => Encoding.UTF8.GetString(Convert.FromBase64String(t));

        public static string EncodeBase58(this string t) => Base58.Encode(Encoding.UTF8.GetBytes(t));

        public static string EncodeBase58(this byte[] t) => Base58.Encode(t);

        public static string DecodeBase58(this string t)
        {
            if (!Base58.Decode(t, out byte[] result))
                throw new FormatException("Base58 decoding failed");

            return Encoding.UTF8.GetString(result);
        }

        #endregion

        #region Encode/Decode a string to a hex representation

        /// <summary>
        /// Converts a string of ASCII characters to a byte[]
        /// </summary>
        public static byte[] CharsToByte(this string input, int count = 0)
        {
            count = (count == 0) ? input.Length : count;
            byte[] raw = new byte[count];
            for (int i = 0; i < count; i++)
                raw[i] = Convert.ToByte(input[i]);

            return raw;
        }

        /// <summary>
        /// Converts a string of ASCII characters to an sbyte[]
        /// </summary>
        public static sbyte[] CharsToSByte(this string input, int count = 0)
        {
            count = (count == 0) ? input.Length : count;
            sbyte[] raw = new sbyte[count];
            for (int i = 0; i < count; i++)
                raw[i] = Convert.ToSByte(input[i]);

            return raw;
        }

        /// <summary>
        /// Converts a hex formatted string to a byte[]
        /// </summary>
        public static byte[] FromByteHex(this string input)
        {
            byte[] raw = new byte[input.Length / 2];
            for (int i = 0; i < raw.Length; i++)
                raw[i] = Convert.ToByte(input.Substring(i * 2, 2), 16);

            return raw;
        }

        /// <summary>
        /// Converts a hex formatted string to an sbyte[]
        /// </summary>
        public static sbyte[] FromSByteHex(this string input)
        {
            sbyte[] raw = new sbyte[input.Length / 2];
            for (int i = 0; i < raw.Length; i++)
                raw[i] = Convert.ToSByte(input.Substring(i * 2, 2), 16);

            return raw;
        }

        /// <summary>
        /// Converts a byte[] to a hex formatted string
        /// </summary>
        public static string ToHex(this byte[] input, bool lowerCase = true, int count = 0)
        {
            try
            {
                count = (count == 0) ? input.Length : count;
                string format = lowerCase ? "{0:x2}" : "{0:X2}";
                StringBuilder sb = new StringBuilder();
                for (int i = 0; i < count; i++)
                    sb.AppendFormat(format, input[i]);
                return sb.ToString().Trim();
            }
            catch { return string.Empty; }
        }

        /// <summary>
        /// Converts an sbyte[] to a hex formatted string
        /// </summary>
        public static string ToHex(this sbyte[] input, bool lowerCase = true, int count = 0)
        {
            try
            {
                count = (count == 0) ? input.Length : count;
                string format = lowerCase ? "{0:x2}" : "{0:X2}";
                StringBuilder sb = new StringBuilder();
                for (int i = 0; i < count; i++)
                    sb.AppendFormat(format, input[i]);
                return sb.ToString().Trim();
            }
            catch { return string.Empty; }
        }

        public static string ToPrefixedHex(this byte[] hex) => $"0x{hex.ToHex()}";

        public static string ToPrefixedHex(this string hex) => $"{(hex.StartsWith("0x") ? hex : "0x" + hex)}";

        public static string TrimHexPrefix(this string hex) => (hex.StartsWith("0x") ? hex.Substring(2) : hex);

        #endregion
    }
}