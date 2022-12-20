using System.IO;
using System.Threading.Tasks;
using ConfigurationService.Client.Parsers;
using Xunit;

namespace ConfigurationService.Test;

public class ParserTests
{
    [Theory]
    [InlineData("Files/test.json")]
    public async Task Parses_Valid_Json(string path)
    {
        await using var stream = new FileStream(path, FileMode.Open);
        var parser = new JsonConfigurationFileParser();
        var output = parser.Parse(stream);
            
        Assert.NotEmpty(output);
    }

    [Theory]
    [InlineData("Files/test.xml")]
    public async Task Parses_Valid_Xml(string path)
    {
        await using var stream = new FileStream(path, FileMode.Open);
        var parser = new XmlConfigurationFileParser();
        var output = parser.Parse(stream);
            
        Assert.NotEmpty(output);
    }
        
    [Theory]
    [InlineData("Files/test.yaml")]
    public async Task Parses_Valid_Yaml(string path)
    {
        await using var stream = new FileStream(path, FileMode.Open);
        var parser = new YamlConfigurationFileParser();
        var output = parser.Parse(stream);
            
        Assert.NotEmpty(output);
    }
        
    [Theory]
    [InlineData("Files/test.ini")]
    public async Task Parses_Valid_Ini(string path)
    {
        await using var stream = new FileStream(path, FileMode.Open);
        var parser = new IniConfigurationFileParser();
        var output = parser.Parse(stream);
            
        Assert.NotEmpty(output);
    }
}