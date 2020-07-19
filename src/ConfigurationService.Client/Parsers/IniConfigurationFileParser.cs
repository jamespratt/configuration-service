using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Extensions.Configuration;

namespace ConfigurationService.Client.Parsers
{
    public class IniConfigurationFileParser : IConfigurationParser
    {
        private readonly IDictionary<string, string> _data = new SortedDictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        public IDictionary<string, string> Parse(Stream input) => ParseStream(input);

        private IDictionary<string, string> ParseStream(Stream input)
        {
            _data.Clear();

            using (var reader = new StreamReader(input))
            {
                var sectionPrefix = string.Empty;

                while (reader.Peek() != -1)
                {
                    var rawLine = reader.ReadLine();
                    if (rawLine == null)
                    {
                        continue;
                    }

                    var line = rawLine.Trim();

                    if (string.IsNullOrWhiteSpace(line))
                    {
                        continue;
                    }

                    if (line[0] == ';' || line[0] == '#' || line[0] == '/')
                    {
                        continue;
                    }

                    if (line[0] == '[' && line[line.Length - 1] == ']')
                    {
                        sectionPrefix = line.Substring(1, line.Length - 2) + ConfigurationPath.KeyDelimiter;
                        continue;
                    }

                    int separator = line.IndexOf('=');
                    if (separator < 0)
                    {
                        throw new FormatException($"Unrecognized line format '{rawLine}'.");
                    }

                    string key = sectionPrefix + line.Substring(0, separator).Trim();
                    string value = line.Substring(separator + 1).Trim();

                    if (value.Length > 1 && value[0] == '"' && value[value.Length - 1] == '"')
                    {
                        value = value.Substring(1, value.Length - 2);
                    }

                    if (_data.ContainsKey(key))
                    {
                        throw new FormatException($"A duplicate key '{key}' was found.");
                    }

                    _data[key] = value;
                }
            }
            return _data;
        }
    }
}