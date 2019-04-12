// MIT License, see COPYING.TXT
using System;
using BitAddict.Aras.ArasSync.Ops;
using JetBrains.Annotations;
using ManyConsole;

namespace BitAddict.Aras.ArasSync.Commands
{
    /// <summary>
    /// Extract single AML tag from file and writes to disk
    /// </summary>
    [CommandCategory("Advanced")]
    [UsedImplicitly]
    public class ExtractAmlCommand : ConsoleCommand
    {
        public string AmlFile { get; set; }
        public string CodeFile { get; set; }

        public string XPathExpr { get; set; }

        public ExtractAmlCommand()
        {
            IsCommand("ExtractAML", "Extract xml-tag from AML and writes to on-disk code file");

            HasLongDescription("Synchronizes on-disc files (editable in Visual Studio) " +
                               "into the raw AML files that Aras imports/exports.\n" +
                               "Files are placed as CDATA wrapped text contents inside " +
                               "the specified XML element, just as Aras does.");

            HasRequiredOption("aml=", "The full path to the AML file.", f => AmlFile = f);
            HasRequiredOption("file=", "The full path to the code file.", f => CodeFile = f);
            HasRequiredOption("xpath=", "An XPath expression naming where the code file should be inserted", e => XPathExpr = e);
        }

        public override int Run(string[] remainingArguments)
        {
            Xml.ExtractInnerTextToFile(AmlFile, CodeFile, XPathExpr);

            Console.WriteLine("Successfully extracted AML-part into file!");

            return 0;
        }
    }
}
