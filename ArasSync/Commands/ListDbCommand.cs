// MIT License, see COPYING.TXT
using System;
using System.IO;
using BitAddict.Aras.ArasSync.Ops;
using BitAddict.Aras.Data;
using JetBrains.Annotations;
using ManyConsole;
using Newtonsoft.Json;

namespace BitAddict.Aras.ArasSync.Commands
{
    /// <summary>
    /// Lists Aras database information in local arasdb.json file
    /// </summary>
    [UsedImplicitly]
    public class ListDbCommand : ConsoleCommand
    {
        public bool ShortFormat { get; set; }

        public ListDbCommand()
        {
            IsCommand("ListDB", "List Aras instances specified in arasdb.json file(s)");

            HasOption("shortformat", "Show only DB Ids, not full data", b => ShortFormat = true);
        }

        public override int Run(string[] remainingArguments)
        {
            var slnDir = Config.SolutionDir.FullName;
            var foundUnitTest = false;
            Console.WriteLine();

            foreach (var mfFile in new[] { "arasdb-local.json", "arasdb.json" })
            {
                var mfFilePath = Path.Combine(slnDir, mfFile);

                if (!ShortFormat)
                    Console.WriteLine($"{mfFilePath}:");
                else
                    Console.Write($"{mfFile, -18}:");

                if (!File.Exists(mfFilePath))
                {
                    Console.WriteLine("Not found");
                    continue;
                }

                var json = File.ReadAllText(mfFilePath);
                var mf = JsonConvert.DeserializeObject<ArasConfManifest>(json);

                foreach (var db in mf.Instances)
                {
                    if (ShortFormat)
                    {
                        Console.Write($" {db.Id}");
                        if (db.Id != mf.DevelopmentInstance || foundUnitTest)
                            continue;

                        Console.Write('*');
                        foundUnitTest = true;
                    }
                    else
                    {
                        Console.WriteLine($"  {db.Id}");
                        Console.WriteLine($"    url:        {db.Url}");
                        Console.WriteLine($"    db name:    {db.DbName}");
                        Console.WriteLine($"    bin folder: {db.BinFolder}");

                        if (db.Id != mf.DevelopmentInstance || foundUnitTest)
                            continue;

                        Console.WriteLine("    *** Will be used by tests ***");
                        foundUnitTest = true;
                    }
                }

                Console.WriteLine();
            }

            return 0;
        }
    }
}
