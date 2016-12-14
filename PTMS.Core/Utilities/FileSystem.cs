using System.Configuration;
using System.IO;
using System.Reflection;

namespace PTMS.Core.Utilities {
    public static class FileSystem {
        public static void AssertDirectoryExists(string path) {
            if (!Directory.Exists(path)) {
                Directory.CreateDirectory(path);
            }
        }

        public static string BuildAssemblyPath(string relPath) {
            return Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "\\" + relPath + "\\";
        }
        public static string BuildAbsolutePath(string absPath) {
            return Path.GetDirectoryName(absPath);
        }
    }
}
