// MIT License, see COPYING.TXT
using System.IO;
using System.Threading;
using BitAddict.Aras.Security;
using Microsoft.VisualStudio.TestTools.UnitTesting;

#pragma warning disable 1591

namespace BitAddict.Aras.UnitTests
{
    [TestClass]
    public class TestLoginInfo
    {
        private readonly string _tempKeyFilePath = Path.GetTempFileName();

        [TestInitialize]
        public void TestInitialize()
        {
            // Use locking on LoginInfo type to prevent more than one test to run on LoginInfo at a time, due to static data
            // could probably be done better, extract KeyFilePath into some mockable "PathFactory" or so..
            Monitor.Enter(typeof(LoginInfo));
            LoginInfo.KeyFilePath = _tempKeyFilePath;
            File.Delete(_tempKeyFilePath); // remove temp file
        }

        [TestCleanup]
        public void TestCleanup()
        {
            try
            {
                if (File.Exists(_tempKeyFilePath))
                    File.Delete(_tempKeyFilePath);

                LoginInfo.KeyFilePath = null;
            }
            finally
            {
                Monitor.Exit(typeof (LoginInfo));
            }
        }

        [TestMethod]
        public void TestNothingExists()
        {
            Assert.IsFalse(LoginInfo.Exists());
        }

        [TestMethod]
        public void TestNothingIsValid()
        {
            Assert.IsFalse(LoginInfo.IsValid());
        }


        [TestMethod]
        public void TestValidAndExistsAfterStore()
        {
            var loginInfo = new LoginInfo {Username = "bob", Password = "secret"};
            loginInfo.Store();
            Assert.IsTrue(LoginInfo.Exists());
            Assert.IsTrue(LoginInfo.IsValid());
        }

        [TestMethod]
        public void TestDoesNotExistsAfterDelete()
        {
            var loginInfo = new LoginInfo { Username = "bob", Password = "secret" };
            loginInfo.Store();
            LoginInfo.Delete();
            Assert.IsFalse(LoginInfo.Exists());
        }

        [TestMethod]
        public void TestLoginInfoIsRestored()
        {
            var loginInfo = new LoginInfo { Username = "bob", Password = "secret" };
            loginInfo.Store();
            var loginInfo2 = LoginInfo.Load();
            Assert.IsNotNull(loginInfo2);

            Assert.AreEqual(loginInfo.Username, loginInfo2.Username);
            Assert.AreEqual(loginInfo.Password, loginInfo2.Password);
        }


        [TestMethod]
        public void TestFileIsEncrypted()
        {
            var loginInfo = new LoginInfo { Username = "bob", Password = "secret" };
            loginInfo.Store();

            var cipherText = File.ReadAllText(LoginInfo.KeyFilePath);
            Assert.IsFalse(cipherText.Contains(loginInfo.Username));
            Assert.IsFalse(cipherText.Contains(loginInfo.Password));
        }

        [TestMethod]
        public void TestBogusFileIsNotValid()
        {
            File.WriteAllText(LoginInfo.KeyFilePath, "---bobs secret---");
            Assert.IsTrue(LoginInfo.Exists());
            Assert.IsFalse(LoginInfo.IsValid());
        }
    }
}
