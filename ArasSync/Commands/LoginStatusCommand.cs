// MIT License, see COPYING.TXT
using System;
using BitAddict.Aras.Security;
using JetBrains.Annotations;
using ManyConsole;

namespace BitAddict.Aras.ArasSync.Commands
{
    [UsedImplicitly]
    public class LoginStatusCommand : ConsoleCommand
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
