﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Web;

public class Protection
{
    public byte[] OpenSSLEncrypt(byte[] plain, string passphrase)
    {
        // generate salt
        byte[] key, iv;
        byte[] salt = new byte[8];
        RNGCryptoServiceProvider rng = new RNGCryptoServiceProvider();
        rng.GetNonZeroBytes(salt);
        DeriveKeyAndIV(passphrase, salt, out key, out iv);
        // encrypt bytes
        byte[] encryptedBytes = EncryptBytesAes(plain, key, iv);
        // add salt as first 8 bytes
        byte[] encryptedBytesWithSalt = new byte[salt.Length + encryptedBytes.Length + 8];
        Buffer.BlockCopy(Encoding.ASCII.GetBytes("Salted__"), 0, encryptedBytesWithSalt, 0, 8);
        Buffer.BlockCopy(salt, 0, encryptedBytesWithSalt, 8, salt.Length);
        Buffer.BlockCopy(encryptedBytes, 0, encryptedBytesWithSalt, salt.Length + 8, encryptedBytes.Length);
        // base64 encode
        return encryptedBytesWithSalt;
    }


    public string OpenSSLEncrypt(string plainText, string passphrase)
    {
        // generate salt
        byte[] key, iv;
        byte[] salt = new byte[8];
        RNGCryptoServiceProvider rng = new RNGCryptoServiceProvider();
        rng.GetNonZeroBytes(salt);
        DeriveKeyAndIV(passphrase, salt, out key, out iv);
        // encrypt bytes
        byte[] encryptedBytes = EncryptStringToBytesAes(plainText, key, iv);
        // add salt as first 8 bytes
        byte[] encryptedBytesWithSalt = new byte[salt.Length + encryptedBytes.Length + 8];
        Buffer.BlockCopy(Encoding.ASCII.GetBytes("Salted__"), 0, encryptedBytesWithSalt, 0, 8);
        Buffer.BlockCopy(salt, 0, encryptedBytesWithSalt, 8, salt.Length);
        Buffer.BlockCopy(encryptedBytes, 0, encryptedBytesWithSalt, salt.Length + 8, encryptedBytes.Length);
        // base64 encode
        return Convert.ToBase64String(encryptedBytesWithSalt);
    }

    public byte[] OpenSSLDecrypt(byte[] encrypted, string passphrase)
    {
        // extract salt (first 8 bytes of encrypted)
        byte[] salt = new byte[8];
        byte[] encryptedBytes = new byte[encrypted.Length - salt.Length - 8];
        Buffer.BlockCopy(encrypted, 8, salt, 0, salt.Length);
        Buffer.BlockCopy(encrypted, salt.Length + 8, encryptedBytes, 0, encryptedBytes.Length);
        // get key and iv
        byte[] key, iv;
        DeriveKeyAndIV(passphrase, salt, out key, out iv);
        return DecryptFromBytesAes(encryptedBytes, key, iv);
    }

    public string OpenSSLDecrypt(string encrypted, string passphrase)
    {
        // base 64 decode
        byte[] encryptedBytesWithSalt = Convert.FromBase64String(encrypted);
        // extract salt (first 8 bytes of encrypted)
        byte[] salt = new byte[8];
        byte[] encryptedBytes = new byte[encryptedBytesWithSalt.Length - salt.Length - 8];
        Buffer.BlockCopy(encryptedBytesWithSalt, 8, salt, 0, salt.Length);
        Buffer.BlockCopy(encryptedBytesWithSalt, salt.Length + 8, encryptedBytes, 0, encryptedBytes.Length);
        // get key and iv
        byte[] key, iv;
        DeriveKeyAndIV(passphrase, salt, out key, out iv);
        return DecryptStringFromBytesAes(encryptedBytes, key, iv);
    }

    private static void DeriveKeyAndIV(string passphrase, byte[] salt, out byte[] key, out byte[] iv)
    {
        // generate key and iv
        List<byte> concatenatedHashes = new List<byte>(48);

        byte[] password = Encoding.UTF8.GetBytes(passphrase);
        byte[] currentHash = new byte[0];
        MD5 md5 = MD5.Create();
        bool enoughBytesForKey = false;
        // See http://www.openssl.org/docs/crypto/EVP_BytesToKey.html#KEY_DERIVATION_ALGORITHM
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

    static byte[] EncryptBytesAes(byte[] plain, byte[] key, byte[] iv)
    {
        // Check arguments.
        if (plain == null || plain.Length <= 0)
            throw new ArgumentNullException("plain");
        if (key == null || key.Length <= 0)
            throw new ArgumentNullException("key");
        if (iv == null || iv.Length <= 0)
            throw new ArgumentNullException("iv");

        // Declare the stream used to encrypt to an in memory
        // array of bytes.
        MemoryStream msEncrypt;

        // Declare the RijndaelManaged object
        // used to encrypt the data.
        RijndaelManaged aesAlg = null;

        try
        {
            // Create a RijndaelManaged object
            // with the specified key and IV.
            aesAlg = new RijndaelManaged { Mode = CipherMode.CBC, KeySize = 256, BlockSize = 128, Key = key, IV = iv };

            // Create an encryptor to perform the stream transform.
            ICryptoTransform encryptor = aesAlg.CreateEncryptor(aesAlg.Key, aesAlg.IV);

            // Create the streams used for encryption.
            msEncrypt = new MemoryStream();
            using (CryptoStream csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write | CryptoStreamMode.Read))
            {
                using (BinaryWriter swEncrypt = new BinaryWriter(csEncrypt))
                {
                    
                    //Write all data to the stream.
                    swEncrypt.Write(plain);
                    swEncrypt.Flush();
                    swEncrypt.Close();

                }
            }
        }
        finally
        {
            // Clear the RijndaelManaged object.
            if (aesAlg != null)
                aesAlg.Clear();
        }

        // Return the encrypted bytes from the memory stream.
        return msEncrypt.ToArray();
    }

