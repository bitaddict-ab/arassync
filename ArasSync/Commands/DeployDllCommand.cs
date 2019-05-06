// MIT License, see COPYING.TXT
using System;
using System.IO;
using System.Linq;
using BitAddict.Aras.ArasSync.Ops;
using BitAddict.Aras.Data;
using JetBrains.Annotations;
using ManyConsole;

namespace BitAddict.Aras.ArasSync.Commands
{
    /// <summary>
    /// Build and copies DLLs
    /// </summary>
    [UsedImplicitly]
    public class DeployDllCommand : ConsoleCommand
    {
        public bool Confirm { get; set; } = true;
        public bool Build { get; set; } = true;
        public string Database { get; set; }
        public string Dir { get; set; }
        public string BuildConfig { get; set; } = "Release";
        public bool Doc { get; set; } = true;

        public DeployDllCommand()
        {
            IsCommand("DeployDLL", "Builds and copies DLLs & docs for the current feature to the Aras Web Server bin folder");

            HasOption("dir=|directory=", "Directory to copy to", dir => Dir = dir);
            HasOption("db=|database=", "Database Id to copy to", db => Database = db);

            HasOption("noconfirm", "Disable user confirmation", _ => Confirm = false);
            HasOption("nobuild", "Disable building feature", _ => Build = false);
            HasOption("nodoc", "Disable uploading documentation", _ => Doc = false);

            HasOption("cfg=|msbuildconfiguration=", "MSBuild configuration to use (default: Release)", cfg => BuildConfig = cfg);
        }

        public override int Run(string[] remainingArguments)
        {
            if (Database == null && Dir == null || (Database != null && Dir != null))
                throw new UserMessageException("You must specify one and only one of --db or --dir");

            Common.RequireArasFeatureManifest();
            var featureName = Common.GetFeatureName();

            var csproj = Directory.EnumerateFiles(Environment.CurrentDirectory, "*.csproj")
                .SingleOrDefault();

            if (csproj == null)
            {
                Console.WriteLine("No (or multiple) .csproj found. Cannot continue.");
                return 0;
            }

            if (Build)
            {
                Console.WriteLine($"Building {featureName} in {BuildConfig}...\n");


                string nullStr = null;
                if (Common.RunProcess(Common.MSBuildCmd, false, ref nullStr,
                    csproj,
                    $"/p:Configuration={BuildConfig}",
                    "/verbosity:minimal",
                    "/filelogger",
                    "/nologo") != 0)
                {
                    throw new UserMessageException("Build failed");
                }
            }

            var sourceFolder = Path.Combine(Environment.CurrentDirectory, "bin", BuildConfig);
            var targetFolder = Dir;
            ArasDb arasDb = null;

            if (Database != null)
            {
                Common.RequireLoginInfo(); // enforce logged in. Not same permissions as filecopy though, but something.
                arasDb = Config.FindDb(Database);
                targetFolder = Path.Combine(arasDb.BinFolder, "Innovator", "server", "bin");
            }
            else if (Dir != null)
            {
                targetFolder = Path.IsPathRooted(Dir) ? Dir : Path.Combine(Environment.CurrentDirectory, Dir);
                Confirm = false;
            }

            if (targetFolder == null)
                throw new ArgumentNullException(nameof(targetFolder), "internal error");

            var info = Config.GetDeployDllInfo();

            Console.WriteLine($"About to copy [{string.Join(",", info.Extensions.Distinct().Select(e => $"*{e}"))}]\n" +
                              $"  from: {sourceFolder}\n" +
                              $"  to:   {targetFolder}");

            if (Dir != null && !Directory.Exists(targetFolder))
            {
                Console.WriteLine($"\nCreating {targetFolder} ...");
                Directory.CreateDirectory(targetFolder);
            }

            if (Confirm)
                // ReSharper disable once PossibleNullReferenceException
                Common.RequestUserConfirmation($"copy {BuildConfig} DLLs for '{featureName}' to Aras server '{arasDb.Id}'");
            else
                Console.WriteLine();

            // Copy build files
            var count = 0;

            foreach (var filename in Directory.EnumerateFiles(sourceFolder)
                .Select(Path.GetFileName)
                .Where(file => info.Extensions.Contains(Path.GetExtension(file)?.ToLowerInvariant()))
                .Where(file => !info.Excludes.Any(file.Contains)))
            {
                Common.CopyFileWithProgress(
                    Path.Combine(sourceFolder, filename),
                    Path.Combine(targetFolder, filename));
                count++;
            }

            Console.WriteLine($"\n{count} file(s) copied.");

            // Upload docs too

            if (!Doc)
                return 0;

            var uploadDoc = new UploadDocCommand
            {
                Database = Database,
                Dir = Dir,
                Confirm = false,
                Build = false, // don't build twice
                BuildConfig = BuildConfig,
            };

            Console.WriteLine();
            return uploadDoc.Run(new string[] {});
        }
    }
}
