// MIT License, see COPYING.TXT
using System;
using System.IO;
using System.Linq;
using BitAddict.Aras.ArasSync.Ops;
using JetBrains.Annotations;
using ManyConsole;

namespace BitAddict.Aras.ArasSync.Commands
{
    /// <summary>
    /// Export all AML for a feature to disk
    /// </summary>
    [UsedImplicitly]
    public class ExportCommand : ConsoleCommand
    {
        public string ManifestFile { get; set; }
        public string Database { get; set; }
        public string AmlSyncFile { get; set; } = "amlsync.json";

        public ExportCommand()
        {
            IsCommand("Export", "Exports the current directory's feature from Aras database to disk");

            HasOption("mf=|manifest=", "Manifest file to run, otherwise runs *.mf in order",
                f => ManifestFile = f);
            HasRequiredOption("db=|database=", "The Aras instance id to export from",
                db => Database = db);
            HasOption("amlsync=", "The path to the amlsync.json file.", f => AmlSyncFile = f);
        }

        public override int Run(string[] remainingArguments)
        {
            var arasDb = Config.FindDb(Database);
            var loginInfo = Common.RequireLoginInfo();
            var exportDir = Path.Combine(Environment.CurrentDirectory, "ArasExport");
            var manifestFiles = Common.GetArasImportManifestFiles(exportDir, ManifestFile);
            var data = Common.ParseArasFeatureManifest(AmlSyncFile);

            // export each manifest
            foreach (var mfFile in manifestFiles.OrderBy(s => s))
                ConsoleUpgrade.Export(arasDb, loginInfo, exportDir, mfFile);

            // download files
            Console.WriteLine($"\nDownloading {data.ServerFiles.Count} file(s) from {arasDb.BinFolder}...\n");

            foreach (var srvFile in data.ServerFiles)
            {
                var srcFile = Path.Combine(arasDb.BinFolder, srvFile.Remote);
                var dstFile = Path.Combine(data.LocalDirectory, srvFile.Local);

                if (!File.Exists(srcFile))
                {
                    Console.WriteLine("File {file.Remote} not found on server. Skipping");
                    continue;
                }

                Common.CopyFileWithProgress(srcFile, dstFile);
            }

            Console.WriteLine("\nFiles downloaded successfully.\n");

            // extract after export
            return new ExtractAllCommand().Run(new string[] { });
        }
    }
}
