// See https://aka.ms/new-console-template for more information
using System.Text;
using Effortless.Net.Encryption;

Console.WriteLine("Hello, World!");

byte[] key = Encoding.ASCII.GetBytes("ff12jj12uu12ll12ff12jj12uu12ll12");
byte[] iv = Encoding.ASCII.GetBytes("ff12jj12uu12ll12");

string encrypted = Strings.Encrypt("Secret", key, iv);
Console.WriteLine(encrypted);
string decrypted = Strings.Decrypt(encrypted, key, iv);
Console.WriteLine(decrypted);
Console.ReadLine();
