using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using ManyConsole;
using static ManyConsole.ConsoleCommandDispatcher;

namespace BitAddict.Aras.ArasSyncTool
{
    internal static class Program
    {
        private static int Main(string[] args)
        {
            try
            {
                var commands = FindCommandsInSameAssemblyAs(typeof (Program)).ToList();

                if ((!args.Any() || args[0].EndsWith("help")) && args.Length == 1)
                    return ShowHelp(commands);

                return DispatchCommand(commands, args, Console.Out);
            }
            catch (UserMessageException e)
            {
                Console.Error.WriteLine(e.Message);
                return 1;
            }
            catch (Exception ex)
            {
                var stack = new Stack<Exception>(new []{ex});

                while (stack.Any())
                {
                    var e = stack.Pop();

                    if (e is AggregateException ae)
                    {
                        foreach(var ie in ae.InnerExceptions)
                            stack.Push(ie);
                        continue;
                    }

                    Console.Error.WriteLine($"{e.GetType()}: {e.Message}");
                    Console.Error.WriteLine(e.StackTrace);

                    if (e.InnerException != null)
                    {
                        stack.Push(e.InnerException);
                    }
                }

                if (Debugger.IsAttached)
                    Debugger.Break();

                return 3;
            }
        }

        private static int ShowHelp(IEnumerable<ConsoleCommand> commands)
        {
            foreach (var g in commands
                .GroupBy(c => c.GetType().GetCustomAttribute<CommandCategoryAttribute>()?.Category ?? "Standard")
                .OrderBy(g => g.Key))
            {
                Console.Write(g.Key);
                DispatchCommand(g, new string[] {}, Console.Out);
            }
            return 0;
        }
    }
}
