using System.Collections.Generic;

namespace BitAddict.Aras.Data
{
    /// <summary>
    /// Defines the data required to sync with Aras databases
    ///
    /// Represents an entire arasdb(-local).json file.
    /// </summary>
    public class ArasConfManifest
    {
        /// <summary>
        /// Aras Instance used for development/unit testing
        /// </summary>
        public string DevelopmentInstance { get; set; }
        /// <summary>
        /// A list of Aras DB instances
        /// </summary>
        public List<ArasDb> Instances { get; set; }
        /// <summary>
        /// File copying information
        /// </summary>
        public CopyDllInfo CopyDll { get; set; }
    }

    /// <summary>
    /// Defines an Aras database instance
    /// </summary>
    public class ArasDb
    {
        /// <summary>
        /// Instance id. Used when invoking arassync only. Not relevant for Aras.
        /// </summary>
        public string Id { get; set; }
        /// <summary>
        /// HTTP URL to web server
        /// </summary>
        public string Url { get; set; }
        /// <summary>
        /// Database name as in web login dialgo
        /// </summary>
        public string DbName { get; set; }
        /// <summary>
        /// Web server 'binaries', i.e. c:\program files (x86)\Aras\TheInstance\
        /// </summary>
        public string BinFolder { get; set; }
    }

    /// <summary>
    /// Configures copyDLL command with extensions and exclusions
    /// </summary>
    public class CopyDllInfo
    {
        /// <summary>
        /// Copies these extensions only
        /// </summary>
        public List<string> Extensions { get; set; } = new List<string>
        {
            ".dll", ".pdb"
        };
        /// <summary>
        /// Does not copy files with these fragments in the name
        /// </summary>
        public List<string> Excludes { get; set; } = new List<string>
        {
            "IOM", "InnovatorCore", "Test", "UnitTestFramework"
        };
    }
}
