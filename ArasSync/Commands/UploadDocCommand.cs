using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Security.AccessControl;
using System.Threading;
using BitAddict.Aras.ArasSyncTool.Ops;
using BitAddict.Aras.Data;
using ManyConsole;

namespace BitAddict.Aras.ArasSyncTool.Commands
{
    [CommandCategory("Advanced")]
    class UploadDocCommand : ConsoleCommand
    {
        public bool Confirm { get; set; } = true;
        public bool Build { get; set; } = true;
        public string Database { get; set; }
        public string Dir { get; set; }
        public string BuildConfig { get; set; } = "Release";

        public UploadDocCommand()
        {
            IsCommand("UploadDoc", "Generates and uploads documentation from C#-sources for the current feature to the Aras Web Server bin/htmldoc/<feature> folder");

            HasOption("dir=|directory=", "Directory to copy to", dir => Dir = dir);
            HasOption("db=|database=", "Database Id to copy to", db => Database = db);

            HasOption("noconfirm", "Disable user confirmation", _ => Confirm = false);
            HasOption("nobuild", "Disable building", _ => Build = false);

            HasOption("cfg=|msbuildconfiguration=", "MSBuild configuration to use (default: Release)", cfg => BuildConfig = cfg);
        }

        public override int Run(string[] remainingArguments)
        {
            if (Database == null && Dir == null || (Database != null && Dir != null))
                throw new UserMessageException("You must specify one and only one of --db or --dir");

            Common.RequireArasFeatureManifest();

            if (!Directory.EnumerateFiles(Environment.CurrentDirectory, "*.csproj").Any())
            {
                Console.WriteLine("No .csproj here. Nothing to do.");
                return 0;
            }

            var featureName = Common.GetFeatureName();

            if (Build)
                BuildFeature(featureName);

            var targetFolder = GetTargetFolder(featureName);

            if (Confirm)
                // ReSharper disable once PossibleNullReferenceException
                Common.RequestUserConfirmation($"create documentation for '{featureName}' on {targetFolder}'");
            else
                Console.WriteLine($"Create documentation in {targetFolder}");

            CreateTargetFolder(targetFolder);

            if (Database != null)
                UploadWebConfig(targetFolder);

            // Generate documents directly into target folder
            var sourceFolder = Path.Combine(Environment.CurrentDirectory, "bin", BuildConfig);
            return RunDocu(sourceFolder, targetFolder);
        }

        private static void CreateTargetFolder(string targetFolder)
        {
            if (!Directory.Exists(targetFolder))
            {
                Console.WriteLine($"\nCreating {targetFolder} ...");
                Directory.CreateDirectory(targetFolder);
            }
        }

        private static int RunDocu(string sourceFolder, string targetFolder)
        {
            Console.WriteLine($"\nGenerating documentation ...");

            var docuExe = Path.Combine(Config.SolutionDir.FullName, "Core", "docu", "docu.exe");
            string outputDummy = null;
            return Common.RunProcess(docuExe, false, ref outputDummy,
                Path.Combine(sourceFolder, "*.Aras*.dll"),
                $"--output={targetFolder}");
        }

        private string GetTargetFolder(string featureName)
        {
            var targetFolder = Dir;

            if (Database != null)
            {
                Common.RequireLoginInfo(); // enforce logged in. Not same permissions as filecopy though, but something.
                var arasDb = Config.FindDb(Database);
                targetFolder = Path.Combine(arasDb.BinFolder, "Innovator", "server", "bin", "htmldoc", featureName);
            }
            else if (Dir != null)
            {
                targetFolder = Path.IsPathRooted(Dir) ? Dir : Path.Combine(Environment.CurrentDirectory, Dir);
                Confirm = false;
            }

            if (targetFolder == null)
                throw new ArgumentNullException(nameof(targetFolder), "internal error");

            return targetFolder;
        }

        private void BuildFeature(string featureName)
        {
            Console.WriteLine($"Building {featureName} in {BuildConfig} mode...\n");

            string nullStr = null;
            if (Common.RunProcess("msbuild", false, ref nullStr,
                $"/p:Configuration={BuildConfig}",
                "/verbosity:minimal",
                "/nologo") != 0)
            {
                throw new UserMessageException("Build failed");
            }
        }

        private static void UploadWebConfig(string targetFolder)
        {
            const string file = "web.config";
            Console.WriteLine($"\nUploading {file} to unlock IIS ...");

            var targetFile = Path.Combine(targetFolder, $@"..\{file}");
            
            // allow overwrite
            try
            {
                File.SetAttributes(targetFile, FileAttributes.Normal);
            }
            catch (FileNotFoundException)
            {
                // The file hasn't been uploaded yet and does therefore not exist
            }

            File.Copy(Path.GetFullPath($@"{Assembly.GetEntryAssembly().Location}\..\..\..\Files\{file}"), targetFile, true);

            // prevent IIS dir browse from showing
            File.SetAttributes(targetFile, FileAttributes.Hidden);
        }
    }
}
