using System;
using System.IO;
using System.Xml.Linq;
using System.Xml.XPath;
using BitAddict.Aras.Data;
using BitAddict.Aras.Security;

namespace BitAddict.Aras.ArasSyncTool.Ops
{
    class ConsoleUpgrade
    {
        internal static void Import(ArasDb arasDb, LoginInfo loginInfo, string exportDir,
            string mfFile, string releaseInfo, int timeout = 30000)
        {
            Console.WriteLine($"Importing {Path.GetFileName(mfFile)}...\n");

            string nullStr = null;
            var rval = Common.RunProcess("ConsoleUpgrade", false, ref nullStr,
                $"server={arasDb.Url}",
                $"database={arasDb.DbName}",
                $"login={loginInfo.Username}",
                $"password={loginInfo.Password}",
                "import",
                $"dir={exportDir}",
                $"mfFile={mfFile}",
                $"release={releaseInfo}",
                "merge",
                $"timeout={timeout}");

            if (rval != 0)
                throw new UserMessageException($"ConsoleUpgrade.exe failed with {rval}.");
        }

        internal static void Export(ArasDb arasDb, LoginInfo loginInfo, string exportDir,
            string mfFile, int timeout = 30000)
        {
            string pkgName;
            using (var fs = new FileStream(mfFile, FileMode.Open))
            {
                var xd = XDocument.Load(fs);
                pkgName = xd.Root?.XPathSelectElement("//package").Attribute("name")?.Value ?? "";

                if (pkgName.StartsWith("com.aras") || pkgName.StartsWith("Minerva")) // set in arasdb.json?   
                {
                    Console.WriteLine($"Not exporting package {pkgName}, as partial export is not supported");
                    return;
                }
            }

            Console.WriteLine($"Exporting package {pkgName} via {Path.GetFileName(mfFile)} ...");

            string nullStr = null;
            var rval = Common.RunProcess("ConsoleUpgrade", false, ref nullStr,
                $"server={arasDb.Url}",
                $"database={arasDb.DbName}",
                $"login={loginInfo.Username}",
                $"password={loginInfo.Password}",
                // "export",
                $"dir={exportDir}",
                $"mfFile={mfFile}",
                "verbose",
                $"timeout={timeout}");

            if (rval != 0)
                throw new UserMessageException($"ConsoleUpgrade.exe failed with {rval}.");
        }
    }
}
