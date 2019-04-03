using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using Aras.Common.Compression;
using Aras.IOM;
using BitAddict.Aras.Data;
using BitAddict.Aras.Security;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MSTestHacks;
using Newtonsoft.Json;

namespace BitAddict.Aras.Test
{
    public class ArasUnitTestBase : TestBase
    {
        protected static Innovator Innovator;
        protected static HttpServerConnection Connection;
        protected static Item LoginItem;
        protected static string LogFolder;

        private static int _connectionCount;

        private static readonly ThreadLocal<List<Item>> TempItemsTl
            = new ThreadLocal<List<Item>>(() => new List<Item>());

        public static List<Item> TempItems => TempItemsTl.Value;

        [AssemblyInitialize]
        public static void AssemblyInitialize(TestContext cxt)
        {
            // do this once-and-for-all when unit testing
            // Logger.EnableConsoleLogging = true;
            Logger.AlwaysIncrementLogNumber = true;
            ArasPermissionGrant.Disable = true;
        }

        [ClassInitialize]
        public static void ClassInitialize(TestContext ctx)
        {
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

            var slnDir = Directory.GetParent(ctx.TestRunDirectory).Parent;
            if (slnDir == null)
                throw new ArasException("Unit test not run from within Visual studio?");

            var developmentDb = GetDevelopmentDb(slnDir);

            Connection = IomFactory.CreateHttpServerConnection(
                developmentDb.Url, developmentDb.DbName,
                loginInfo.Username, loginInfo.Password);

            Connection.Timeout = 15000; // need time for AppPool recycle on new DLLs
            Connection.Compression = CompressionType.none;

            LoginItem = Connection.Login();

            if (LoginItem.isError())
                throw new ArasException("Aras login failed: " + LoginItem.getErrorString());

            Innovator = LoginItem.getInnovator();
            LogFolder = ctx.TestDir; // dont use name, shared by many tests
            ArasExtensions.Innovator = Innovator;
            ArasExtensions.LogRootFolder = LogFolder;
        }

        private static ArasDb GetDevelopmentDb(FileSystemInfo slnDir)
        {
            ArasDb developmentDb = null;

            foreach (var mfFile in new[] {"arasdb-local.json", "arasdb.json"})
            {
                var mfFilePath = Path.Combine(slnDir.FullName, mfFile);
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
                throw new ArasException($"Aras development database not defined " +
                                        $"in arasdb[-local].json? (looking in {slnDir})");
            return developmentDb;
        }

        [ClassCleanup]
        public static void ClassCleanup()
        {
            if (--_connectionCount > 0)
                return;

            Connection?.Logout();
        }

        [TestCleanup]
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
