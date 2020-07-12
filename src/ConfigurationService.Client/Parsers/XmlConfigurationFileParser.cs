using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using Microsoft.Extensions.Configuration;

namespace ConfigurationService.Client.Parsers
{
    public class XmlConfigurationFileParser : IConfigurationParser
    {
        private const string NameAttributeKey = "Name";

        private readonly IDictionary<string, string> _data = new SortedDictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        public IDictionary<string, string> Parse(Stream input) => ParseStream(input);

        private IDictionary<string, string> ParseStream(Stream input)
        {
            _data.Clear();

            var readerSettings = new XmlReaderSettings()
            {
                CloseInput = false,
                DtdProcessing = DtdProcessing.Prohibit,
                IgnoreComments = true,
                IgnoreWhitespace = true
            };

            using (var reader = CreateXmlReader(input, readerSettings))
            {
                var prefixStack = new Stack<string>();

                SkipUntilRootElement(reader);

                ProcessAttributes(reader, prefixStack, _data, AddNamePrefix);
                ProcessAttributes(reader, prefixStack, _data, AddAttributePair);

                var preNodeType = reader.NodeType;
                while (reader.Read())
                {
                    switch (reader.NodeType)
                    {
                        case XmlNodeType.Element:
                            prefixStack.Push(reader.LocalName);
                            ProcessAttributes(reader, prefixStack, _data, AddNamePrefix);
                            ProcessAttributes(reader, prefixStack, _data, AddAttributePair);

                            if (reader.IsEmptyElement)
                            {
                                prefixStack.Pop();
                            }
                            break;

                        case XmlNodeType.EndElement:
                            if (prefixStack.Any())
                            {
                                if (preNodeType == XmlNodeType.Element)
                                {
                                    var key = ConfigurationPath.Combine(prefixStack.Reverse());
                                    _data[key] = string.Empty;
                                }

                                prefixStack.Pop();
                            }
                            break;

                        case XmlNodeType.CDATA:
                        case XmlNodeType.Text:
                            {
                                var key = ConfigurationPath.Combine(prefixStack.Reverse());
                                if (_data.ContainsKey(key))
                                {
                                    throw new FormatException($"A duplicate key '{key}' was found.");
                                }

                                _data[key] = reader.Value;
                                break;
                            }
                        case XmlNodeType.XmlDeclaration:
                        case XmlNodeType.ProcessingInstruction:
                        case XmlNodeType.Comment:
                        case XmlNodeType.Whitespace:
                            break;

                        default:
                            throw new FormatException($"Unsupported node type '{reader.NodeType}' was found.");
                    }
                    preNodeType = reader.NodeType;

                    if (preNodeType == XmlNodeType.Element &&
                        reader.IsEmptyElement)
                    {
                        preNodeType = XmlNodeType.EndElement;
                    }
                }
            }

            return _data;
        }

        private XmlReader CreateXmlReader(Stream input, XmlReaderSettings settings)
        {
            var memStream = new MemoryStream();
            input.CopyTo(memStream);
            memStream.Position = 0;

            var document = new XmlDocument();
            using (var reader = XmlReader.Create(memStream, settings))
            {
                document.Load(reader);
            }
            memStream.Position = 0;

            return XmlReader.Create(memStream, settings);
        }

        private void SkipUntilRootElement(XmlReader reader)
        {
            while (reader.Read())
            {
                if (reader.NodeType != XmlNodeType.XmlDeclaration &&
                    reader.NodeType != XmlNodeType.ProcessingInstruction)
                {
                    break;
                }
            }
        }

        private void ProcessAttributes(XmlReader reader, Stack<string> prefixStack, IDictionary<string, string> data,
            Action<XmlReader, Stack<string>, IDictionary<string, string>, XmlWriter> act, XmlWriter writer = null)
        {
            for (int i = 0; i < reader.AttributeCount; i++)
            {
                reader.MoveToAttribute(i);

                if (!string.IsNullOrEmpty(reader.NamespaceURI))
                {
                    throw new FormatException("Namespace is not supported.");
                }

                act(reader, prefixStack, data, writer);
            }

            reader.MoveToElement();
        }

        private static void AddNamePrefix(XmlReader reader, Stack<string> prefixStack,
            IDictionary<string, string> data, XmlWriter writer)
        {
            if (!string.Equals(reader.LocalName, NameAttributeKey, StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            if (prefixStack.Any())
            {
                var lastPrefix = prefixStack.Pop();
                prefixStack.Push(ConfigurationPath.Combine(lastPrefix, reader.Value));
            }
            else
            {
                prefixStack.Push(reader.Value);
            }
        }

        private static void AddAttributePair(XmlReader reader, Stack<string> prefixStack,
            IDictionary<string, string> data, XmlWriter writer)
        {
            prefixStack.Push(reader.LocalName);
            var key = ConfigurationPath.Combine(prefixStack.Reverse());
            if (data.ContainsKey(key))
            {
                throw new FormatException($"A duplicate key '{key}' was found.");
            }

            data[key] = reader.Value;
            prefixStack.Pop();
        }
    }
}