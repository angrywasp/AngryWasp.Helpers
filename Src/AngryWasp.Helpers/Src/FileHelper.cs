using System;
using System.IO;
using System.Text;
using System.IO.Compression;

namespace AngryWasp.Helpers
{
    public class FileHelper
    {
        public static string RenameDuplicateFile(string p)
        {
            if (!File.Exists(p))
                return p;

            string fileName = Path.GetFileNameWithoutExtension(p);
            string dirName = Path.GetDirectoryName(p);
            string ext = Path.GetExtension(p);

            for (int n = 0; n < 1000; n++)
            {
                string v = string.Format("{0:000}", n);

                string fn = $"{Path.Combine(dirName, fileName)}_{v}{ext}";

                if (!File.Exists(fn))
                    return Path.Combine(dirName, Path.GetFileName(fn));
            }

            throw new IOException("Could not rename file after 1000 attempts");
        }

        public static string RenameDuplicateFolder(string p)
        {
            if (!Directory.Exists(p))
                return p;

            for (int n = 0; n < 1000; n++)
            {
                string v = string.Format("{0:000}", n);
                string dn = $"{p}_{v}";

                if (!Directory.Exists(dn))
                    return dn;
            }

            throw new IOException("Could not rename directory after 1000 attempts");
        }

        /// <summary>
        /// Converts a path to a relative path
        /// </summary>
        /// <param name="path1">The path to make relative</param>
        /// <param name="path2">The path to make Path1 relative to</param>
        /// <returns>the relative path equivalent of Path1</returns>
        public static string MakeRelative(string path1, string path2)
        {
            if (path1 == path2)
                return string.Empty;
            string[] path1Parts = SplitPath(path1);
            string[] path2Parts = SplitPath(path2);

            int counter = 0;
            while ((counter < path2Parts.Length) && (counter < path1Parts.Length) && path2Parts[counter].Equals(path1Parts[counter], StringComparison.InvariantCultureIgnoreCase))
                counter++;

            if (counter == 0)
                return path1; // There is no relative link.

            StringBuilder sb = new StringBuilder();
            for (int i = counter; i < path2Parts.Length; i++)
                sb.Append("../");

            for (int i = counter; i < path1Parts.Length; i++)
                sb.Append($"{path1Parts[i]}/");

            //remove end seperator char
            sb.Length--;

            return sb.ToString();
        }
            
        public static string MoveUpDirectories(string path, int levels)
        {
            string[] pathParts = SplitPath(path);

            //return 
            if (pathParts.Length < levels)
                return null;

            StringBuilder sb = new StringBuilder();

            for (int i = 0; i < pathParts.Length - levels; i++)
                sb.Append($"{pathParts[i]}/");

            if (sb.Length > 0)
                sb.Length--;

            return sb.ToString();
        }

        private static string[] SplitPath(string path) => path.NormalizeFilePath().Trim('/').Split('/');

        public static byte[] Compress(byte[] b)
        {
            MemoryStream ms = new MemoryStream();
            GZipStream sw = new GZipStream(ms, CompressionMode.Compress);

            sw.Write(b, 0, b.Length);
            sw.Close();
            b = ms.ToArray();
            ms.Close();
            sw.Dispose();
            ms.Dispose();
            return b;
        }

        public static void Compress(string FileToCompress, string CompressedFile)
        {
            byte[] buffer = new byte[1024 * 1024]; // 1MB

            using (System.IO.FileStream sourceFile = System.IO.File.OpenRead(FileToCompress))
            {
                using (System.IO.FileStream destinationFile = System.IO.File.Create(CompressedFile))
                {
                    using (System.IO.Compression.GZipStream output = new System.IO.Compression.GZipStream(destinationFile,
                        System.IO.Compression.CompressionMode.Compress))
                    {
                        int bytesRead = 0;
                        while (bytesRead < sourceFile.Length)
                        {
                            int ReadLength = sourceFile.Read(buffer, 0, buffer.Length);
                            output.Write(buffer, 0, ReadLength);
                            output.Flush();
                            bytesRead += ReadLength;
                        }

                        destinationFile.Flush();
                    }

                    destinationFile.Close();
                }

                sourceFile.Close();
            }
        }

        /// <summary>
        /// Decompresses a byte[]
        /// </summary>
        /// <param name="b">the byte[] to decompress</param>
        /// <param name="i">the original (uncompressed) length of the array</param>
        /// <returns></returns>
        public static byte[] Decompress(byte[] b, int i)
        {
            //Prepare for decompress
            MemoryStream ms = new MemoryStream(b);
            GZipStream sr = new GZipStream(ms, CompressionMode.Decompress);

            //Reset variable to collect uncompressed result
            b = new byte[i];

            //Decompress
            sr.Read(b, 0, i);
            sr.Close();
            ms.Close();
            sr.Dispose();
            ms.Dispose();
            return b;
        }

        public static byte[] Decompress(FileStream fs, int i)
        {
            //Prepare for decompress
            GZipStream sr = new GZipStream(fs, CompressionMode.Decompress);

            //Reset variable to collect uncompressed result
            byte[] b = new byte[i];

            //Decompress
            sr.Read(b, 0, i);
            sr.Close();
            fs.Close();

            sr.Dispose();
            fs.Dispose();
            return b;
        }

        public static byte[] Decompress(ref GZipStream sr, int i)
        {
            //Reset variable to collect uncompressed result
            byte[] b = new byte[i];
            //Decompress
            sr.Read(b, 0, i);
            return b;
        }
    }
}
