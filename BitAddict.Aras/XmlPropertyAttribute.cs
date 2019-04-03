using System;
using System.Linq;
using System.Xml;

namespace BitAddict.Aras
{
    /// <summary>
    /// Marks a property as readable from XML element.
    ///
    /// Allows easy binding from XML data to C# properties.
    /// Example: Decoding Aras Method call parameters from Item body.
    /// </summary>
    public class XmlPropertyAttribute : Attribute
    {
        /// <summary>
        /// Create default instance with null element name
        /// </summary>
        public XmlPropertyAttribute()
        { }

        /// <summary>
        /// Create instance, setting element name and required flag
        /// </summary>
        /// <param name="elementName"></param>
        /// <param name="required"></param>
        public XmlPropertyAttribute(string elementName, bool required = false)
        {
            ElementName = elementName;
            Required = required;
        }

        /// <summary>
        /// If element is required. Binding will fail if not set.
        /// </summary>
        public bool Required { get; set; }

        /// <summary>
        /// XML element name to bind to
        /// </summary>
        public string ElementName { get; set; }

        /// <summary>
        /// Sets properties on an object matching XML child element's tag names
        /// using their InnerText as value via [XmlProperty] attributes.
        ///
        /// Attempts to convert values to matching type via System.Convert class.
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="element"></param>
        public static void BindXml(XmlElement element, object obj)
        {
            var type = obj.GetType();
            var xmlProps = type.GetProperties()
                .Where(pi => pi.GetCustomAttributes(typeof(XmlPropertyAttribute), true)
                    .Any())
                .ToList();

            var requiredProps = xmlProps
                .Where(pi => pi.GetCustomAttributes(typeof (XmlPropertyAttribute), true)
                    .Cast<XmlPropertyAttribute>()
                    .First()
                    .Required)
                .ToList();

            var errors = "";

            foreach (var child in element.ChildNodes.OfType<XmlElement>())
            {
                var prop = xmlProps
                    .FirstOrDefault(pi =>
                    {
                        var attr = pi.GetCustomAttributes(typeof (XmlPropertyAttribute), true)
                            .Cast<XmlPropertyAttribute>()
                            .First();

                        return attr.ElementName == child.LocalName ||
                               attr.ElementName == null &&
                               string.Equals(child.LocalName, pi.Name, StringComparison.InvariantCultureIgnoreCase);
                    });

                if (prop == null)
                    throw new XmlException($"Class {type.FullName} has no property " +
                                           $"bound to Xml element '{child.LocalName}'.");

                try
                {
                    var value = Convert.ChangeType(child.InnerText, prop.PropertyType);
                    prop.SetValue(obj, value, null);
                    requiredProps.Remove(prop); // non-required are ignored
                }
                catch (Exception e)
                {
                    var msg =
                        $"Error setting '{prop.Name}' as '{prop.PropertyType}' " +
                        $"from value '{child.InnerText}' in element'{child.LocalName}'" +
                        $": {e.Message}.\n";

                    errors += msg;
                    ArasExtensions.LogException(msg,e);
                }
            }

            if (requiredProps.Any())
               errors += "Failed to set all required xml elements: [" +
                          string.Join(", ", requiredProps.Select(pi => pi.Name)) + "]\n";

            if (errors.Any())
                throw new XmlException(errors);
        }
    }
}