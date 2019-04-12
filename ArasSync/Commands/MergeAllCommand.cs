// MIT License, see COPYING.TXT
using System;
using System.IO;
using System.Xml;
using BitAddict.Aras.ArasSync.Ops;
using JetBrains.Annotations;
using ManyConsole;

namespace BitAddict.Aras.ArasSync.Commands
{
    /// <summary>
    /// Merges all xml tags in amlsync.json into local AML files.
    /// </summary>
    [CommandCategory("Advanced")]
    [UsedImplicitly]
    public class MergeAllCommand : ConsoleCommand
    {
        public string AmlSyncFile { get; set; } = "amlsync.json";

        public MergeAllCommand()
        {
            IsCommand("MergeAll", "Merges all xml-tags specifed in amlsync.json from files into AML file.");

            HasLongDescription("Synchronizes on-disc files (editable in Visual Studio) " +
                               "into the raw AML files that Aras imports/exports.\n" +
                               "Files are placed as CDATA wrapped text contents inside " +
                               "the specified XML element, just as Aras does.");

            HasOption("amlsync=", "The path to the amlsync.json file.", f => AmlSyncFile = f);
        }

        public override int Run(string[] remainingArguments)
        {
            var data = Common.ParseArasFeatureManifest(AmlSyncFile);

            Console.WriteLine("Merging local files into AML ...\n");

            foreach (var aml in data.AmlFragments)
            {
                // ReSharper disable once AssignNullToNotNullAttribute
                var amlFile = Path.Combine(data.LocalDirectory, aml.AmlFile);

                Console.WriteLine($"  Reading {aml.AmlFile}");
                var doc = new XmlDocument();
                doc.Load(amlFile);

                foreach (var node in aml.Nodes)
                    Xml.MergeFileIntoCData(doc, node.File, node.XPath);

                using (var w = Xml.GetFormattedXmlWriter(amlFile))
                    doc.WriteTo(w);

                Console.WriteLine($"    ... Ok!");
            }

            return 0;
        }
    }
}
