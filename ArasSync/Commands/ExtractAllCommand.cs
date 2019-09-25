// MIT License, see COPYING.TXT
using System;
using System.IO;
using System.Xml;
using BitAddict.Aras.ArasSync.Ops;
using ManyConsole;

namespace BitAddict.Aras.ArasSync.Commands
{
    /// <summary>
    /// Extract xml-tags from AML file and writes to on disk file
    /// </summary>
    [CommandCategory("Advanced")]
    public class ExtractAllCommand : ConsoleCommand
    {
        public string AmlSyncFile { get; set; } = "amlsync.json";

        public ExtractAllCommand()
        {
            IsCommand("ExtractAll", "Extract all xml-tags specified in amlsync.json from AML and writes to on-disk code file");

            HasLongDescription("Extracts XML element contents to on-disc files " +
                               "(editable in Visual Studio)\n" +
                               "from raw AML file that Aras imports/exports.");

            HasOption("amlsync=", "The path to the amlsync.json file.", f => AmlSyncFile = f);
        }

        public override int Run(string[] remainingArguments)
        {
            var data = Common.ParseArasFeatureManifest(AmlSyncFile);

            Console.WriteLine("Extracting AML into local files...\n");

            foreach (var aml in data.AmlFragments)
            {
                Console.WriteLine($"  Loading {aml.AmlFile}...");

                // ReSharper disable once AssignNullToNotNullAttribute
                var amlFile = Path.Combine(data.LocalDirectory, aml.AmlFile);

                if (!File.Exists(amlFile))
                    continue;

                var doc = new XmlDocument();
                doc.Load(amlFile);

                foreach (var node in aml.Nodes) 
                    Xml.ExtractInnerTextToFile(doc, node.File, node.XPath);
            }

            return 0;
        }
    }
}
