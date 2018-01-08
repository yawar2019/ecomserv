using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Text;
using System.Security.Cryptography;
using System.IO;

 
    //Developer : Vinod Manammal
    //Creation Date : 23 June 2014

public class Encryptor
{
    public string clientpass = "iNhhTuntT654356yhUyjfvRGp09fXqaa";
    public string serverpass = "iNhhTuntT654356yhUyjfvRGp09fXqaa";
    private string passphrase = "";
    public Encryptor(bool isserver)
    {
        if (isserver)
            passphrase = serverpass;
        else
            passphrase = clientpass;
    }
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
            common.Log("Encrypt Data", exp.Message + "(value=" + value + ")", true, exp);
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
            res = DecryptStringFromBytesAes(encryptedBytes, key, iv);
        }
        catch (Exception exp)
        {
            //weps_EmployeeMobilePortalApp.Common.clsLog.Log(true, true, true, "Decrypt Data", exp.Message + "(value=" + value + ")", weps_EmployeeMobilePortalApp.Common.clsLog.LOG_TYPE.APP_ERROR, "", "", "", exp);
            clsCommon common = new clsCommon();
            common.Log("Decrypt Data", exp.Message + "(value=" + value + ")",true,exp);
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
    private string Decrypt1(string cipherText)
    {
        string EncryptionKey = "MAKV2SPBNI99212";
        cipherText = cipherText.Replace(" ", "+");
        byte[] cipherBytes = Convert.FromBase64String(cipherText);
        using (Aes encryptor = Aes.Create())
        {
            Rfc2898DeriveBytes pdb = new Rfc2898DeriveBytes(EncryptionKey, new byte[] { 0x49, 0x76, 0x61, 0x6e, 0x20, 0x4d, 0x65, 0x64, 0x76, 0x65, 0x64, 0x65, 0x76 });
            encryptor.Key = pdb.GetBytes(32);
            encryptor.IV = pdb.GetBytes(16);
            using (MemoryStream ms = new MemoryStream())
            {
                using (CryptoStream cs = new CryptoStream(ms, encryptor.CreateDecryptor(), CryptoStreamMode.Write))
                {
                    cs.Write(cipherBytes, 0, cipherBytes.Length);
                    cs.Close();
                }
                cipherText = Encoding.Unicode.GetString(ms.ToArray());
            }
        }
        return cipherText;
    }
    private string Encrypt1(string clearText)
    {
        string EncryptionKey = "MAKV2SPBNI99212";
        byte[] clearBytes = Encoding.Unicode.GetBytes(clearText);
        using (Aes encryptor = Aes.Create())
        {
            Rfc2898DeriveBytes pdb = new Rfc2898DeriveBytes(EncryptionKey, new byte[] { 0x49, 0x76, 0x61, 0x6e, 0x20, 0x4d, 0x65, 0x64, 0x76, 0x65, 0x64, 0x65, 0x76 });
            encryptor.Key = pdb.GetBytes(32);
            encryptor.IV = pdb.GetBytes(16);
            using (MemoryStream ms = new MemoryStream())
            {
                using (CryptoStream cs = new CryptoStream(ms, encryptor.CreateEncryptor(), CryptoStreamMode.Write))
                {
                    cs.Write(clearBytes, 0, clearBytes.Length);
                    cs.Close();
                }
                clearText = Convert.ToBase64String(ms.ToArray());
            }
        }
        return clearText;
    }
    public string encrypt_str(string str)
    {
        if (str == null)
            return "";
        //return ConvertStringToHex(str, System.Text.Encoding.Unicode);
        // return URLEncoder.encode("TEj+TWBQExpz8/p5SAjIhA==", "UTF-8");
        // return System.Web.HttpUtility.UrlEncode(str, Encoding.UTF8);
        return Encrypt1(str);
        string htext = "";
        for (int i = 0; i < str.Length; i++)
        {
            if (htext.Trim().Length > 0)
                htext += ".";
            htext = htext + (int)((char)(str[i] + 10 - 1 * 2));
        }
        return htext;
    }
    public string decrypt_str(string str)
    {
        if (str == null)
            return "";
        // return ConvertHexToString(str, System.Text.Encoding.Unicode);
        // return System.Web.HttpUtility.UrlDecode(str,Encoding.UTF8);
        return Decrypt1(str);
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
}
