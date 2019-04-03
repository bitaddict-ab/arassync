using System;
using System.IO;
using System.Linq;
using BitAddict.Aras.ArasSyncTool.Ops;
using ManyConsole;

namespace BitAddict.Aras.ArasSyncTool.Commands
{
    public class ImportAMLCommand : ConsoleCommand
    {
        public string ManifestFile { get; set; }
        public string Database { get; set; }
        public string AmlSyncFile { get; set; } = "amlsync.json";
        public bool Confirm { get; set; } = true;

        public ImportAMLCommand()
        {
            IsCommand("ImportAML", "Imports the current directory's AML from local disc into an Aras database");

            HasRequiredOption("db=|database=", "The Aras instance id to import into", db => Database = db);
            HasOption("mf=|manifest=", "Manifest file to run, otherwise runs *.mf in order", f => ManifestFile = f);
            HasOption("amlsync=", "The path to the amlsync.json file.", f => AmlSyncFile = f);

            HasOption("noconfirm", "Disable user confirmation", _ => Confirm = false);
        }

        public override int Run(string[] remainingArguments)
        {
            var gitInfo = Git.GetRepoStatusString();
            var arasDb = Config.FindDb(Database);
            var loginInfo = Common.RequireLoginInfo();
            var featureName = Common.GetFeatureName();

            // merge AML before import into aras
            new MergeAllCommand {AmlSyncFile = AmlSyncFile}.Run(new string[] { });

            if (Confirm)
                Common.RequestUserConfirmation($"import '{featureName}' into Aras DB {arasDb.Id}");

            var exportDir = Path.Combine(Environment.CurrentDirectory, "ArasExport");
            var manifestFiles = Common.GetArasImportManifestFiles(exportDir, ManifestFile);

            foreach(var mfFile in manifestFiles.OrderBy(s => s))
                ConsoleUpgrade.Import(arasDb, loginInfo, exportDir, mfFile, gitInfo);

            return 0;
        }
    }
}
