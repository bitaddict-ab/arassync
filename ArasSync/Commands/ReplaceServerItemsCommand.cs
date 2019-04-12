// MIT License, see COPYING.TXT
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using System.Xml.XPath;
using Aras.IOM;
using BitAddict.Aras.ArasSync.Ops;
using JetBrains.Annotations;
using ManyConsole;

namespace BitAddict.Aras.ArasSync.Commands
{
    /// <summary>
    /// Update the project's AML files with delete actions for relationships that exist in the database but not locally.
    /// </summary>
    [CommandCategory("Advanced")]
    [UsedImplicitly]
    public class ReplaceServerItemsCommand : ConsoleCommand
    {
        public string Database { get; set; }

        private Innovator Innovator { get; set; }

        public ReplaceServerItemsCommand()
        {
            IsCommand("ReplaceServerItems",
                "Update the project's AML files with delete actions for relationships that exist in the database " +
                "but not locally.");

            HasOption("db=|database=", "Database Id to compare AML files with", db => Database = db);
        }

        public override int Run(string[] remainingArguments)
        {
            Innovator = Common.GetNewInnovator(Database);
            foreach (var filepath in AmlFilepaths())
            {
                var itemDocument = ItemDocument.FromFilepath(filepath);
                var serverItemDocument = itemDocument.GetServerVersion(Innovator);
                if (serverItemDocument == null)
                    continue;
                itemDocument.DeleteRelationshipsThatOnlyExistInOther(serverItemDocument);
                Console.WriteLine($"Updating {filepath}");
                itemDocument.Save(filepath);
            }
            return 0;
        }

        private static IEnumerable<string> AmlFilepaths()
        {
            // ReSharper disable once LoopCanBeConvertedToQuery
            foreach (var packageDir in Directory.GetDirectories("ArasExport"))
            {
                var importDir = Path.Combine(packageDir, "Import");
                foreach (var itemtypeDir in Directory.GetDirectories(importDir))
                {
                    foreach (var itemFile in Directory.GetFiles(itemtypeDir))
                    {
                        yield return itemFile;
                    }
                }
            }
        }

        private class ItemDocument
        {
            private XDocument Document { get; }

            private XElement RootItem => Document.XPathSelectElement("/AML/Item") ?? Document.XPathSelectElement("/Item");

            private XElement RelationshipItem => RootItem.XPathSelectElement("Relationships");

            private IEnumerable<XElement> RelatedItems
            {
                get
                {
                    // The exported Identity items seems to be a special case where there is no Relationships node.
                    // Instead, all Members are directly under the root AML node.
                    var exportedIdentityMembersOnDisk = Document.XPathSelectElements("/AML/Item").Skip(1);

                    var relationshipsItems = RelationshipItem?.XPathSelectElements("Item") ?? new List<XElement>();
                    return exportedIdentityMembersOnDisk.Concat(relationshipsItems);
                }
            }

            private IEnumerable<string> RelatedItemtypes => RelatedItems
                .Select(e => e.Attribute(XName.Get("type"))?.Value)
                .Where(e => e != null)
                .Distinct();

            private ItemDocument(TextReader stream)
            {
                Document = XDocument.Load(stream);
            }

            public static ItemDocument FromFilepath(string filepath)
            {
                using (var reader = new StreamReader(filepath))
                {
                    return new ItemDocument(reader);
                }
            }

            public ItemDocument GetServerVersion(Innovator innovator)
            {
                var rootItem = RootItem;
                var type = rootItem.Attribute(XName.Get("type"))?.Value;
                var id = rootItem.Attribute(XName.Get("id"))?.Value;

                var item = innovator.newItem(type, "get");
                item.setAttribute("id", id);
                foreach (var relatedItemtype in RelatedItemtypes)
                {
                    var accessRel = item.createRelationship(relatedItemtype, "get");
                    accessRel.setAttribute("select", "related_id(*)");
                }
                item = item.apply();

                if (item.node == null)
                    return null;

                using (var reader = new StringReader(item.node.OuterXml))
                {
                    return new ItemDocument(reader);
                }
            }

            public void DeleteRelationshipsThatOnlyExistInOther(ItemDocument other)
            {
                var relatedIds = RelatedIds();
                var nonexistingItemsInThisDocument = other.RelatedItems
                    .Select(x => new Tuple<string, string>(x.GetAttribute("type"), x.GetAttribute("id")))
                    .Where(t => !relatedIds.ContainsKey(t.Item1) || !relatedIds[t.Item1].Contains(t.Item2));
                foreach (var item in nonexistingItemsInThisDocument)
                {
                    RelationshipItem.Add(new XElement("Item",
                        new XAttribute("type", item.Item1),
                        new XAttribute("id", item.Item2),
                        new XAttribute("action", "delete")
                    ));
                }
            }

            public void Save(string filepath)
            {
                var settings = new XmlWriterSettings
                {
                    Indent = true,
                    IndentChars = " ",
                    OmitXmlDeclaration = true
                };

                using (var writer = XmlWriter.Create(filepath, settings))
                {
                    Document.Save(writer);
                }
            }

            private IDictionary<string, ISet<string>> RelatedIds()
            {
                var relatedIds = new Dictionary<string, ISet<string>>();
                foreach (var relatedItem in RelatedItems)
                {
                    var type = relatedItem.GetAttribute("type");
                    if (!relatedIds.ContainsKey(type))
                    {
                        relatedIds[type] = new HashSet<string>();
                    }
                    var id = relatedItem.GetAttribute("id");
                    relatedIds[type].Add(id);
                }
                return relatedIds;
            }
        }
    }

    internal static class Extensions
    {
        public static string GetAttribute(this XElement element, string attribute)
        {
            return element.Attribute(XName.Get(attribute))?.Value;
        }
    }
}
