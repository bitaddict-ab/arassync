using System;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.XPath;

namespace BitAddict.Aras.ArasSyncTool.Ops
{
    internal class Xml
    {
        private static string NormalizeNewlines(string str)
        {
            return Regex.Replace(str, @"\r\n|\n\r|\n|\r", "\r\n");
        }

        internal static void ExtractInnerTextToFile(string amlFile, string outFile, string xPathExpr)
        {
            Console.WriteLine($"  Reading {amlFile}");

            var doc = new XmlDocument();
            doc.Load(amlFile);

            ExtractInnerTextToFile(doc, outFile, xPathExpr);
        }

        internal static void ExtractInnerTextToFile(XmlDocument doc, string outFile, string xPathExpr)
        {
            var node = doc.SelectSingleNode(xPathExpr);
            if (node == null)
                throw new ArgumentException("xPathExpr", $"No node found matching {xPathExpr}");

            var dir = Path.GetDirectoryName(outFile);
            if (dir != null && !Directory.Exists(dir))
            {
                Console.WriteLine($"    Creating directory {dir}");
                Directory.CreateDirectory(dir);
            }

            Console.WriteLine($"    Extracting {outFile}");
            File.WriteAllText(outFile, NormalizeNewlines(node.InnerText));
        }

        internal static void MergeFileIntoCData(string amlFile, string codeFile, string xPathExpr)
        {
            Console.WriteLine($"    Reading {amlFile}");

            var doc = new XmlDocument();
            doc.Load(amlFile);

            MergeFileIntoCData(doc, codeFile, xPathExpr);

            Console.WriteLine($"    Writing {amlFile}");
            using (var w = GetFormattedXmlWriter(amlFile))
                doc.WriteTo(w);
        }

        internal static void MergeFileIntoCData(XmlDocument doc, string codeFile, string xPathExpr)
        {
            var node = doc.SelectSingleNode(xPathExpr);
            if (node == null)
                throw new ArgumentException("xPathExpr", $"No node found matching {xPathExpr}.");

            Console.WriteLine($"    Merging {codeFile}");

            var contents = File.ReadAllText(codeFile, Encoding.UTF8);
            var cdata = doc.CreateCDataSection(contents);
            node.InnerXml = cdata.OuterXml;
        }

        /// <summary>
        /// Merge an XML snippet into an XML document.
        /// </summary>
        /// <param name="doc">The XML docmument to merge the snippet into.</param>
        /// <param name="codeFile">File path to the XML snippet.</param>
        /// <param name="existenceXPathExpr">XPath expression to check if the snippet already exists in the
        /// document.</param>
        /// <param name="additionXPathExpr">XPath expression that finds the node the snippet will be appended to as a
        /// child.</param>
        /// <returns>If the document has been updated.</returns>
        internal static bool MergeFileIntoXml(XmlDocument doc, string contents, string existenceXPathExpr,
            string additionXPathExpr)
        {
            XmlNode nodeToAppendTo;
            try
            {
                nodeToAppendTo = doc.SelectSingleNode(additionXPathExpr);
                if (nodeToAppendTo == null)
                {
                    throw new ArgumentException("xPathExpr", $"No node found matching {additionXPathExpr}.");
                }
            }
            catch (XPathException e)
            {
                Console.Error.WriteLine($"Error in additionExpr '{additionXPathExpr}': {e.Message}");
                throw;
            }

            XmlNode existingNode;
            try
            {
                existingNode = doc.SelectSingleNode(existenceXPathExpr);
            }
            catch (XPathException e)
            {
                Console.Error.WriteLine($"Error in existenceExpr '{existenceXPathExpr}': {e.Message}");
                throw;
            }

            if (existingNode == null)
            {
                Console.WriteLine($"    Appending xml");
                nodeToAppendTo.AppendChild(StringToXmlNode(doc, contents));
                return true;
            }
            else
            {
                if (existingNode.Compare(contents))
                {
                    Console.WriteLine($"    File already contains up-to-date xml");
                    return false;
                }
                else
                {
                    Console.WriteLine($"    Merging changed xml");

                    existingNode.ParentNode?.RemoveChild(existingNode);
                    nodeToAppendTo.AppendChild(StringToXmlNode(doc, contents));

                    return true;
                }
            }
        }

        /// <summary>
        /// Convert an XML string to an XmlNode.
        /// </summary>
        /// <param name="document">The document the node will be used within.</param>
        /// <param name="xmlString">A string containing an XML structure.</param>
        /// <returns>A new XMLNode whose context is the provided document.</returns>
        internal static XmlNode StringToXmlNode(XmlDocument document, string xmlString)
        {
            var newNodeDocument = new XmlDocument();
            newNodeDocument.LoadXml(xmlString);
            if (newNodeDocument.DocumentElement == null)
                throw new NullReferenceException("XML not parsed as document?");

            return document.ImportNode(newNodeDocument.DocumentElement, true);
        }

        /// <summary>
        /// Returns xmlwriter whose output matches Aras AML XML formatting 
        /// </summary>
        /// <returns></returns>
        internal static XmlWriter GetFormattedXmlWriter(string filename)
        {
            var settings = new XmlWriterSettings
            {
                NewLineHandling = NewLineHandling.Replace,
                Indent = true,
                IndentChars = " ",
                Encoding = Encoding.UTF8,
                NewLineChars = Environment.NewLine,
                OmitXmlDeclaration = true // Aras import fails with this, as it places AML as part of another document
            };

            return XmlWriter.Create(filename, settings);
        }

        /// <summary>
        /// Save an XML file with the same formatting as used in Aras source code files.
        /// </summary>
        internal static void SaveFormattedXmlFile(XmlDocument doc, string filePath)
        {
            string xml;
            using (var memoryStream = new MemoryStream())
            {
                using (var writer = GetFormattedXmlFileWriter(memoryStream))
                {
                    doc.Save(writer);
                }

                memoryStream.Position = 0;
                using (var streamReader = new StreamReader(memoryStream))
                {
                    xml = streamReader.ReadToEnd();
                }
            }

            // Most self-closing XML tags in IncludeNamespaceConfig.xml does not have a space before the slash.
            xml = xml.Replace(" />", "/>");

            xml = NormalizeNewlines(xml);

            using (var streamWriter = new StreamWriter(filePath))
            {
                streamWriter.Write(xml);
            }
        }

        /// <summary>
        /// Returns XmlWriter whose output matches Aras' XML file formatting, such as IncludeNamespaceConfig.xml.
        /// </summary>
        internal static XmlWriter GetFormattedXmlFileWriter(MemoryStream stream)
        {
            var settings = new XmlWriterSettings
            {
                NewLineHandling = NewLineHandling.None,
                Indent = true,
                IndentChars = "\t",
                Encoding = Encoding.UTF8,
                OmitXmlDeclaration = false
            };

            return XmlWriter.Create(stream, settings);
        }
    }

    public static class XmlNodeExentensions
    {
        internal static bool Compare(this XmlNode node, string xmlString)
        {
            var xmlNode = new XmlDocument();
            xmlNode.LoadXml(xmlString);
            return node.OuterXml == xmlNode.OuterXml;
        }
    }
}