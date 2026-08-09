using System.Text;
using Effortless.Net.Encryption;

namespace EncDecExample.WebApi.Services
{
    public class EncDecService
    {
        private readonly byte[] key;
        private readonly byte[] iv;
        public EncDecService(IConfiguration config)
        {
            key = Encoding.ASCII.GetBytes(config["Security:Key"]!);
            iv = Encoding.ASCII.GetBytes(config["Security:IV"]!);
        }

        public string Encrypt(string plainText)
        {
            return Strings.Encrypt(plainText, key, iv);
        }
        public string Decrypt(string encryptedText)
        {
            return Strings.Decrypt(encryptedText, key, iv);
        }
    }
}
