using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Security.AccessControl;
using System.Security.Cryptography;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;

namespace PTMS.GenerateKey {
    class Program {
        static void Main(string[] args) {
            //testuser@test.com:testingPassword123!
            //AM/9g+c39HnUT8Ioe3D4Y0uOLzDQdmWBb3raXpFHm8VYk/ZNt3rW+qXLFaiV+1WoWw==

            var hash = HashPassword("Vt3zjmcTVa32n7yMAA4M");

            Console.WriteLine("Hash => {0}", hash);

            Console.WriteLine("Equal => {0}", VerifyHashedPassword("AHI8y0ZAzTmDEaP/2jF4pLmvf2uDSNkzwJGiBuAMQUrx64mTmGOUPKcIYDJe/WLBqQ==", ""));


            var s = GenerateKey();
            var d = GenerateKey();

            Console.WriteLine("Key 1 => {0}", s);
            Console.WriteLine("Key 2 => {0}", d);
            Console.WriteLine("Password Hash => {0}",hash);
  
            string keyName = (string)null;
            byte[] bytes = (byte[])null;
  
            //new SecurityIdentifier("S-1-5-32-568");
            var val = GetAppSetting("NobleHostConnStr", (string)null);
            if (IsEncrypted(val, out keyName, out bytes)) {
                val = DecryptBytesAES(bytes, keyName, new SecurityIdentifier("S-1-5-32-568"));
                //this._EncryptedMembers[name] = EncryptedConfigCtrl.SettingState.Encrypted;
            }
            Console.WriteLine("ConnStr = {0}", val);
        }

        public static string GetAppSetting(string name, string defaultValue) {
            string str = defaultValue;
            string appSetting = ConfigurationManager.AppSettings[name];
            if (!string.IsNullOrEmpty(appSetting))
                str = appSetting;
            return str;
        }

        private static bool IsEncrypted(string s, out string keyName, out byte[] bytes) {
            keyName = (string)null;
            bytes = (byte[])null;
            if (string.IsNullOrEmpty(s) || (uint)(s.Trim().Length % 4) > 0U)
                return false;
            try {
                StringBuilder stringBuilder = new StringBuilder();
                byte[] bytes1 = Convert.FromBase64String(s);
                for (int index = 0; index < bytes1.Length; ++index) {
                    string str = Encoding.UTF8.GetString(bytes1, index, 1);
                    if (str == "|") {
                        keyName = stringBuilder.ToString();
                        bytes = ((IEnumerable<byte>)bytes1).Skip<byte>(index + 1).ToArray<byte>();
                        break;
                    }
                    stringBuilder.Append(str);
                }
                return keyName != null;
            } catch (FormatException ex) {
                return false;
            }
        }

        public static string DecryptStringAES(string key, string keyContainer = "NSCContainer", params SecurityIdentifier[] visibleTo) {
            if (string.IsNullOrWhiteSpace(keyContainer))
                keyContainer = "NSCContainer";
            return DecryptBytesAES(Convert.FromBase64String(key), keyContainer, Array.Empty<SecurityIdentifier>());
        }

        public static string DecryptBytesAES(byte[] encryptedData, string keyContainer = "NSCContainer", params SecurityIdentifier[] visibleTo) {
            if (string.IsNullOrWhiteSpace(keyContainer))
                keyContainer = "NSCContainer";
            using (MemoryStream memoryStream = new MemoryStream()) {
                using (CryptoStream cryptoStream = new CryptoStream((Stream)memoryStream, GetCryptoTransform(keyContainer, true, visibleTo), CryptoStreamMode.Write)) {
                    cryptoStream.Write(encryptedData, 0, encryptedData.Length);
                    cryptoStream.FlushFinalBlock();
                }
                return Encoding.UTF8.GetString(memoryStream.ToArray());
            }
        }

        private static ICryptoTransform GetCryptoTransform(string keyContainer, bool isDecrypt, SecurityIdentifier[] visibleTo) {
            string path = Path.Combine(".", keyContainer);

            CspParameters cspParameters = new CspParameters() {
                CryptoKeySecurity = new CryptoKeySecurity(),
                //Flags = CspProviderFlags.UseMachineKeyStore,
                KeyContainerName = keyContainer
            };
            CryptoKeyAccessRule rule = new CryptoKeyAccessRule("everyone", CryptoKeyRights.FullControl, AccessControlType.Allow);

            cspParameters.CryptoKeySecurity = new CryptoKeySecurity();
            cspParameters.CryptoKeySecurity.SetAccessRule(rule);

            using (RSACryptoServiceProvider cryptoServiceProvider1 = new RSACryptoServiceProvider(2048)) { //}, cspParameters)) {
                AesCryptoServiceProvider cryptoServiceProvider2 = new AesCryptoServiceProvider();
                cryptoServiceProvider2.KeySize = 256;
                if (File.Exists(path)) {
                    byte[] rgb = File.ReadAllBytes(path);
                    byte[] numArray = cryptoServiceProvider1.Decrypt(rgb, false);
                    cryptoServiceProvider2.Key = ((IEnumerable<byte>)numArray).Take<byte>(32).ToArray<byte>();
                    cryptoServiceProvider2.IV = ((IEnumerable<byte>)numArray).Skip<byte>(32).ToArray<byte>();
                } else {
                    cryptoServiceProvider2.GenerateKey();
                    cryptoServiceProvider2.GenerateIV();
                    List<byte> list = ((IEnumerable<byte>)cryptoServiceProvider2.Key).ToList<byte>();
                    list.AddRange((IEnumerable<byte>)cryptoServiceProvider2.IV);

                    File.WriteAllBytes(path, cryptoServiceProvider1.Encrypt(list.ToArray(), false));
                }
                if (isDecrypt)
                    return cryptoServiceProvider2.CreateDecryptor(cryptoServiceProvider2.Key, cryptoServiceProvider2.IV);
                return cryptoServiceProvider2.CreateEncryptor(cryptoServiceProvider2.Key, cryptoServiceProvider2.IV);
            }
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
