// MIT License, see COPYING.TXT
using System;
using BitAddict.Aras.Security;
using JetBrains.Annotations;
using ManyConsole;

namespace BitAddict.Aras.ArasSync.Commands
{
    /// <summary>
    /// Allows user to enter Aras credentials which are used to log in, run tests and read/write items in database
    /// </summary>
    [UsedImplicitly]
    public class LoginCommand : ConsoleCommand
    {
        public string Username { get; set; }
        public string Password { get; set; }
        public bool UseLoginInfo { get; set; }

        public LoginCommand()
        {
            IsCommand("Login", "Asks user for Aras name/password and stores encrypted on disk.");

            HasLongDescription("Unless given on commandline, prompts user to enter user/password " +
                               "to use for Aras DB/web operations. User/Password will be stored " +
                               "encrypted on disk so it needs to be entered only once.");

            HasOption("username=", "The user's Aras login name", n => Username = n);
            HasOption("password=", "The user's Aras password", p => Password = p);
            // ReSharper disable once StringLiteralTypo
            HasOption("uselogininfo", "Don't ask for credentials if login info already exists",
                _ => UseLoginInfo = true);

            SkipsCommandSummaryBeforeRunning();
        }

        public override int Run(string[] remainingArguments)
        {
            // if we're just checking, and we can load, we're logged in and all is ok
            var loginInfo = LoginInfo.Load();
            if (UseLoginInfo && loginInfo != null)
            {
                Console.WriteLine($"Login info for user '{loginInfo.Username}' is present. Not updating.");
                return 0;
            }

            if (Username == null)
            {
                Console.Write("Please enter your Aras login name: ");
                Username = Console.ReadLine();
            }

            if (Password == null)
            {
                Console.Write("Please enter your Aras password: ");

                try
                {
                    Password = "";
                    ConsoleKeyInfo cki;
                    while ((cki = Console.ReadKey(true)).KeyChar != '\r')
                    {
                        if (cki.KeyChar == '\b')
                        {
                            if (Password.Length <= 0)
                                continue;

                            Console.Write("\b \b");
                            Password = Password.Substring(0, Math.Max(0, Password.Length - 1));
                        }
                        else
                        {
                            Password += cki.KeyChar;
                            Console.Write('*');
                        }
                    }
                }
                catch (InvalidOperationException e)
                {
                    Console.Error.WriteLine("\n\nFailed to read keys: " + e.Message);
                }

                Console.WriteLine("\n");
            }

            if (string.IsNullOrWhiteSpace(Username) || string.IsNullOrWhiteSpace(Password))
                throw new UserMessageException("Not enough data. Login info not updated.");

            loginInfo = new LoginInfo {Username = Username, Password = Password};
            loginInfo.Store();

            Console.WriteLine($"Login info with user '{loginInfo.Username}' updated successfully.");
            return 0;
        }
    }
}
