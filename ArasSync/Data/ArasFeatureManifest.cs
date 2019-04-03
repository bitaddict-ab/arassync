using JetBrains.Annotations;
using System.Collections.Generic;

namespace BitAddict.Aras.ArasSyncTool.Data
{
    [UsedImplicitly]
    public class AmlNode
    {
        [UsedImplicitly] public string File { get; set; }
        [UsedImplicitly] public string XPath { get; set; }
    }

    [UsedImplicitly]
    public class AmlFragment

    {
        [UsedImplicitly] public string AmlFile { get; set; }
        [UsedImplicitly] public List<AmlNode> Nodes { get; set; }
    }

    [UsedImplicitly]
    public class XmlNode

    {
        [UsedImplicitly] [CanBeNull] public string File { get; set; }
        [UsedImplicitly] [CanBeNull] public string Fragment { get; set; }
        [UsedImplicitly] public string ExistenceXPath { get; set; }
        [UsedImplicitly] public string AdditionXPath { get; set; }
    }

    [UsedImplicitly]
    public class XmlFragment
    {
        [UsedImplicitly] public string RemoteFile { get; set; }
        [UsedImplicitly] public List<XmlNode> Nodes { get; set; }
    }

    [UsedImplicitly]
    public class ClientFile
    {
        [UsedImplicitly] public string Local { get; set; }
        [UsedImplicitly] public string Remote { get; set; }
    }

    public class ArasFeatureManifest
    {
        [UsedImplicitly]
        public List<AmlFragment> AmlFragments { get; set; }
            = new List<AmlFragment>();

        [UsedImplicitly]
        public List<XmlFragment> XmlFragments { get; set; }
            = new List<XmlFragment>();

        [UsedImplicitly]
        public List<ClientFile> ServerFiles { get; set; }
            = new List<ClientFile>();

        /// <summary>
        /// Local directory of manifest file
        /// Set by arassync when loading the manifest from disk
        /// </summary>
        public string LocalDirectory { get; internal set; }
    }
}