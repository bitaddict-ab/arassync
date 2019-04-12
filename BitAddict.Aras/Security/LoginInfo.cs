// MIT License, see COPYING.TXT
using JetBrains.Annotations;
using System;
using System.Diagnostics;
using System.IO;
using System.Text;

namespace BitAddict.Aras.Security
{
    /// <summary>
    /// Stores developers login/password to Aras in a secure way
    /// to allow arassync / integration tests to access Aras repeatedly.
    ///
    /// Currently stored encrypted under User\AppData\Local.
    ///
    /// Does not keep string secure in memory at the moment,
    /// as it is sent to IOM.dll unencrypted anyway.
    /// </summary>
    public class LoginInfo
    {
        /// <summary>
        /// User name
        /// </summary>
        public string Username { get; set; } = "";
        /// <summary>
        /// Unencrypted password
        /// </summary>
        public string Password { get; set; } = "";

        private const string KeyFilePassword = "€€Aras.FTW.321!!åäö";
        private static readonly byte[] KeyFilePayload = Encoding.UTF8.GetBytes("scramble.all.the.eggs!");

        private static string _keyFilePath;

        /// <summary>
        /// Path to encrytped key file
        /// </summary>
        public static string KeyFilePath
        {
            get
            {
                if (_keyFilePath != null) return _keyFilePath;

                var appDataLocal = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
                _keyFilePath = Path.Combine(appDataLocal, "BitAddict", "ArasSyncTool", "login.crypt");

                return _keyFilePath;
            }
            internal set => _keyFilePath = value;
        }

        /// <summary>
        /// Stores login info in key file
        /// </summary>
        public void Store()
        {
            var plainText = $"{Username}\0{Password}";
            var cipherText = Encryption.SimpleEncryptWithPassword(plainText, KeyFilePassword, KeyFilePayload);

            var keyFileDir = Path.GetDirectoryName(KeyFilePath);
            Debug.Assert(keyFileDir != null);

            if (!Directory.Exists(keyFileDir))
                // ReSharper disable once AssignNullToNotNullAttribute
                Directory.CreateDirectory(keyFileDir);

            File.WriteAllText(KeyFilePath, cipherText);
        }

        /// <summary>
        /// Loads login info from key file
        /// </summary>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        [CanBeNull]
        public static LoginInfo Load()
        {
            try
            {
                var cipherText = File.ReadAllText(KeyFilePath);
                if (string.IsNullOrEmpty(cipherText))
                    return null;

                var plainText = Encryption.SimpleDecryptWithPassword(cipherText, KeyFilePassword, KeyFilePayload.Length);

                var parts = plainText.Split('\0');
                if (parts.Length != 2)
                    throw new Exception("The stored password file is invalid.");

                return new LoginInfo {Username = parts[0], Password = parts[1]};
            }
            catch (Exception e)
            {
                Debug.WriteLine(e.GetType() + ": " + e.Message);
                return null;
            }
        }

        /// <summary>
        /// Delete key file
        /// </summary>
        public static void Delete()
        {
            File.Delete(KeyFilePath);
        }

        /// <summary>
        /// Check if key file exists
        /// </summary>
        /// <returns></returns>
        public static bool Exists()
        {
            return File.Exists(KeyFilePath);
        }

        /// <summary>
        /// Loads and decrypts key file, then checks that it contains some info
        /// </summary>
        /// <returns></returns>
        public static bool IsValid()
        {
            var loginInfo = Load();
            if (loginInfo == null)
                return false;

            return !string.IsNullOrWhiteSpace(loginInfo.Username) &&
                    !string.IsNullOrWhiteSpace(loginInfo.Password);
        }
    }
}
