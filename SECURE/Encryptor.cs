using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Text;
using System.Security.Cryptography;
using System.IO;

namespace ecomserv.SECURE
{
    //Developer : Vinod Manammal
    //Creation Date : 23 June 2014

    public class Encryptor
    {
        public string passphrase = "iN8uyhtgrRETFRDE2432fvRGp09fXqaa";
        public string Encrypt(string value)
        {
            string res = "";
            try
            {                
                if (value == null)
                    return "";
                if (value.Trim().Length == 0)
                    return "";                
                byte[] key, iv;
                byte[] salt = new byte[8];
                RNGCryptoServiceProvider rng = new RNGCryptoServiceProvider();
                rng.GetNonZeroBytes(salt);
                DeriveKeyAndIV(passphrase, salt, out key, out iv);
                byte[] encryptedBytes = EncryptStringToBytesAes(value, key, iv);
                byte[] encryptedBytesWithSalt = new byte[salt.Length + encryptedBytes.Length + 8];
                Buffer.BlockCopy(Encoding.ASCII.GetBytes("Salted__"), 0, encryptedBytesWithSalt, 0, 8);
                Buffer.BlockCopy(salt, 0, encryptedBytesWithSalt, 8, salt.Length);
                Buffer.BlockCopy(encryptedBytes, 0, encryptedBytesWithSalt, salt.Length + 8, encryptedBytes.Length);
                res = Convert.ToBase64String(encryptedBytesWithSalt);
                string new_pass = sha256(passphrase);
                string new_enc = CalcHMACSHA256Hash(res, new_pass) + res;
                res = encrypt_str(new_enc);
                return res;
            }
            catch (Exception exp)
            {
                clsCommon common = new clsCommon();
                common.Log("Encrypt", exp.Message + "(value=" + value + ")", true, exp);
            }
            return res;
        }
        public string Decrypt(string value)
        {
            string res = "";
            try
            {             
                if (value == null)
                    return "";
                if (value.Trim().Length == 0)
                    return "";
                string encrypted = decrypt_str(value);
                encrypted = encrypted.Substring(64);
                byte[] encryptedBytesWithSalt = Convert.FromBase64String(encrypted);
                byte[] salt = new byte[8];
                byte[] encryptedBytes = new byte[encryptedBytesWithSalt.Length - salt.Length - 8];
                Buffer.BlockCopy(encryptedBytesWithSalt, 8, salt, 0, salt.Length);
                Buffer.BlockCopy(encryptedBytesWithSalt, salt.Length + 8, encryptedBytes, 0, encryptedBytes.Length);
                byte[] key, iv;
                DeriveKeyAndIV(passphrase, salt, out key, out iv);
                res=DecryptStringFromBytesAes(encryptedBytes, key, iv);                
            }
            catch (Exception exp)
            {
                clsCommon common = new clsCommon();
                common.Log("Decrypt", exp.Message + "(value=" + value + ")", true, exp);
            }
            return res;
        }
        private string CalcHMACSHA256Hash(string plaintext, string salt)
        {
            string result = "";
            var enc = Encoding.Default;
            byte[]
            baText2BeHashed = enc.GetBytes(plaintext),
            baSalt = enc.GetBytes(salt);
            System.Security.Cryptography.HMACSHA256 hasher = new HMACSHA256(baSalt);
            byte[] baHashedText = hasher.ComputeHash(baText2BeHashed);
            result = string.Join("", baHashedText.ToList().Select(b => b.ToString("x2")).ToArray());
            return result;
        }
        private string sha256(string password)
        {
            SHA256Managed crypt = new SHA256Managed();
            string hash = String.Empty;
            byte[] crypto = crypt.ComputeHash(Encoding.UTF8.GetBytes(password), 0, Encoding.UTF8.GetByteCount(password));
            foreach (byte bit in crypto)
            {
                hash += bit.ToString("x2");
            }
            return hash;
        }
        private string encrypt_str(string str)
        {
            if (str == null)
                return "";
            return Microsoft.JScript.GlobalObject.escape(str);
            string htext = "";
            for (int i = 0; i < str.Length; i++)
            {
                if (htext.Trim().Length > 0)
                    htext += ".";
                htext = htext + (int)((char)(str[i] + 10 - 1 * 2));
            }
            return htext;
        }
        private string decrypt_str(string str)
        {
            if (str == null)
                return "";
            return Microsoft.JScript.GlobalObject.unescape(str);
            string[] splited = str.Split('.');
            string htext = "";
            for (int i = 0; i < splited.Length; i++)
                htext = htext + (char)((char)Convert.ToInt32(splited[i]) - 10 + 1 * 2);
            return htext;
        }
        private string DecryptStringFromBytesAes(byte[] cipherText, byte[] key, byte[] iv)
        {
            if (cipherText == null || cipherText.Length <= 0)
                throw new ArgumentNullException("cipherText");
            if (key == null || key.Length <= 0)
                throw new ArgumentNullException("key");
            if (iv == null || iv.Length <= 0)
                throw new ArgumentNullException("iv");
            RijndaelManaged aesAlg = null;
            string plaintext;
            try
            {
                aesAlg = new RijndaelManaged { Mode = CipherMode.CBC, KeySize = 256, BlockSize = 128, Key = key, IV = iv };
                ICryptoTransform decryptor = aesAlg.CreateDecryptor(aesAlg.Key, aesAlg.IV);
                using (MemoryStream msDecrypt = new MemoryStream(cipherText))
                {
                    using (CryptoStream csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
                    {
                        using (StreamReader srDecrypt = new StreamReader(csDecrypt))
                        {
                            plaintext = srDecrypt.ReadToEnd();
                            srDecrypt.Close();
                        }
                    }
                }
            }
            finally
            {
                if (aesAlg != null)
                    aesAlg.Clear();
            }
            return plaintext;
        }
        private void DeriveKeyAndIV(string passphrase, byte[] salt, out byte[] key, out byte[] iv)
        {
            List<byte> concatenatedHashes = new List<byte>(48);
            byte[] password = Encoding.UTF8.GetBytes(passphrase);
            byte[] currentHash = new byte[0];
            MD5 md5 = MD5.Create();
            bool enoughBytesForKey = false;
            while (!enoughBytesForKey)
            {
                int preHashLength = currentHash.Length + password.Length + salt.Length;
                byte[] preHash = new byte[preHashLength];
                Buffer.BlockCopy(currentHash, 0, preHash, 0, currentHash.Length);
                Buffer.BlockCopy(password, 0, preHash, currentHash.Length, password.Length);
                Buffer.BlockCopy(salt, 0, preHash, currentHash.Length + password.Length, salt.Length);
                currentHash = md5.ComputeHash(preHash);
                concatenatedHashes.AddRange(currentHash);
                if (concatenatedHashes.Count >= 48)
                    enoughBytesForKey = true;
            }
            key = new byte[32];
            iv = new byte[16];
            concatenatedHashes.CopyTo(0, key, 0, 32);
            concatenatedHashes.CopyTo(32, iv, 0, 16);
            md5.Clear();
            md5 = null;
        }
        private byte[] EncryptStringToBytesAes(string plainText, byte[] key, byte[] iv)
        {
            if (plainText == null || plainText.Length <= 0)
                throw new ArgumentNullException("plainText");
            if (key == null || key.Length <= 0)
                throw new ArgumentNullException("key");
            if (iv == null || iv.Length <= 0)
                throw new ArgumentNullException("iv");
            MemoryStream msEncrypt;
            RijndaelManaged aesAlg = null;
            try
            {
                aesAlg = new RijndaelManaged { Mode = CipherMode.CBC, KeySize = 256, BlockSize = 128, Key = key, IV = iv };
                ICryptoTransform encryptor = aesAlg.CreateEncryptor(aesAlg.Key, aesAlg.IV);
                msEncrypt = new MemoryStream();
                using (CryptoStream csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
                {
                    using (StreamWriter swEncrypt = new StreamWriter(csEncrypt))
                    {
                        swEncrypt.Write(plainText);
                        swEncrypt.Flush();
                        swEncrypt.Close();
                    }
                }
            }
            finally
            {
                if (aesAlg != null)
                    aesAlg.Clear();
            }
            return msEncrypt.ToArray();
        }
        public string GetNewPW()
        {
            string res = "";
            try
            {
                string new_pass = "iuTHfr888852TTy76543ujYtgE09fXoo";
                return new_pass;
            }
            catch (Exception exp)
            {
                clsCommon common = new clsCommon();
                common.Log("GetNewPW", exp.Message, true, exp);
            }
            return res;
        }
    }
}