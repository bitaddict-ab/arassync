// MIT License, see COPYING.TXT
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Xml;
using Aras.IOM;
using Aras.Common.Compression;
using JetBrains.Annotations;

namespace BitAddict.Aras
{
    /// <summary>
    /// Extensions to ease development of Aras Innovator methods.
    ///
    /// These methods can be used from classes that do not inherit ArasMethod.
    /// </summary>
    public static class ArasExtensions
    {
        /// <summary>
        /// Master Log root folder. Per database and method logs are created in/beneath this.
        /// </summary>
        public static string LogRootFolder = @"C:\ArasLogs\";

        private static readonly ThreadLocal<Logger> ThreadLogger = new ThreadLocal<Logger>();
        private static readonly ThreadLocal<Innovator> ThreadInnovator = new ThreadLocal<Innovator>();
        private static string _dbErrorLogFile;

        /// <summary>
        /// Global log object. Vaid during method invocation.
        /// </summary>
        public static Logger Logger => ThreadLogger.Value;

        /// <summary>
        /// Global Innovator ojbect. Valid during method invocation.
        /// </summary>
        /// <exception cref="ArasException"></exception>
        public static Innovator Innovator
        {
            [NotNull]
            get
            {
                if (ThreadInnovator.Value == null)
                    throw new ArasException("Innovator instance not set for this thread currently.");

                return ThreadInnovator.Value;
            }
            internal set => ThreadInnovator.Value = value; // set from unit tests
            
        }

        /// <summary>
        /// Create and apply an ArasMethod via reflection on single line.
        /// Allows exceptions in constructor to be logged
        /// </summary>
        /// <typeparam name="TMethod">The ArasMethod type to run</typeparam>
        /// <param name="item">Item to apply, must be server method's 'this' object.</param>
        /// <returns></returns>
        public static Item CallMethod<TMethod>([NotNull]Item item) where TMethod : ArasMethod
        {
            // methodname not present in server events, use class name instead then
            var methodName = item.getAttribute("type") == "method" ? item.getAttribute("action") : typeof(TMethod).Name;

            return CallMethod(methodName, i =>
            {
                var method = (TMethod)Activator.CreateInstance(typeof(TMethod));
                var result = method.DoApply(i);
                // ReSharper disable once SuspiciousTypeConversion.Global
                (method as IDisposable)?.Dispose();
                return result;
            }, item);
        }

        /// <summary>
        /// Apply a generic callable on an item, while logging invocation and errors.
        /// </summary>
        /// <param name="methodName"></param>
        /// <param name="method"></param>
        /// <param name="item"></param>
        /// <returns></returns>
        [NotNull]
        public static Item CallMethod([NotNull] string methodName, [NotNull]Func<Item, Item> method,
            [NotNull]Item item)
        {
            var dbName = item.getInnovator().getConnection().GetDatabaseName();
            var logFolder = Path.Combine(LogRootFolder, $"{dbName} Method Logs");
            var methodLogFile = Path.Combine(logFolder, $"{methodName}.xml");
            _dbErrorLogFile = Path.Combine(LogRootFolder, $"{dbName} Error Log.xml");

            if (!Directory.Exists(logFolder))
                Directory.CreateDirectory(logFolder);

            if (!File.Exists(_dbErrorLogFile))
                File.WriteAllText(_dbErrorLogFile, "");

            return WithLogger(methodLogFile, item.getInnovator(),
                () => RunAndLogArasMethod(method, item, dbName, methodName));
        }

        private static Item RunAndLogArasMethod(Func<Item, Item> method, Item item,
            string dbName, string methodName)
        {
            var dateTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:sss.fff");
            var startLogMsg = $"\n<ArasMethod start='{dateTime}' " +
                              $"database='{dbName}'>\n" +
                              $"<MethodInput>\n{FormatXml(item.node, 2)}\n</MethodInput>\n";

            try
            {
                Log(startLogMsg);

                var result = LogTime(() => method(item), "MethodTime");
                var xml = FormatXml(result?.node ?? result?.dom.DocumentElement, 2);
                Log($"\n<MethodResult>\n{xml}\n</MethodResult>\n");

                return result;
            }
            catch (Exception e)
            {
                LogException(startLogMsg, e);
                File.AppendAllText(_dbErrorLogFile, "\r\n</ArasMethod>\r\n");
                return Innovator.newError(
                        $"Error in {methodName}':<br/>" +
                        $"{e.GetType()}: {e.Message}<br/>" +
                        "Further details may be available in the error log");
            }
            finally
            {
                Log("\r\n</ArasMethod>\r\n");
            }
        }

