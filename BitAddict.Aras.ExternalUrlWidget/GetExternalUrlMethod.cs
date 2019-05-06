// MIT License, see COPYING.TXT

using Aras.IOM;
using JetBrains.Annotations;

namespace BitAddict.Aras.ExternalUrlWidget
{
    /// <inheritdoc />
    ///  <summary>
    ///  Generates an external URL for an item on an Aras web server
    ///  It is a Server method to demonstrate some features
    ///  </summary>
    public class GetExternalUrlMethod : ArasMethod
    {
        [XmlProperty("id")] [UsedImplicitly] public string Id { get; set; }
        [XmlProperty("type")] [UsedImplicitly] public string Type { get; set; }

        [UsedImplicitly]
        [XmlProperty("baseurl")]
        public string BaseUrl { get; set; }

        /// <inheritdoc/>
        /// <summary>
        /// Returns direct URL to file if it is a File item, otherwise
        /// builds url from items
        /// </summary>
        public override Item DoApply(Item root)
        {
            // populate properties from xml
            XmlPropertyAttribute.BindXml(root.node, this);

            // log info
            Log(nameof(DoApply), $"Generating URL for {Type} ID {Id})");

            // generate url
            var url = Type == "File"
                ? Innovator.getFileUrl(Id, UrlType.None)
                : $"{BaseUrl}/default.aspx?StartItem={Type}:{Id}";

            // return result
            return Innovator.newResult(url);
        }
    }
}