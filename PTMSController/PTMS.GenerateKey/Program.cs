using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace PTMS.GenerateKey {
    class Program {
        static void Main(string[] args) {
            //testuser@test.com:testingPassword123!
            //AM/9g+c39HnUT8Ioe3D4Y0uOLzDQdmWBb3raXpFHm8VYk/ZNt3rW+qXLFaiV+1WoWw==

            var hash = HashPassword("K66ZD5z0ocbL7xqhpvTx");

            Console.WriteLine("Hash => {0}", hash);

            Console.WriteLine("Equal => {0}", VerifyHashedPassword("AG04bMxM/MegTjvu+1Al8Dqa6nCclUv4gWJe68Jq/2T6EHdcC1FnZrUYjRCuymTDgQ==", ""));


            var s = GenerateKey();
            var d = GenerateKey();

            Console.WriteLine("Key 1 => {0}", s);
            Console.WriteLine("Key 2 => {0}", d);
        }

        // Function to Generate a 64 bits Key.
        static string GenerateKey() {
            // Create an instance of Symetric Algorithm. Key and IV is generated automatically.
            DESCryptoServiceProvider desCrypto = (DESCryptoServiceProvider)DESCryptoServiceProvider.Create();



            // Use the Automatically generated key for Encryption. 
            return ASCIIEncoding.UTF7.GetString(desCrypto.Key);
        }


        public static string HashPassword(string password) {
            byte[] salt;
            byte[] buffer2;

            if (password == null) {
                throw new ArgumentNullException("password");
            }

            using (Rfc2898DeriveBytes bytes = new Rfc2898DeriveBytes(password, 0x10, 0x3e8)) {
                salt = bytes.Salt;
                buffer2 = bytes.GetBytes(0x20);
            }

            byte[] dst = new byte[0x31];
            Buffer.BlockCopy(salt, 0, dst, 1, 0x10);
            Buffer.BlockCopy(buffer2, 0, dst, 0x11, 0x20);

            return Convert.ToBase64String(dst);
        }

        public static bool VerifyHashedPassword(string hashedPassword, string password) {
            byte[] buffer4;
            if (hashedPassword == null) {
                return false;
            }
            if (password == null) {
                throw new ArgumentNullException("password");
            }
            byte[] src = Convert.FromBase64String(hashedPassword);
            if ((src.Length != 0x31) || (src[0] != 0)) {
                return false;
            }
            byte[] dst = new byte[0x10];
            Buffer.BlockCopy(src, 1, dst, 0, 0x10);
            byte[] buffer3 = new byte[0x20];
            Buffer.BlockCopy(src, 0x11, buffer3, 0, 0x20);
            using (Rfc2898DeriveBytes bytes = new Rfc2898DeriveBytes(password, dst, 0x3e8)) {
                buffer4 = bytes.GetBytes(0x20);
            }
            return ByteArraysEqual(buffer3, buffer4);
        }

        public static bool ByteArraysEqual(byte[] b1, byte[] b2) {
            if (b1 == b2) return true;
            if (b1 == null || b2 == null) return false;
            if (b1.Length != b2.Length) return false;

            for (int i = 0; i < b1.Length; i++) {
                if (b1[i] != b2[i]) return false;
            }

            return true;
        }
    }
}