        /// <summary>
        /// Log exception to error log and method log
        /// </summary>
        /// <param name="msg"></param>
        /// <param name="exception"></param>
        /// <returns></returns>
        public static string LogException(string msg, Exception exception)
        {
            var errMsg = msg;
            var exceptions = new Queue<Exception>(new[] {exception});

            while (exceptions.Any())
            {
                var e = exceptions.Dequeue();
                errMsg += "\n<MethodException>\n" +
                          $"  {e.GetType()}: {e.Message}\n" +
                          $"  {e.Source}\n" +
                          "  <![CDATA[\n" +
                          $"{e.StackTrace}\n" +
                          "  ]]>\n" +
                          "</MethodException>\n";

                if (e is AggregateException ae)
                    foreach (var ie in ae.InnerExceptions)
                        exceptions.Enqueue(ie);
                else if (e.InnerException != null)
                    exceptions.Enqueue(e.InnerException);
            }

            Log(errMsg);

            if (_dbErrorLogFile != null)
            {
                File.AppendAllText(_dbErrorLogFile,
                    errMsg
                        .Replace("\n", "\r\n")
                        .Replace("\r\n\r\n", "\r\n")
                        .Replace("\r\r\n", "\r\n"));
            }

            return errMsg;
        }

        /// <summary>
        /// Creates and sets a Logger and Innovator object
        /// </summary>
        /// <param name="methodLogFile"></param>
        /// <param name="innovator"></param>
        /// <param name="action"></param>
        /// <returns></returns>
        internal static Item WithLogger(string methodLogFile, Innovator innovator,
            Func<Item> action)
        {
            // This happens when one of our Aras methods directly or indirectly calls another of our Aras methods.
            if (ThreadLogger.Value != null && ThreadInnovator.Value != null)
                return action();

            using (var logger = new Logger(methodLogFile))
            {
                ThreadLogger.Value = logger;
                ThreadInnovator.Value = innovator;

                try
                {
                    return action();
                }
                finally
                {
                    ThreadLogger.Value = null;
                    ThreadInnovator.Value = null;
                }
            }
        }


        /// <summary>
        /// Run and log AML query
        /// </summary>
        /// <param name="innovator"></param>
        /// <param name="amlQuery"></param>
        /// <returns></returns>
        /// <exception cref="ArasException"></exception>
        [NotNull]
        public static Item ApplyAML(this Innovator innovator, [NotNull] string amlQuery)
        {
            Log($"<ApplyAml>\n  {amlQuery}\n");

            var result = LogTime(() => innovator.applyAML(amlQuery), "QueryTime");
            var xml = FormatXml(result.node ?? result.dom.DocumentElement);

            Log($"  <Result>\n  {xml}\n  </Result>\n" +
                $"  <Count>{result.getItemCount()}</Count>\n" +
                "</ApplyAml>");

            if (result.isError())
                throw new ArasException(result) { Source = amlQuery };

            return result;
        }


        /// <summary>
        /// Run and log SQL query
        /// </summary>
        /// <param name="innovator"></param>
        /// <param name="sqlQuery"></param>
        /// <returns></returns>
        /// <exception cref="ArasException"></exception>
        [NotNull]
        public static Item ApplySQL(this Innovator innovator, [NotNull] string sqlQuery)
        {
            Log($"<ApplySQL>\n  <sql>\n    {sqlQuery}\n  </sql>\n");

            var result = LogTime(() => innovator.applySQL(sqlQuery), "QueryTime");
            var xml = FormatXml(result.node ?? result.dom.DocumentElement);

            Log($"  <Result>\n{xml}\n  </Result>\n" +
                $"  <Count>{result.getItemCount()}\n</Count>\n" +
                "</ApplySQL>\n");

            if (result.isError())
                throw new ArasException(result) { Source = sqlQuery };

            return result;
        }

