using Aras.IOM;

namespace BitAddict.Aras
{
    /// <summary>
    /// Simplify usage of ArasExtensions in the top class for an Aras method
    /// </summary>
    public abstract class ArasMethod
    {
        /// <summary>
        /// Setup logging, innovator and error handling, then run method.
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public Item Apply(Item item)
        {
            return ArasExtensions.CallMethod(GetType().Name, DoApply, item);
        }

        /// <summary>
        /// Main method code. Everything is setup prior to this being called.
        /// Excpetions from this method are caught, logged and returned as error item.
        /// </summary>
        /// <param name="root">Item the Aras method was called with</param>
        /// <returns>Method result item</returns>
        public abstract Item DoApply(Item root);

        /// <summary>
        /// Shared Aras Innovator instance
        /// </summary>
        public Innovator Innovator
        {
            get => _innovator ?? (_innovator = ArasExtensions.Innovator);
            set => _innovator = value;
        }

        /// <summary>
        /// Shared logger instance
        /// </summary>
        public Logger Logger => _logger ?? (_logger = ArasExtensions.Logger);

        private Innovator _innovator;
        private Logger _logger;

        /// <summary>
        /// Run AML query, log query and result
        /// </summary>
        /// <param name="aml"></param>
        /// <returns></returns>
        // ReSharper disable once InconsistentNaming
        public Item ApplyAML(string aml)
        {
            return Innovator.ApplyAML(aml);
        }

        /// <summary>
        /// Run SQL query, log query and result
        /// </summary>
        /// <param name="sql"></param>
        /// <returns></returns>
        // ReSharper disable once InconsistentNaming
        public Item ApplySQL(string sql)
        {
            return Innovator.ApplySQL(sql);
        }

        /// <summary>
        /// Run Item query, log query and result
        /// </summary>
        /// <param name="item"></param>
        /// <param name="logResult">Log result (default true). Disable for speedups.</param>
        /// <returns></returns>
        public Item ApplyItem(Item item, bool logResult = true)
        {
            return Innovator.ApplyItem(item, logResult);
        }

        /// <summary>
        /// Fetch relationships, log query and result
        /// </summary>
        /// <param name="item"></param>
        /// <param name="relationShipTypeName"></param>
        /// <param name="selectList">What to select from relationshiptype</param>
        /// <returns></returns>
        public Item FetchRelationships(Item item, string relationShipTypeName, string selectList)
        {
            return Innovator.FetchRelationships(item, relationShipTypeName, selectList);
        }

        /// <summary>
        /// Log a message to log file
        /// </summary>
        /// <param name="tag">Where message occurs</param>
        /// <param name="message">Actual log message</param>
        public void Log(string tag, string message)
        {
            ArasExtensions.Log(tag, message);
        }
    }

}