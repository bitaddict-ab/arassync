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
    /// Uploads the current feature's server files to Aras installation directory
    /// </summary>
    [UsedImplicitly]
    public class ImportFilesCommand : ConsoleCommand
    {
        public string Database { get; set; }
        public string AmlSyncFile { get; set; } = "amlsync.json";
        public bool Confirm { get; set; } = true;

        public ImportFilesCommand()
        {
            IsCommand("ImportFiles", "Upload the current feature's server files from local disc into the Aras " +
                "installation directory");

            HasRequiredOption("db=|database=", "The Aras instance id to import into", db => Database = db);
            HasOption("amlsync=", "The path to the amlsync.json file.", f => AmlSyncFile = f);

            HasOption("noconfirm", "Disable user confirmation", _ => Confirm = false);
        }

        public override int Run(string[] remainingArguments)
        {
            var arasDb = Config.FindDb(Database);
            var featureName = Common.GetFeatureName();
            var data = Common.ParseArasFeatureManifest(AmlSyncFile);

            if (!data.ServerFiles.Any())
            {
                Console.WriteLine($"\nNo files specified for raw upload to server...\n");
                return 0;
            }

            if (Confirm)
                Common.RequestUserConfirmation($"upload the files from '{featureName}' into Aras installation directory {arasDb.BinFolder}");

            Console.WriteLine($"\nUploading {data.ServerFiles.Count} file(s) to {arasDb.BinFolder}...\n");

            foreach (var file in data.ServerFiles)
            {
                var srcFile = Path.Combine(data.LocalDirectory, file.Local);
                var dstFile = Path.Combine(arasDb.BinFolder, file.Remote);

                var dstDir = Path.GetDirectoryName(dstFile);
                if (dstDir != null && !Directory.Exists(dstDir))
                {
                    Console.WriteLine("  Creating " + dstDir + " ...");
                    Directory.CreateDirectory(dstDir);
                } 

                Common.CopyFileWithProgress(srcFile, dstFile);
            }

            Console.WriteLine("\nFiles uploaded successfully.");

            return 0;
        }
    }
}