        /// <summary>
        /// Run and log Item query
        /// </summary>
        /// <param name="_"></param>
        /// <param name="item"></param>
        /// <param name="logResult"></param>
        /// <returns></returns>
        /// <exception cref="ArasException"></exception>
        [NotNull]
        // ReSharper disable once UnusedParameter.Global
        public static Item ApplyItem(this Innovator _, [NotNull] Item item, bool logResult = true)
        {
            Log($"<ApplyItem logresult='{logResult}'>\n  <input>\n{FormatXml(item.node)}\n  </input>\n");

            var result = LogTime(item.apply, "QueryTime");

            if (logResult)
            {
                var xml = FormatXml(result.node ?? result.dom.DocumentElement);
                Log($"  <Result>\n{xml}\n  </Result>\n" +
                    $"  <Count>{result.getItemCount()}</Count>\n" +
                    "</ApplyItem>\n");
            }

            if (result.isError() && result.getErrorCode() != "0") // no rows is not an error
                throw new ArasException(result) { SourceItem = item, Source = FormatXml(item.node) };

            return result;
        }

        /// <summary>
        /// Run and log query.
        /// </summary>
        /// <param name="item"></param>
        /// <param name="logResult"></param>
        /// <returns></returns>
        public static Item Apply(this Item item, bool logResult = true)
        {
            if (item.getInnovator() is MockInnovator mock)
            {
                var action = item.getAction()?.ToLower();
                switch (action)
                {
                    case "update":
                        return mock.UpdateMockItem(item);
                }
            }
            return Innovator.ApplyItem(item, logResult);
        }

        /// <summary>
        /// Get the names of the item's properties.
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public static IEnumerable<string> GetProperties(this Item item)
        {
            return item.node.ChildNodes.OfType<XmlNode>().Select(n => n.Name);
        }

        /// <summary>
        /// Run and log Innovator.getItemByKeyedName() query
        /// </summary>
        /// <param name="innovator"></param>
        /// <param name="itemType"></param>
        /// <param name="keyedName"></param>
        /// <returns></returns>
        public static Item GetItemByKeyedName(this Innovator innovator, [NotNull] string itemType,
            [NotNull] string keyedName)
        {
            Log($"<GetItemByKeyedName>\n  <input>\n    type = '{itemType}', keyedName = '{keyedName}'\n  </input>\n");

            var result = innovator is MockInnovator mock
                ? mock.getItemByKeyedName(itemType, keyedName)
                : LogTime(() => innovator.getItemByKeyedName(itemType, keyedName), "QueryTime");

            var xml = FormatXml(result?.node ?? result?.dom?.DocumentElement);
            Log($"  <Result>\n{xml}\n  </Result>\n" +
                $"  <Count>{result?.getItemCount()}</Count>\n" +
                "</GetItemByKeyedName>\n");

            if (result?.node == null)
                throw new ArasException($"No item of type '{itemType}' with name '{keyedName}' found.");

            if (result.isError() || result.isEmpty())
                throw new ArasException(result) { Source = $"{itemType} keyed_name: '{keyedName}'" };

            return result;
        }

        /// <summary>
        /// Run and log Innovator.getItemById() query
        /// </summary>
        /// <param name="innovator">innovator ojb</param>
        /// <param name="itemType">item type</param>
        /// <param name="id">item id</param>
        /// <returns></returns>
        public static Item GetItemById(this Innovator innovator, [NotNull] string itemType, [NotNull] string id)
        {
            Log($"<GetItemByID>\n  <input>\n    type = '{itemType}', id = '{id}'\n  </input>\n");

            var result = innovator is MockInnovator mock
                ? mock.getItemById(itemType, id)
                : LogTime(() => innovator.getItemById(itemType, id), "QueryTime");

            var xml = FormatXml(result.node ?? result.dom.DocumentElement);
            Log($"  <Result>\n{xml}\n  </Result>\n" +
                $"  <Count>{result.getItemCount()}</Count>\n" +
                "</GetItemByID>\n");

            if (result.node == null)
                throw new ArasException($"No item of type '{itemType}' with id '{id}' found.");

            if (result.isError() && result.isEmpty())
                throw new ArasException(result) { Source = $"{itemType} id: '{id}'" };

            return result;
        }

