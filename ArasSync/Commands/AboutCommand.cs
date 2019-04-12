// MIT License, see COPYING.TXT
using System;
using BitAddict.Aras.ArasSync.Properties;
using JetBrains.Annotations;
using ManyConsole;

namespace BitAddict.Aras.ArasSync.Commands
{
    /// <summary>
    /// Shows contents of COPYING.TXT
    /// </summary>
    [UsedImplicitly]
    public class AboutCommand : ConsoleCommand
    {
        public AboutCommand()
        {
            IsCommand("About", "Shows full license/copyright notice.");
        }

        public override int Run(string[] remainingArguments)
        {
            Console.WriteLine(Resources.COPYING);
            return 0;
        }
    }
}
