using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using ConfigurationService.Providers;

namespace ConfigurationService.Hosting
{
    [Route("[controller]")]
    [ApiController]
    public class ConfigurationController : ControllerBase
    {
        private readonly IProvider _provider;

        public ConfigurationController(IProvider provider)
        {
            _provider = provider;
        }

        /// <summary>
        /// Returns the content of a configuration file.
        /// </summary>
        /// <param name="name"></param>
        /// <response code="200">Returns the content of the request configuration file</response>
        /// <response code="400">Returns details specifying why the request is not valid</response> 
        [HttpGet("{name}")]
        [ProducesResponseType(typeof(string), 200)]
        [ProducesResponseType(typeof(ProblemDetails), 400)]
        public IActionResult Get(string name)
        {
            name = WebUtility.UrlDecode(name);
            var bytes = _provider.GetFile(name);

            if (bytes == null)
            {
                return NotFound();
            }

            var fileContent = Encoding.UTF8.GetString(bytes);

            return Content(fileContent);
        }

        /// <summary>
        /// Returns a list of all configuration file names.
        /// </summary>
        /// <response code="200">Returns a list of all configuration file names</response>
        /// <response code="400">Returns details specifying why the request is not valid</response> 
        [HttpGet(nameof(List))]
        [Produces("application/json", Type = typeof(List<string>))]
        [ProducesResponseType(typeof(IEnumerable<string>), 200)]
        [ProducesResponseType(typeof(ProblemDetails), 400)]
        public IActionResult List()
        {
            var files = _provider.ListAllFiles().OrderBy(o => o);

            return Ok(files);
        }
    }
}