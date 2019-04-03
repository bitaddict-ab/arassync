using System;
using System.IO;
using System.Linq;
using BitAddict.Aras.ArasSyncTool.Ops;
using ManyConsole;
using static ManyConsole.ConsoleCommandDispatcher;

namespace BitAddict.Aras.ArasSyncTool.Commands
{
    [CommandCategory("Advanced")]
    class ForAllCommand : ConsoleCommand
    {
        public ForAllCommand()
        {
            IsCommand("ForAll", "Runs an 'arassync' command in every feature directory");

            HasLongDescription("Allows mass operations for all Aras features in a directory.\n" +
                               "(For instance build/deploy all DLLs when a common base assembly has changed.)\n\n" +
                               "Aborts on first failed command.");

            AllowsAnyAdditionalArguments(" <command name> [arguments, ...]");
        }

        public override int Run(string[] remainingArguments)
        {
            if (!remainingArguments.Any())
                throw new UserMessageException("Requires at least one argument");

            var dirs = Directory.EnumerateDirectories(Config.SolutionDir.FullName)
                .Where(d => File.Exists(Path.Combine(d, "amlsync.json")))
                .ToList();

            var commands = FindCommandsInSameAssemblyAs(typeof(Program)).ToList();

            foreach (var p in dirs.Select((d, i) => new {Dir = d, Index = i}))
            {
                Console.WriteLine($"**** Executing '{string.Join(" ", remainingArguments)}' in '{Path.GetFileName(p.Dir)}' ({p.Index+1}/{dirs.Count}) *****");
                Environment.CurrentDirectory = p.Dir;

                var r = DispatchCommand(commands, remainingArguments, Console.Out);
                if (r != 0)
                    return r;
            }

            return 0;
        }
    }
}
