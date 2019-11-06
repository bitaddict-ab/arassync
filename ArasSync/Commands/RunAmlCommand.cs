// MIT License, see COPYING.TXT
using System;
using System.IO;
using Aras.Common.Compression;
using Aras.IOM;
using BitAddict.Aras.ArasSync.Ops;
using BitAddict.Aras.Security;
using JetBrains.Annotations;
using ManyConsole;

namespace BitAddict.Aras.ArasSync.Commands
{
    /// <summary>
    /// Runs an AML query on an Aras database
    /// </summary>
    [UsedImplicitly]
    public class RunAmlCommand : ConsoleCommand
    {
        public string AmlFilePath { get; set; }
        public string Database { get; set; }

        public RunAmlCommand()
        {
            IsCommand("RunAML", "Run an AML query on the database");

            HasRequiredOption("aml=|amlfile=", "File that contains the AML query", f => AmlFilePath = f);
            HasRequiredOption("db=|database=", "The Aras instance id to run the query in", db => Database = db);
        }

        public override int Run(string[] remainingArguments)
        {
            var amlQuery = File.ReadAllText(AmlFilePath);
            Console.WriteLine("\nRead the following query:\n");
            Console.WriteLine(amlQuery);
            Console.WriteLine("\n");
            Common.RequestUserConfirmation($"run the query above on {Database}");

            var loginInfo = LoginInfo.Load();
            if (loginInfo == null)
                throw new ArasException("No user logged in.");

            var arasDb = Config.FindDb(Database);
            var connection = IomFactory.CreateHttpServerConnection(
                arasDb.Url, arasDb.DbName,
                loginInfo.Username, loginInfo.Password);

            connection.Timeout = 2 * 60 * 1000;
            connection.Compression = CompressionType.deflate;

            var loginItem = connection.Login();

            if (loginItem.isError())
                throw new ArasException("Aras login failed: " + loginItem.getErrorString());

            var innovator = loginItem.getInnovator();

            var response = innovator.ApplyAML(amlQuery);

            Console.WriteLine("\nGot the following response:\n");
            Console.WriteLine(response);

            return 0;
        }
    }
}

