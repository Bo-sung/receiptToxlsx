using System;
using System.Security.Cryptography;

namespace Ragnarok
{
    internal class AesEncrypter
    {
        public System.Text.UTF8Encoding utf8Encoding = null;
        private RijndaelManaged rijndael = null;

        public string Key
        {
            get { return byte2Hex(this.rijndael.Key); }
            set
            {
                if (value == null || value == "")
                    throw new ArgumentException("The key is not null.", "key");
                this.rijndael.Key = hex2Byte(value);
            }
        }

        public string IV
        {
            get { return byte2Hex(this.rijndael.IV); }
            set
            {
                if (value == null || value == "")
                    throw new ArgumentException("The initial vector is not null.", "initialVector");
                this.rijndael.IV = hex2Byte(value);
            }
        }

        public CipherMode AesCipherMode
        {
            get { return this.rijndael.Mode; }
            set { this.rijndael.Mode = value; }
        }

        public PaddingMode AesPaddingMode
        {
            get { return this.rijndael.Padding; }
            set { this.rijndael.Padding = value; }
        }

        public int iAesCipherMode
        {
            get { return (int)this.rijndael.Mode; }
            set { this.rijndael.Mode = (CipherMode)Enum.Parse(typeof(CipherMode), value.ToString()); }
        }

        public int iAesPaddingMode
        {
            get { return (int)this.rijndael.Padding; }
            set { this.rijndael.Padding = (PaddingMode)Enum.Parse(typeof(PaddingMode), value.ToString()); }
        }

        public AesEncrypter(bool useIV)
        {
            this.utf8Encoding = new System.Text.UTF8Encoding();
            this.rijndael = new RijndaelManaged();
            this.rijndael.Mode = CipherMode.ECB;
            this.rijndael.Padding = PaddingMode.PKCS7;
            this.rijndael.KeySize = 128;
            this.rijndael.BlockSize = 128;
            this.rijndael.GenerateKey();
            if (useIV)
                this.rijndael.GenerateIV();

            //UnityEngine.Debug.Log("Key: " + byte2Hex(rijndael.Key));
            //UnityEngine.Debug.Log("IV: " + byte2Hex(rijndael.IV));
        }

        public AesEncrypter(string key)
        {
            if (key == null || key == "")
                throw new ArgumentException("The key is not null.", "key");

            this.utf8Encoding = new System.Text.UTF8Encoding();
            this.rijndael = new RijndaelManaged();
            this.rijndael.Mode = CipherMode.ECB;
            this.rijndael.Padding = PaddingMode.PKCS7;
            this.rijndael.KeySize = 128;
            this.rijndael.BlockSize = 128;

            this.rijndael.Key = hex2Byte(key);
        }

        public AesEncrypter(string key, string iv)
        {
            if (key == null || key == "")
                throw new ArgumentException("The key is not null.", "key");
            if (iv == null || iv == "")
                throw new ArgumentException("The initial vector cis not null.",
                  "initialVector");
            this.utf8Encoding = new System.Text.UTF8Encoding();
            this.rijndael = new RijndaelManaged();
            this.rijndael.Mode = CipherMode.CBC;
            this.rijndael.Padding = PaddingMode.PKCS7;
            this.rijndael.KeySize = 128;
            this.rijndael.BlockSize = 128;

            this.rijndael.Key = hex2Byte(key);
            this.rijndael.IV = hex2Byte(iv);
        }

        public byte[] encrypt(byte[] content)
        {
            byte[] cipherBytes = null;
            ICryptoTransform transform = null;
            //             if (text == null)
            //                 text = "";
            try
            {
                cipherBytes = new byte[] { };
                transform = this.rijndael.CreateEncryptor();
                /*byte[] plainText = this.utf8Encoding.GetBytes(text);*/
                byte[] plainText = content;
                cipherBytes = transform.TransformFinalBlock(plainText, 0,
                  plainText.Length);
            }
            catch
            {
                //System.Console.WriteLine(e.StackTrace);
                throw new ArgumentException(
                   "text is not a valid string!(Encrypt)", "text");
            }
            finally
            {
                //if (this.rijndael != null)
                //this.rijndael.Clear();

            }
            return cipherBytes;
        }

        public byte[] decrypt(byte[] content)
        {
            byte[] plainText = null;
            ICryptoTransform transform = null;
            //             if (text == null || text == "")
            //                 throw new ArgumentException("text is not null.");

            try
            {
                plainText = new byte[] { };
                transform = rijndael.CreateDecryptor();
                /*byte[] encryptedValue = Convert.FromBase64String(text);*/
                byte[] encryptedValue = content;
                plainText = transform.TransformFinalBlock(encryptedValue, 0,
                   encryptedValue.Length);
            }
            catch (System.Exception e)
            {
                throw new ArgumentException("text is not a valid string!(Decrypt)", "text");
                //System.Console.WriteLine(e.StackTrace);
            }
            finally
            {
                //if (this.rijndael != null)
                //this.rijndael.Clear();
            }
            return plainText;
        }

        public static byte[] hex2Byte(string hex)
        {
            byte[] bytes = new byte[hex.Length / 2];
            for (int i = 0; i < bytes.Length; i++)
            {
                try
                {
                    bytes[i] = Convert.ToByte(hex.Substring(i * 2, 2), 16);
                }
                catch
                {
                    throw new ArgumentException(
                    "hex is not a valid hex number!", "hex");
                }
            }
            return bytes;
        }

        public static string byte2Hex(byte[] bytes)
        {
            string hex = "";
            if (bytes != null)
            {
                for (int i = 0; i < bytes.Length; i++)
                {
                    hex += bytes[i].ToString("X2");
                }
            }
            return hex;
        }
    }
}