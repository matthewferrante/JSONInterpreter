using System;
using System.Configuration;
using System.IO;
using System.Reflection;
using System.Security.Cryptography;

namespace PTMS.Core.Utilities {
    public static class FileSystem {
        public static void AssertDirectoryExists(string path) {
            if (!Directory.Exists(path)) {
                Directory.CreateDirectory(path);
            }
        }

        public static string BuildAssemblyPath(string relPath) {
            return Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), relPath);
        }
        public static string BuildAbsolutePath(string absPath) {
            return Path.GetDirectoryName(absPath);
        }

        public static string MD5Sum(string path) {
            using (var md5 = MD5.Create()) {
                using (var stream = File.OpenRead(path)) {
                    return ByteArrayToString(md5.ComputeHash(stream));
                }
            }
        }

        public static string ByteArrayToString(byte[] ba) {
            string hex = BitConverter.ToString(ba);

            return hex.Replace("-", "");
        }
    }
}
