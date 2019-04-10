using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using Aras.Common.Compression;
using Aras.IOM;
using BitAddict.Aras.Data;
using BitAddict.Aras.Security;
using Newtonsoft.Json;
using NUnit.Framework;

namespace BitAddict.Aras.Test
{
    /// <summary>
    /// Base class for NUnit Aras tests
    /// </summary>
    public class ArasNUnitTestBase
    {
        protected static Innovator Innovator { get; private set; }
        protected static HttpServerConnection Connection { get; private set; }
        protected static Item LoginItem { get; private set; }
        protected static string LogFolder { get; private set; }

        private static int _connectionCount;

        private static readonly ThreadLocal<List<Item>> TempItemsTl
            = new ThreadLocal<List<Item>>(() => new List<Item>());

        /// <summary>
        /// Temporary items to be removed at end of test
        /// </summary>
        public static List<Item> TempItems => TempItemsTl.Value;

        /// <summary>
        /// Connects to Aras test DB
        /// </summary>
        [OneTimeSetUp]
        public static void OneTimeSetUp()
        {
            // do this once-and-for-all when unit testing
            // Logger.EnableConsoleLogging = true;
            Logger.AlwaysIncrementLogNumber = true;
            ArasPermissionGrant.Disable = true;
            _connectionCount++;

            if (_connectionCount > 1)
            {
                if (Innovator == null)
                    throw new ArasException("ArasTestBase Setup failed.");
                return;
            }

            var loginInfo = LoginInfo.Load();
            if (loginInfo == null)
                throw new ArasException("No user logged in. Cannot run tests against Aras.");

            var slnDir = TestContext.CurrentContext.TestDirectory;
            while (slnDir != null && !Directory.EnumerateFiles(slnDir, "*.sln").Any())
                slnDir = Directory.GetParent(slnDir).FullName;

            if (slnDir == null)
                throw new ArasException(
                    $"Failed to find top/solution directory in parents of {TestContext.CurrentContext.TestDirectory}");

            Console.WriteLine(slnDir);

            var developmentDb = GetDevelopmentDb(slnDir);

            Connection = IomFactory.CreateHttpServerConnection(
                developmentDb.Url, developmentDb.DbName,
                loginInfo.Username, loginInfo.Password);

            Connection.Timeout = 15000; // need time for AppPool recycle on new DLLs
            Connection.Compression = CompressionType.deflate;

            LoginItem = Connection.Login();

            if (LoginItem.isError())
                throw new ArasException("Aras login failed: " + LoginItem.getErrorString());

            Innovator = LoginItem.getInnovator();
            LogFolder = TestContext.CurrentContext.WorkDirectory;
            ArasExtensions.Innovator = Innovator;
            ArasExtensions.LogRootFolder = LogFolder;
        }

        private static ArasDb GetDevelopmentDb(string slnDir)
        {
            ArasDb developmentDb = null;

            foreach (var mfFile in new[] {"arasdb-local.json", "arasdb.json"})
            {
                var mfFilePath = Path.Combine(slnDir, mfFile);
                if (!File.Exists(mfFilePath))
                    continue;

                try
                {
                    var json = File.ReadAllText(mfFilePath);
                    var arasdbmf = JsonConvert.DeserializeObject<ArasConfManifest>(json);
                    developmentDb = arasdbmf.Instances.Single(db => db.Id == arasdbmf.DevelopmentInstance);
                    break;
                }
                catch (Exception e)
                {
                    Console.WriteLine($"{mfFile}: {e.Message}");
                }
            }

            if (developmentDb == null)
                throw new ArasException("Aras development database not properly defined in arasdb[-local].json");
            return developmentDb;
        }

        /// <summary>
        /// Closes connection 
        /// </summary>
        [OneTimeTearDown]
        public static void ClassCleanup()
        {
            if (--_connectionCount > 0)
                return;

            Connection?.Logout();
        }

        /// <summary>
        /// Removes temporary items
        /// </summary>
        [TearDown]
        public void Cleanup()
        {
            if (!TempItems.Any())
                return;

            Exception ex = null;
            Console.WriteLine($"Cleaning up {TempItems.Count} temp item(s) from Aras DB...\n");

            foreach(var item in TempItems)
            {
                try
                {
                    // create new item to reduce log spam
                    var delItem = Innovator.newItem(item.getType(), "delete");
                    delItem.setID(item.getID());
                    delItem.setProperty("keyed_name", item.getProperty("keyed_name"));
                    if (item.getProperty("affected_id", "") != "")
                    {
                        delItem.setProperty("affected_id", item.getProperty("affected_id"));
                        delItem.setPropertyAttribute("affected_id", "keyed_name",
                            item.getPropertyAttribute("affected_id", "keyed_name"));
                    }
                    Innovator.ApplyItem(delItem, false);
                }
                catch (Exception e)
                {
                    Console.Error.WriteLine(e.Message);
                    ex = e; // lazy, but one exception is better than none on multiple failures
                }
            }

            TempItems.Clear();

            if (ex != null)
                throw ex;
        }
    }
}
