// MIT License, see COPYING.TXT
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using Aras.IOM;
using BitAddict.Aras.ArasSync.Data;
using BitAddict.Aras.Security;
using Newtonsoft.Json;

namespace BitAddict.Aras.ArasSync.Ops
{
    internal static class Common
    {
        /// <summary>
        /// runs a process and captures stdout into string, ignores stderr
        /// </summary>
        /// <param name="exeName">Name of executable</param>
        /// <param name="silent">Pipe output in console or not</param>
        /// <param name="output">If non-null, process's output is collected and written here</param>
        /// <param name="args">args to the process (automatically wrapped in "")</param>
        /// <returns></returns>
        internal static int RunProcess(string exeName, bool silent, ref string output, params string[] args)
        {
            var sb = output != null ? new StringBuilder() : null;
            var argsString = string.Join(" ", args.Select(s => $"\"{s}\""));

            if (Environment.GetEnvironmentVariable("ARASSYNC_DEBUG") != null)
            {
                Console.WriteLine("exe:  " + exeName);
                Console.WriteLine("args: " + argsString);
            }

            var process = new Process
            {
                StartInfo = new ProcessStartInfo(exeName)
                {
                    Arguments = argsString,
                    UseShellExecute = false,
                    RedirectStandardOutput = true
                }
            };

            process.OutputDataReceived += (sender, a) =>
            {
                if (!silent)
                    Console.WriteLine(a.Data);
                sb?.Append(a.Data);
            };

            process.ErrorDataReceived += (sender, a) =>
            {
                if (!silent)
                    Console.WriteLine(a.Data);
            };

            try
            {
                process.Start();
                process.BeginOutputReadLine();
                process.WaitForExit();
            }
            catch (Exception e)
            {
                Console.Error.WriteLine(
                    $"\n{e.GetType()} thrown running '{exeName}'\n" +
                    $"  in '{Environment.CurrentDirectory}'\n" +
                    $"  with args:\n  {string.Join(", ", args)}\n");
                throw;
            }

            output = sb?.ToString();

            return process.ExitCode;
        }

        internal static void RequireArasFeatureManifest(string file = "amlsync.json")
        {
            if (!File.Exists(file))
                throw new UserMessageException($"No {file} in current directory. Not a feature directory?");
        }

        internal static LoginInfo RequireLoginInfo()
        {
            var loginInfo = LoginInfo.Load();
            if (loginInfo == null)
                throw new UserMessageException("Please login before trying to access an Aras instance.");
            return loginInfo;
        }

        internal static IEnumerable<string> GetArasImportManifestFiles(string exportDir, string singleFile = null)
        {
            var manifestFiles = new List<string>();

            if (singleFile != null)
                manifestFiles.Add(Path.Combine(exportDir, singleFile));
            else
                manifestFiles.AddRange(Directory.GetFiles(exportDir, "*.mf"));

            return manifestFiles;
        }

        internal static void RequestUserConfirmation(string opText)
        {
            Console.Write($"\nType YES to {opText}: ");

            if (Console.ReadLine() != "YES")
                throw new UserMessageException("");

            Console.WriteLine();
        }

        internal static string GetFeatureName()
        {
            return Path.GetFileName(Environment.CurrentDirectory);
        }

        internal static ArasFeatureManifest ParseArasFeatureManifest(string amlSyncFile)
        {
            RequireArasFeatureManifest();

            if (!Path.IsPathRooted(amlSyncFile))
                amlSyncFile = Path.Combine(Environment.CurrentDirectory, amlSyncFile);

            var dir = Path.GetDirectoryName(amlSyncFile);
            var json = File.ReadAllText(amlSyncFile);

            // handle legacy format where we only had AmlFragments directly
            // (note to self: don't restrict config files that way again..)

            ArasFeatureManifest data;
            try
            {
                data = JsonConvert.DeserializeObject<ArasFeatureManifest>(json);
            }
            catch (Exception e)
            {
                try
                {
                    var fragments = JsonConvert.DeserializeObject<List<AmlFragment>>(json);
                    data = new ArasFeatureManifest {AmlFragments = fragments};

                    Console.WriteLine($"Read {amlSyncFile} using legacy format (raw amlfragment list)...");
                }
                catch (Exception e2)
                {
                    Console.WriteLine($"Failed to parse {amlSyncFile}: {e.Message}\n  Legacy failed too: {e2.Message}");
                    throw e;
                }
            }

            data.LocalDirectory = dir;

            return data;
        }

        private static readonly string BackSpaces = string.Join("", Enumerable.Repeat('\b', 120));

        internal static void CopyFileWithProgress(string src, string dst)
        {
            var srcFile = Path.GetFileName(src);
            Console.Write($"  {srcFile}        ");

            var webClient = new WebClient();
            var done = false;

            webClient.DownloadProgressChanged += (s, a) =>
            {
                lock (BackSpaces)
                {
                    var eol = (a.ProgressPercentage == 100 && !done) ? "\r\n" : "";

                    if (!done)
                        Console.Write($"{BackSpaces}  {srcFile} {a.ProgressPercentage}%   {eol}");

                    if (a.ProgressPercentage == 100 && !done)
                        done = true;
                }
            };

            webClient.DownloadFileTaskAsync(new Uri(src), dst)
                .Wait();

            // resolve race condition, as we're done first, then get last progress changed
            while(!done)
                Thread.Sleep(15);
        }


        internal static Innovator GetNewInnovator(string database)
        {
            var loginInfo = LoginInfo.Load();
            if (loginInfo == null)
                throw new Exception("No user logged in.");

            var arasDb = Config.FindDb(database);

            return ArasExtensions.GetNewInnovator(
                arasDb.Url, arasDb.DbName, loginInfo.Username, loginInfo.Password);
        }

        /// <summary>
        /// Check if file exists on PATH 
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        public static bool ExistsOnPath(string fileName)
        {
            return GetFullPath(fileName) != null;
        }

        private static string GetFullPath(string fileName)
        {
            if (File.Exists(fileName))
                return Path.GetFullPath(fileName);

            var values = Environment.GetEnvironmentVariable("PATH") ?? "";

            return values.Split(';')
                .Select(path => Path.Combine(path, fileName))
                .FirstOrDefault(File.Exists);
        }
    }
}
