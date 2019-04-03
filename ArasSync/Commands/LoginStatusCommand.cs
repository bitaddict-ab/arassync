using System;
using System.Linq;
using BitAddict.Aras.Security;
using ManyConsole;

namespace BitAddict.Aras.ArasSyncTool.Commands
{
    class LoginStatusCommand : ConsoleCommand
    {
        public LoginStatusCommand()
        {
            IsCommand("LoginStatus", "Returns 0 (ok) if user is logged in or 1 if not.");

            SkipsCommandSummaryBeforeRunning();
        }

        public override int Run(string[] remainingArguments)
        {
            var loginInfo = LoginInfo.Load();
            Console.WriteLine(loginInfo == null
                ? "No login info found"
                : $"Login info found for user '{loginInfo.Username}'.");

            return loginInfo != null ? 0 : 1;
        }
    }
}
