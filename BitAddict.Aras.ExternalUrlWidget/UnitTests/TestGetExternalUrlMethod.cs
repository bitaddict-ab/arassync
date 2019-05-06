// MIT License, see COPYING.TXT

using System;
using System.Linq;
using Aras.IOM;
using BitAddict.Aras.Test;
using NUnit.Framework;

namespace BitAddict.Aras.ExternalUrlWidget.UnitTests
{
    [TestFixture]
    [Parallelizable]
    public class TestGetExternalUrlMethod : ArasNUnitTestBase
    {
        private static readonly string[] TestItemTypes =
        {
            "Part",
            "File",
        };

        [Test]
        [TestCaseSource(nameof(TestItemTypes))]
        [Parallelizable]
        public void TestGetUrl(string itemType)
        {
            var item = GetAnyItemOfType(itemType);
            var result = CallMethod(item);

            Assert.IsFalse(result.isError());
            var url = result.getResult();

            Console.WriteLine("Got: " + url);
            
            Assert.That(url, Does.StartWith("http"));
            Assert.That(url, Does.Contain(item.getID()));
        }


        [Test]
        [Parallelizable]
        public void TestGetFile()
        {        
            var item = GetAnyItemOfType("File");
            var result = CallMethod(item);

            Assert.IsFalse(result.isError());
            var url = result.getResult();

            Console.WriteLine("Got: " + url);
            Assert.That(url, Does.Contain("/vault/"));
        }

        private static Item CallMethod(Item item)
        {
            var bodyItem = Innovator.newItem("Method", "GetExternalUrl");
            bodyItem.setProperty("type", item.getType());
            bodyItem.setProperty("id", item.getID());
            bodyItem.setProperty("baseurl", "http://myarasserver.local");

            var method = new GetExternalUrlMethod();
            var result = method.Apply(bodyItem);
            return result;
        }

        private static Item GetAnyItemOfType(string type)
        {
            var result = Innovator.ApplyAML(
                $"<AML>" +
                $"  <Item type='{type}' page='1' pagemax='1' pagesize='1' " +
                $"        select='id' itemmax='1' action='get'/>" +
                $"</AML>");

            return result.Enumerate().First();
        }
    }
}