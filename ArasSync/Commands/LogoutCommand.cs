using System;
using BitAddict.Aras.Security;
using JetBrains.Annotations;
using ManyConsole;

namespace BitAddict.Aras.ArasSyncTool.Commands
{
    /// <summary>
    /// Removes encrypted Aras login information from disk
    /// </summary>
    [UsedImplicitly]
    public class LogoutCommand : ConsoleCommand
    {
        public LogoutCommand()
        {
            IsCommand("Logout", "Removes login information previously stored on disk.");

            HasLongDescription("Removes the encrypted file storing user's Aras login info.");

            SkipsCommandSummaryBeforeRunning();
        }

        public override int Run(string[] remainingArguments)
        {
            if (LoginInfo.Exists())
            {
                Console.WriteLine("Stored login credentials have been removed.");
                LoginInfo.Delete();
            }
            else
            {
                Console.WriteLine("No login info stored. Nothing to do.");
            }

            return 0;
        }
    }
}
