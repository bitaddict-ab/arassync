using System;
using BitAddict.Aras.ArasSyncTool.Ops;
using JetBrains.Annotations;
using ManyConsole;

namespace BitAddict.Aras.ArasSyncTool.Commands
{
    /// <summary>
    /// Updates an xml-tag in AML from on-disk code file
    /// </summary>
    [CommandCategory("Advanced")]
    [UsedImplicitly]
    public class MergeAmlCommand : ConsoleCommand
    {
        public string AmlFile { get; set; }
        public string CodeFile { get; set; }

        public string XPathExpr { get; set; }

        public MergeAmlCommand()
        {
            IsCommand("MergeAML", "Updates an xml-tag in AML from on-disk code file");

            HasLongDescription("Synchronizes on-disc files (Editable in Visual Studio) " +
                               "into the raw AML files that Aras imports/exports.\n" +
                               "Files are placed as CDATA wrapped text contents inside " +
                               "the specified XML element, just as Aras does.");

            HasRequiredOption("aml=", "The full path to the AML file.", f => AmlFile = f);
            HasRequiredOption("file=", "The full path to the code file.", f => CodeFile = f);
            HasRequiredOption("xpath=", "An XPath expression naming where the code file should be inserted", e => XPathExpr = e);
        }

        public override int Run(string[] remainingArguments)
        {
            Xml.MergeFileIntoCData(AmlFile, CodeFile, XPathExpr);

            Console.WriteLine("Successfully merged code into AML.");

            return 0;
        }
    }
}
