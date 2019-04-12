// MIT License, see COPYING.TXT
using System.Diagnostics.CodeAnalysis;
using System.Xml;
using Microsoft.VisualStudio.TestTools.UnitTesting;

#pragma warning disable 1591

namespace BitAddict.Aras.UnitTests
{
    [TestClass]
    [SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Local")]
    public class TestXmlPropertyAttribute
    {
        private class XmlClass
        {
            [XmlProperty("thestring")]
            public string String { get; set; }
            [XmlProperty("thebool")]
            public bool Bool { get; set; }
            [XmlProperty]
            public int Int { get; set; }
        }

        private class XmlClassReq
        {
            [XmlProperty(Required = true)]
            public int Int { get; set; }
        }

        private class XmlClassInherit : XmlClass
        {
            [XmlProperty]
            public int SubClassInt { get; set; }
        }

        private readonly XmlClass _obj = new XmlClass();
        private readonly XmlClassReq _objReq = new XmlClassReq();
        private readonly XmlDocument _doc = new XmlDocument();

        [TestMethod]
        public void TestBindNothing()
        {
            _doc.LoadXml("<body></body>");
            XmlPropertyAttribute.BindXml(_doc.DocumentElement, _obj);
            Assert.AreEqual(null, _obj.String);
        }

        [TestMethod]
        public void TestIgnoresText()
        {
            _doc.LoadXml("<body> bah </body>");
            XmlPropertyAttribute.BindXml(_doc.DocumentElement, _obj);
            Assert.AreEqual(null, _obj.String);
        }

        [TestMethod]
        public void TestBindSimple()
        {
            _doc.LoadXml("<body><thestring>42</thestring></body>");
            XmlPropertyAttribute.BindXml(_doc.DocumentElement, _obj);
            Assert.AreEqual("42", _obj.String);
        }

        [TestMethod]
        public void TestConvert()
        {
            _doc.LoadXml("<body><thebool>true</thebool></body>");
            XmlPropertyAttribute.BindXml(_doc.DocumentElement, _obj);
            Assert.AreEqual(true, _obj.Bool);
        }


        [TestMethod]
        public void TestDefaultName()
        {
            _doc.LoadXml("<body><int>47</int></body>");
            XmlPropertyAttribute.BindXml(_doc.DocumentElement, _obj);
            Assert.AreEqual(47, _obj.Int);
        }


        [TestMethod]
        public void TestRequiredSet()
        {
            _doc.LoadXml("<body><int>3</int></body>");
            XmlPropertyAttribute.BindXml(_doc.DocumentElement, _objReq);
            Assert.AreEqual(3, _objReq.Int);
        }

        [TestMethod]
        public void TestClassInheritanceSetsAll()
        {
            var obj = new XmlClassInherit();
            _doc.LoadXml("<body><int>3</int><subclassInt>42</subclassInt></body>");
            XmlPropertyAttribute.BindXml(_doc.DocumentElement, obj);
            Assert.AreEqual(3, obj.Int);
            Assert.AreEqual(42, obj.SubClassInt);
        }

        [TestMethod]
        public void TestMissingRequiredThrows()
        {
            _doc.LoadXml("<body></body>");
            ExceptionAssert.Throws<XmlException>(() =>
                XmlPropertyAttribute.BindXml(_doc.DocumentElement, _objReq));
        }

        [TestMethod]
        public void TestUnknownElementNameThrows()
        {
            _doc.LoadXml("<body><fail>indeed</fail></body>");
            ExceptionAssert.Throws<XmlException>(() =>
                XmlPropertyAttribute.BindXml(_doc.DocumentElement, _objReq));
        }
    }
}