    static byte[] EncryptStringToBytesAes(string plainText, byte[] key, byte[] iv)
    {
        // Check arguments.
        if (plainText == null || plainText.Length <= 0)
            throw new ArgumentNullException("plainText");
        if (key == null || key.Length <= 0)
            throw new ArgumentNullException("key");
        if (iv == null || iv.Length <= 0)
            throw new ArgumentNullException("iv");

        // Declare the stream used to encrypt to an in memory
        // array of bytes.
        MemoryStream msEncrypt;

        // Declare the RijndaelManaged object
        // used to encrypt the data.
        RijndaelManaged aesAlg = null;

        try
        {
            // Create a RijndaelManaged object
            // with the specified key and IV.
            aesAlg = new RijndaelManaged { Mode = CipherMode.CBC, KeySize = 256, BlockSize = 128, Key = key, IV = iv };

            // Create an encryptor to perform the stream transform.
            ICryptoTransform encryptor = aesAlg.CreateEncryptor(aesAlg.Key, aesAlg.IV);

            // Create the streams used for encryption.
            msEncrypt = new MemoryStream();
            using (CryptoStream csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
            {
                using (StreamWriter swEncrypt = new StreamWriter(csEncrypt))
                {

                    //Write all data to the stream.
                    swEncrypt.Write(plainText);
                    swEncrypt.Flush();
                    swEncrypt.Close();
                }
            }
        }
        finally
        {
            // Clear the RijndaelManaged object.
            if (aesAlg != null)
                aesAlg.Clear();
        }

        // Return the encrypted bytes from the memory stream.
        return msEncrypt.ToArray();
    }

    static byte[] DecryptFromBytesAes(byte[] encrypted, byte[] key, byte[] iv)
    {
        // Check arguments.
        if (encrypted == null || encrypted.Length <= 0)
            throw new ArgumentNullException("cipherText");
        if (key == null || key.Length <= 0)
            throw new ArgumentNullException("key");
        if (iv == null || iv.Length <= 0)
            throw new ArgumentNullException("iv");

        // Declare the RijndaelManaged object
        // used to decrypt the data.
        RijndaelManaged aesAlg = null;

        // Declare the string used to hold
        // the decrypted text.
        byte[] plain;

        try
        {
            // Create a RijndaelManaged object
            // with the specified key and IV.
            aesAlg = new RijndaelManaged { Mode = CipherMode.CBC, KeySize = 256, BlockSize = 128, Key = key, IV = iv };

            // Create the streams used for decryption.
            using (var input = new MemoryStream(encrypted))
            using (var output = new MemoryStream())
            {
                // Create a decrytor to perform the stream transform.
                var decryptor = aesAlg.CreateDecryptor(aesAlg.Key, aesAlg.IV);

                using (CryptoStream csDecrypt = new CryptoStream(input, decryptor, CryptoStreamMode.Read))
                {
                    var buffer = new byte[1024];
                    var read = csDecrypt.Read(buffer, 0, buffer.Length);
                    while (read > 0)
                    {
                        output.Write(buffer, 0, read);
                        read = csDecrypt.Read(buffer, 0, buffer.Length);
                    }
                    csDecrypt.Flush();
                    plain = output.ToArray();
                }
            }
        }
        finally
        {
            // Clear the RijndaelManaged object.
            if (aesAlg != null)
                aesAlg.Clear();
        }

        return plain;
    }

    static string DecryptStringFromBytesAes(byte[] cipherText, byte[] key, byte[] iv)
    {
        // Check arguments.
        if (cipherText == null || cipherText.Length <= 0)
            throw new ArgumentNullException("cipherText");
        if (key == null || key.Length <= 0)
            throw new ArgumentNullException("key");
        if (iv == null || iv.Length <= 0)
            throw new ArgumentNullException("iv");

        // Declare the RijndaelManaged object
        // used to decrypt the data.
        RijndaelManaged aesAlg = null;

        // Declare the string used to hold
        // the decrypted text.
        string plaintext;

        try
        {
            // Create a RijndaelManaged object
            // with the specified key and IV.
            aesAlg = new RijndaelManaged { Mode = CipherMode.CBC, KeySize = 256, BlockSize = 128, Key = key, IV = iv };

            // Create a decrytor to perform the stream transform.
            ICryptoTransform decryptor = aesAlg.CreateDecryptor(aesAlg.Key, aesAlg.IV);
            // Create the streams used for decryption.
            using (MemoryStream msDecrypt = new MemoryStream(cipherText))
            {
                using (CryptoStream csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
                {
                    using (StreamReader srDecrypt = new StreamReader(csDecrypt))
                    {
                        // Read the decrypted bytes from the decrypting stream
                        // and place them in a string.
                        plaintext = srDecrypt.ReadToEnd();
                        srDecrypt.Close();
                    }
                }
            }
        }
        finally
        {
            // Clear the RijndaelManaged object.
            if (aesAlg != null)
                aesAlg.Clear();
        }

        return plaintext;
    }

}