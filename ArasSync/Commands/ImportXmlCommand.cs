using System;
using System.IO;
using System.Linq;
using System.Xml;
using BitAddict.Aras.ArasSyncTool.Ops;
using JetBrains.Annotations;
using ManyConsole;
using XmlNode = BitAddict.Aras.ArasSyncTool.Data.XmlNode;

namespace BitAddict.Aras.ArasSyncTool.Commands
{
    /// <summary>
    /// Imports XML fragments from arassync.json into Aras installation directory files
    /// </summary>
    [UsedImplicitly]
    public class ImportXmlCommand : ConsoleCommand
    {
        public string Database { get; set; }
        public string AmlSyncFile { get; set; } = "amlsync.json";
        public bool Confirm { get; set; } = true;

        public ImportXmlCommand()
        {
            IsCommand("ImportXml", "Imports the current directory's XML fragments from local disc into an Aras " +
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

            if (!data.XmlFragments.Any())
            {
                Console.WriteLine("\nNo files specified for XML importation to server...\n");
                return 0;
            }

            if (Confirm)
                Common.RequestUserConfirmation($"import XML fragments from '{featureName}' into Aras installation" +
                                               $" at {arasDb.BinFolder}");

            Console.WriteLine($"\nModifying {data.XmlFragments.Count} file(s) in {arasDb.BinFolder}...\n");

            foreach (var fragment in data.XmlFragments)
            {
                if (fragment.RemoteFile == null)
                    throw new InvalidOperationException(nameof(fragment.RemoteFile) + " is empty for in " +
                                                        fragment.Nodes.First().File);

                var remoteFilePath = Path.Combine(arasDb.BinFolder, fragment.RemoteFile);
                var localFilePath = Path.GetTempFileName();

                if (!File.Exists(remoteFilePath))
                {
                    Console.WriteLine($"File {fragment.RemoteFile} not found on server. Skipping");
                    continue;
                }

                Console.WriteLine($"  Downloading {remoteFilePath} to {localFilePath}");
                Common.CopyFileWithProgress(remoteFilePath, localFilePath);

                Console.WriteLine($"  Reading {localFilePath}");
                var doc = new XmlDocument();
                doc.Load(localFilePath);

                bool MergeXml(XmlNode node)
                {
                    if (node.File != null && node.Fragment != null)
                        throw new InvalidOperationException("Only one of File and Fragment can be set!");

                    var xmlFragment = node.File != null
                        ? File.ReadAllText(node.File)
                        : node.Fragment;
                    return Xml.MergeFileIntoXml(doc, xmlFragment, node.ExistenceXPath, node.AdditionXPath);
                }

                var localFileHasChanged = fragment.Nodes.Select(MergeXml)
                    .ToList().Any(didChange => didChange);

                if (localFileHasChanged)
                {
                    Console.WriteLine($"  Saving {localFilePath}");
                    Xml.SaveFormattedXmlFile(doc, localFilePath);

                    var backupFileName =
                        $"{Path.GetFileNameWithoutExtension(remoteFilePath)}-" +
                        $"{DateTime.Now:yyyyMMddHHmmss}" +
                        $"{Path.GetExtension(remoteFilePath)}";
                    var backupFilePath = Path.Combine(Path.GetDirectoryName(remoteFilePath) ?? "", backupFileName);
                    Console.WriteLine($"  Renaming {remoteFilePath} to {backupFilePath}");
                    File.Move(remoteFilePath, backupFilePath);

                    Console.WriteLine($"  Uploading {localFilePath} to {remoteFilePath}");
                    Common.CopyFileWithProgress(localFilePath, remoteFilePath);
                }
                else
                {
                    Console.WriteLine("  No changes detected");
                }

                Console.WriteLine($"  Deleting {localFilePath}");
                File.Delete(localFilePath);
            }

            Console.WriteLine("\nFiles merged and uploaded successfully.");

            return 0;
        }
    }
}