using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace FunnyServer
{
    public static class PasswordLogic
    {
        private static readonly string _validChars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890-_!@#$%^&*=+";
        private static readonly RandomNumberGenerator rng = RandomNumberGenerator.Create();
        public static string GetRandomString(int size)
        {
            string randomString = "";

            for (int i = 0; i < size; i++)
            {
                byte[] arr = new byte[4];
                rng.GetBytes(arr);

                uint randomInt = BitConverter.ToUInt32(arr);

                double value = (double)randomInt / uint.MaxValue;

                int index = (int)Math.Round((_validChars.Length - 1) * value);

                randomString += _validChars[index];
            }

            return randomString;
        }

        public static string ComputeSaltedHash(string password, string salt)
        {
            using SHA256 hashing = SHA256.Create();

            byte[] data = Encoding.UTF8.GetBytes(password + salt);

            byte[] hashedData = hashing.ComputeHash(data);

            string hash = "";

            for (int i = 0; i < hashedData.Length; i++)
            {
                hash += hashedData[i].ToString("x2");
            }

            return hash;
        }
    }
}
