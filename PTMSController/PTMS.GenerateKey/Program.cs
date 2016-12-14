using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace PTMS.GenerateKey {
    class Program {
        static void Main(string[] args) {
            var key = GenerateKey();

            Console.WriteLine("Key => {0}", key);
        }

        // Function to Generate a 64 bits Key.
        static string GenerateKey() {
            // Create an instance of Symetric Algorithm. Key and IV is generated automatically.
            DESCryptoServiceProvider desCrypto = (DESCryptoServiceProvider)DESCryptoServiceProvider.Create();

            

            // Use the Automatically generated key for Encryption. 
            return ASCIIEncoding.UTF7.GetString(desCrypto.Key);
        }
    }
}