        /// <summary>
        /// Run and log Innovator.newItem() query.
        /// </summary>
        /// <param name="innovator">Innovator object.</param>
        /// <param name="itemTypeName">Item type name.</param>
        /// <param name="action">Action.</param>
        /// <returns></returns>
        public static Item NewItem(this Innovator innovator, string itemTypeName, string action)
        {
            Log($"<{nameof(NewItem)}>\n  <input>\n    type = '{itemTypeName}', action = '{action}'\n  </input>\n");

            var result = innovator is MockInnovator mock
                ? LogTime(() =>
                {
                    var item = mock.newItem(itemTypeName, action);
                    item.GetType().GetField("parentInnovator", BindingFlags.NonPublic | BindingFlags.Instance)?.SetValue(item, innovator);
                    return item;
                }, "QueryTime")
                : LogTime(() => innovator.newItem(itemTypeName, action), "QueryTime");

            var xml = FormatXml(result.node ?? result.dom.DocumentElement);
            Log($"  <Result>\n{xml}\n  </Result>\n" +
                $"  <Count>{result.getItemCount()}</Count>\n" +
                $"</{nameof(NewItem)}>\n");

            if (result.isError() && result.isEmpty())
                throw new ArasException(result) { Source = $"{itemTypeName} action: '{action}'" };

            return result;
        }

        /// <summary>
        /// Run and log Aras method
        /// </summary>
        /// <param name="innovator"></param>
        /// <param name="method"></param>
        /// <param name="body"></param>
        /// <returns></returns>
        /// <exception cref="ArasException"></exception>
        [NotNull]
        public static Item ApplyMethod(this Innovator innovator, [NotNull] string method, [NotNull] string body)
        {
            Log($"<ApplyMethod>\n  <input>\n    {body}\n  </input>\n");

            var result = LogTime(() => innovator.applyMethod(method, body), "MethodTime");

            var xml = FormatXml(result.node ?? result.dom.DocumentElement);
            Log($"  <Result>\n{xml}\n  </Result>\n" +
                $"  <Count>{result.getItemCount()}</Count>\n" +
                "</ApplyMethod>\n");

            if (result.isError())
                throw new ArasException(result) { Source = $"{method}\n{body}" };

            return result;
        }

        /// <summary>
        /// Run and log fetchRelationships query
        /// </summary>
        /// <param name="_"></param>
        /// <param name="item"></param>
        /// <param name="relationShipTypeName"></param>
        /// <param name="selectList"></param>
        /// <returns></returns>
        /// <exception cref="ArasException"></exception>
        [NotNull]
        // ReSharper disable once UnusedParameter.Global
        public static Item FetchRelationships(this Innovator _, [NotNull] Item item, string relationShipTypeName, string selectList = "related_id(*)")
        {
            Log($"<FetchRelationships type='{relationShipTypeName}' " +
                $"selectList='{selectList}'>\n" +
                $"  <input>\n    {FormatXml(item.node)}\n  </input>\n");

            var result = LogTime(() => item.fetchRelationships(
                relationShipTypeName, selectList), "QueryTime");

            var xml = FormatXml(result.node ?? result.dom.DocumentElement);
            Log($"  <Result>\n{xml}\n  </Result>\n" +
                $"  <Count>{result.getItemCount()}</Count>\n" +
                "</FetchRelationships>\n");

            if (result.isError())
                throw new ArasException(result)
                {
                    SourceItem = item,
                    Source = $"{relationShipTypeName} {selectList}"
                };

            return result;
        }

