// MIT License, see COPYING.TXT
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using JetBrains.Annotations;

namespace BitAddict.Aras
{
    /// <inheritdoc />
    /// <summary>
    /// Helps with logging by creating separate log files if running concurrently
    /// </summary>
    public class Logger : IDisposable
    {
        private class ConcurrentLogFiles
        {
            public readonly List<Tuple<string, int>> Files = new List<Tuple<string, int>>();
        }

        private static readonly Dictionary<string, ConcurrentLogFiles> ActiveLogfiles =
            new Dictionary<string, ConcurrentLogFiles>();

        /// <summary>
        /// Path to the log file
        /// </summary>
        public string LogFile { get; }
        /// <summary>
        /// If log messages shoulkd be written to the console in addition to file
        /// </summary>
        public static bool EnableConsoleLogging { get; set; }

        /// <summary>
        /// If log messages are output to attached debugger
        /// </summary>
        public static bool EnableDebugLogging => EnableConsoleLogging && Debugger.IsAttached;

        /// <summary>
        /// If log file number should be incremented for each new logger created.
        /// Used in unit-testing only.
        /// </summary>
        public static bool AlwaysIncrementLogNumber { get; set; } = false;

        private readonly FileStream _stream;
        private readonly StreamWriter _writer;
        private readonly string _baseName;

        private bool _writeError;
        private bool _disposed;

        /// <summary>
        /// Create logger instance.
        /// </summary>
        /// <param name="baseName"></param>
        public Logger(string baseName)
        {
            _baseName = baseName;

            LogFile = AllocLogFile(_baseName);

            try
            {
                const int bufferSize = 16*1024;
                _stream = new FileStream(LogFile, FileMode.Create, FileAccess.Write, FileShare.Read, bufferSize);
                _writer = new StreamWriter(_stream, Encoding.UTF8);
            }
            catch (Exception)
            {
                Dispose();
                throw;
            }
        }

        /// <summary>
        /// Log a message to log file et.al. Safe to use concurrently.
        /// </summary>
        /// <param name="msg"></param>
        /// <exception cref="ObjectDisposedException"></exception>
        public void Log(string msg)
        {
            if (EnableConsoleLogging)
                Console.WriteLine(msg);

            if (EnableDebugLogging)
                Debug.WriteLine(msg);

            if (_writeError)
                return;

            if (_disposed)
                throw new ObjectDisposedException("The Logger has been disposed.");

            lock (this)
            {
                DoLog(msg);
            }
        }

        private void DoLog(string msg)
        {
            if (_writeError || _disposed)
            {
                _writeError = true;
                return;
            }

            try
            {
                _writer.Write(msg);

                if (!msg.EndsWith("\n"))
                    _writer.Write('\n');
            }
            catch (Exception e)
            {
                ArasExtensions.LogException("Logger", e);
                _writeError = true;
            }
        }

        /// <inheritdoc />
        /// <summary>
        /// Mark object as disposed, will perform Dispose once writes are completed
        /// </summary>
        public void Dispose()
        {
            if (_disposed)
                return;

            _disposed = true;

            _writer?.Close();
            _writer?.Dispose();
            _stream?.Close();
            _stream?.Dispose();

            if (!AlwaysIncrementLogNumber)
                RetireLogFile(_baseName, LogFile);
        }

        private static string AllocLogFile([NotNull] string baseName)
        {
            if (baseName == null)
                throw new ArgumentNullException(nameof(baseName));

            lock (ActiveLogfiles)
            {
                if (!ActiveLogfiles.TryGetValue(baseName, out var clf))
                {
                    clf = new ConcurrentLogFiles();
                    ActiveLogfiles.Add(baseName, clf);
                }

                // get first free count
                var count = 0;
                for (; count <= clf.Files.Count; ++count)
                {
                    if (!clf.Files.Exists(t => t.Item2 == count))
                        break;
                }


                var baseDir = Path.GetDirectoryName(baseName);
                if (string.IsNullOrEmpty(baseDir))
                    throw new ArgumentException("Path has no directory.", nameof(baseName));

                var newName =
                    Path.Combine(baseDir,
                        $"{Path.GetFileNameWithoutExtension(baseName)}-{count}" +
                        $"{Path.GetExtension(baseName)}");

                clf.Files.Add(Tuple.Create(newName, count));

                if (EnableConsoleLogging || AlwaysIncrementLogNumber)  
                    Console.WriteLine($"Logfile created at {newName}");

                return newName;
            }
        }

        private static void RetireLogFile(string baseName, string name)
        {
            if (baseName == null)
                throw new ArgumentNullException(nameof(baseName));
            if (name == null)
                throw new ArgumentNullException(nameof(name));

            lock (ActiveLogfiles)
            {
                ConcurrentLogFiles clf;
                if (!ActiveLogfiles.TryGetValue(baseName, out clf))
                    return;

                var idx = clf.Files.FindIndex(f => f.Item1 == name);
                clf.Files.RemoveAt(idx);

                if (clf.Files.Count == 0)
                    ActiveLogfiles.Remove(baseName);
            }
        }

    }
}
