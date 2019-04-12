// MIT License, see COPYING.TXT
using BitAddict.Aras.ArasSync.Ops;
using JetBrains.Annotations;
using ManyConsole;

namespace BitAddict.Aras.ArasSync.Commands
{
    /// <summary>
    /// Deploys the current directory's Aras feature into to an Aras instance
    /// </summary>
    [UsedImplicitly]
    public class DeployCommand : ConsoleCommand
    {
        public string ManifestFile { get; set; }
        public string Database { get; set; }
        public string AmlSyncFile { get; set; } = "amlsync.json";

        public DeployCommand()
        {
            IsCommand("Deploy", "Deploy the current directory's feature into the Aras instance");

            HasOption("mf=|manifest=", "Manifest file to run, otherwise runs *.mf in order", f => ManifestFile = f);
            HasRequiredOption("db=|database=", "The Aras instance id to import into", db => Database = db);
            HasOption("amlsync=", "The path to the amlsync.json file.", f => AmlSyncFile = f);
        }

        public override int Run(string[] remainingArguments)
        {
            Config.FindDb(Database); // check that DB exists

            var featureName = Common.GetFeatureName();
            Common.RequestUserConfirmation($"deploy all aspects of {featureName} to {Database}");

            new ImportAmlCommand
            {
                ManifestFile = ManifestFile,
                Database = Database,
                AmlSyncFile = AmlSyncFile,
                Confirm = false,
            }.Run(new string[] { });
            new ImportXmlCommand
            {
                Database = Database,
                AmlSyncFile = AmlSyncFile,
                Confirm = false,
            }.Run(new string[] { });
            new ImportFilesCommand
            {
                Database = Database,
                AmlSyncFile = AmlSyncFile,
                Confirm = false,
            }.Run(new string[] { });
            new CopyDllCommand
            {
                Database = Database,
                Confirm = false,
            }.Run(new string[] { });

            return 0;
        }
    }
}