        /// <summary>
        /// Measure and log the time for a function that returns a value
        /// </summary>
        /// <param name="func"></param>
        /// <param name="tag"></param>
        /// <typeparam name="TResult"></typeparam>
        /// <returns></returns>
        public static TResult LogTime<TResult>(Func<TResult> func, string tag)
        {
            var sw = new Stopwatch();
            sw.Start();

            try
            {
                return func();
            }
            finally
            {
                Log(tag, $"{sw.ElapsedMilliseconds} ms");
            }
        }

        /// <summary>
        /// Measure and log time for a given action
        /// </summary>
        /// <param name="action"></param>
        /// <param name="tag"></param>
        public static void LogTime(Action action, string tag)
        {
            LogTime(() =>
            {
                action();
                return true;
            }, tag);
        }

        /// <summary>
        /// Catch and log exception if it occurs.
        /// </summary>
        /// <param name="action"></param>
        /// <returns>true if ok, false on logged exception</returns>
        public static bool LogIfException(Action action)
        {
            try
            {
                action();
                return true;
            }
            catch (Exception e)
            {
                LogException(action.Method.Name, e);
                return false;
            }
        }

        /// <summary>
        /// Convert multi-result Item to IEnumerable&lt;Item&gt;
        /// </summary>
        /// <param name="items"></param>
        /// <returns></returns>
        /// <exception cref="ArasException"></exception>
        public static IEnumerable<Item> Enumerate(this Item items)
        {
            if (items.isError() && items.getErrorCode() != "0") // no rows is not error
                throw new ArasException(items);

            var nItems = items.getItemCount();
            for (var i = 0; i < nItems; ++i)
                yield return items.getItemByIndex(i);
        }
        /// <summary>
        /// Write message to log
        /// </summary>
        /// <param name="tag"></param>
        /// <param name="message"></param>
        public static void Log(string tag, string message)
        {
            var timeStr = DateTime.Now.ToString("HH:mm:ss.fff");

            var shortMsg = message.Length < 40;
            var newline = shortMsg ? "" : "\n";
            var endl = (shortMsg || message.EndsWith("\n")) ? "" : newline;

            if (shortMsg && message.EndsWith("\n"))
                message = message.Substring(0, message.Length - 1);

            Log($"<{tag} time='{timeStr}'>{newline}{message}{endl}</{tag}>\n");
        }

        internal static void Log(string message)
        {
            if (Logger != null)
                Logger.Log(message);
            else
                Console.WriteLine(message); // shows in unit tests
        }

        /// <summary>
        /// Generate indented XML string
        /// </summary>
        /// <param name="node">node to print</param>
        /// <param name="indent">default indent</param>
        /// <returns></returns>
        public static string FormatXml(this XmlNode node, int indent = 4)
        {
            if (node == null)
                return "(null)";

            var indentStr = string.Concat(Enumerable.Repeat(' ', indent));

            using (var sw = new StringWriter())
            using (var xw = XmlWriter.Create(sw, new XmlWriterSettings()
            {
                Indent = true,
                IndentChars = "  ",
                CloseOutput = true,
                NewLineHandling = NewLineHandling.None,
                NewLineChars = '\n' + indentStr,
                WriteEndDocumentOnClose = false,
                ConformanceLevel = ConformanceLevel.Fragment
            }))
            {
                sw.Write(indentStr);
                node.WriteTo(xw);
                xw.Close();
                return sw.ToString();
            }
        }

        /// <summary>
        /// Create a new connection and return the innovator.
        /// </summary>
        /// <param name="url"></param>
        /// <param name="databaseName"></param>
        /// <param name="username"></param>
        /// <param name="password"></param>
        public static Innovator GetNewInnovator([NotNull] string url, [NotNull] string databaseName,
            [NotNull] string username, [NotNull] string password)
        {
            var connection = IomFactory.CreateHttpServerConnection(
                url, databaseName, username, password);
            connection.Timeout = 15000;
            connection.Compression = CompressionType.deflate;
            var loginItem = connection.Login();
            if (loginItem.isError())
                throw new ArasException("Aras login failed: " + loginItem.getErrorString());
            return loginItem.getInnovator();
        }
    }
}
