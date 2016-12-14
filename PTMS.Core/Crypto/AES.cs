using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace PTMS.Core.Crypto {
    public static class AES {
        private const string Salt = "jjwofjeijwofjqhuwq38vv9";
        private const int SizeOfBuffer = 1024 * 8; // 8k

        public static void EncryptFile(string inputPath, string outputPath, string password) {
            EncryptFile(inputPath, outputPath, EncodePassword(password));
        }
        public static void DecryptFile(string inputPath, string outputPath, string password) {
            DecryptFile(inputPath, outputPath, EncodePassword(password));
        }

        private static void EncryptFile(string inputPath, string outputPath, Rfc2898DeriveBytes key) {
            var input = new FileStream(inputPath, FileMode.Open, FileAccess.Read);
            var output = new FileStream(outputPath, FileMode.OpenOrCreate, FileAccess.Write);
            var algorithm = new RijndaelManaged { KeySize = 256, BlockSize = 128 };

            algorithm.Key = key.GetBytes(algorithm.KeySize / 8);
            algorithm.IV = key.GetBytes(algorithm.BlockSize / 8);

            using (var encryptedStream = new CryptoStream(output, algorithm.CreateEncryptor(), CryptoStreamMode.Write)) {
                CopyStream(input, encryptedStream);
            }
        }
        private static void DecryptFile(string inputPath, string outputPath, Rfc2898DeriveBytes key) {
            var input = new FileStream(inputPath, FileMode.Open, FileAccess.Read);
            var output = new FileStream(outputPath, FileMode.OpenOrCreate, FileAccess.Write);

            // Make sure block size is set to 128 bits for AES
            // don't use CFB mode, or make sure return size is same as block size
            var algorithm = new RijndaelManaged { KeySize = 256, BlockSize = 128 };

            algorithm.Key = key.GetBytes(algorithm.KeySize / 8);
            algorithm.IV = key.GetBytes(algorithm.BlockSize / 8);

            try {
                using (var decryptedStream = new CryptoStream(output, algorithm.CreateDecryptor(), CryptoStreamMode.Write)) {
                    CopyStream(input, decryptedStream);
                }
            } catch (CryptographicException) {
                throw new InvalidDataException("Incorrect Key");
            } catch (Exception ex) {
                throw new Exception(ex.Message);
            }
        }

        private static void CopyStream(Stream input, Stream output) {
            using (output) {
                using (input) {
                    byte[] buffer = new byte[SizeOfBuffer];
                    int read;
                    while ((read = input.Read(buffer, 0, buffer.Length)) > 0) {
                        output.Write(buffer, 0, read);
                    }
                }
            }
        }
        private static Rfc2898DeriveBytes EncodePassword(string password) {
            byte[] encryptedPassword;

            using (var algorithm = new RijndaelManaged()) {
                algorithm.KeySize = 256;
                algorithm.BlockSize = 128;

                // Encrypt the string to an array of bytes.
                encryptedPassword = EncryptStringToBytes(password, algorithm.Key, algorithm.IV);
            }

            return new Rfc2898DeriveBytes(encryptedPassword.Aggregate(string.Empty, (current, b) => current + b.ToString()), Encoding.ASCII.GetBytes(Salt));
        }
        private static byte[] EncryptStringToBytes(string plainText, byte[] key, byte[] iv) {
            if (plainText == null || plainText.Length <= 0) throw new ArgumentNullException("plainText");
            if (key == null || key.Length <= 0) throw new ArgumentNullException("key");
            if (iv == null || iv.Length <= 0) throw new ArgumentNullException("iv");

            byte[] encrypted;

            using (var rijAlg = new RijndaelManaged()) {
                rijAlg.IV = iv;
                rijAlg.Key = key;

                ICryptoTransform enc = rijAlg.CreateEncryptor(rijAlg.Key, rijAlg.IV);

                using (var ms = new MemoryStream()) {
                    using (var cs = new CryptoStream(ms, enc, CryptoStreamMode.Write)) {
                        using (var sw = new StreamWriter(cs)) {
                            sw.Write(plainText);
                        }

                        encrypted = ms.ToArray();
                    }
                }
            }

            return encrypted;
        }
    }
}


//private static string DecryptStringFromBytes(byte[] cipherText, byte[] key, byte[] iv) {
//    if (cipherText == null || cipherText.Length <= 0) throw new ArgumentNullException("cipherText");
//    if (key == null || key.Length <= 0) throw new ArgumentNullException("key");
//    if (iv == null || iv.Length <= 0) throw new ArgumentNullException("iv");

//    string plaintext;

//    using (var rijAlg = new RijndaelManaged()) {
//        rijAlg.IV = iv;
//        rijAlg.Key = key;

//        ICryptoTransform decryptor = rijAlg.CreateDecryptor(rijAlg.Key, rijAlg.IV);

//        using (var ms = new MemoryStream(cipherText)) {
//            using (var cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Read)) {
//                using (var sr = new StreamReader(cs)) {
//                    plaintext = sr.ReadToEnd();
//                }
//            }
//        }

//    }

//    return plaintext;
//}

