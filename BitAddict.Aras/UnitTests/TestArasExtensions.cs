// MIT License, see COPYING.TXT
using System;
using System.IO;
using System.Linq;
using BitAddict.Aras.Test;
using Microsoft.VisualStudio.TestTools.UnitTesting;
#pragma warning disable 1591

namespace BitAddict.Aras.UnitTests
{
    [TestClass]
    public class TestArasExtensions : ArasUnitTestBase
    {
        private string _logfile;

        [TestInitialize]
        public void TestInitialize()
        {
        }


        [TestCleanup]
        public void TestCleanup()
        {
            if (_logfile != null)
                File.Delete(_logfile);
        }

        [TestMethod]
        public void TestLogFileIsCorrectForDatabase()
        {
            ArasExtensions.CallMethod(nameof(TestLogFileIsCorrectForDatabase),
                i =>
            {
                _logfile = ArasExtensions.Logger.LogFile;
                Console.WriteLine(_logfile);
                return i;
            }, LoginItem);

            StringAssert.StartsWith(_logfile, LogFolder);
            StringAssert.Contains(_logfile, $"\\{Connection.GetDatabaseName()}",
                "Logfile path doesn't contain database name {0}: {1}",
                Connection.GetDatabaseName(), _logfile);

            var text = File.ReadAllText(_logfile);
            Console.WriteLine("\n" + string.Join("\n", text.Split('\n').Take(2)));

            StringAssert.Contains(text, Connection.GetDatabaseName(),
                "Text doesn't contain database name {0}: {1}",
                Connection.GetDatabaseName(), text);
        }

        [ClassInitialize]
        public static new void ClassInitialize(TestContext cxt)
        {
            ArasUnitTestBase.ClassInitialize(cxt);
        }

        [ClassCleanup]
        public static new void ClassCleanup()
        {
            ArasUnitTestBase.ClassCleanup();
        }
    }
}
