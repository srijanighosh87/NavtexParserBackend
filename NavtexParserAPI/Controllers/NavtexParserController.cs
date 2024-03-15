using HelperLibrary;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using NavtexPositionParser.Base;
using NavtexPositionParser.Commands;
using NavtexPositionParser.Dtos;
using Swashbuckle.AspNetCore.Annotations;
using System.Runtime.CompilerServices;

namespace NavtexPositionParser.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class NavtexParserController : ControllerBase
    {

        private readonly ILogger<NavtexParserController> _logger;
        private readonly IBaseManager<ParseNavtexCommand,ParsedNavtexDto> _parseManager; 

        public NavtexParserController(
            ILogger<NavtexParserController> logger,
            IBaseManager<ParseNavtexCommand, ParsedNavtexDto> parseManager)
        {
            _logger = logger;
            _parseManager = parseManager;
        }


        /// <summary>
        /// Reads an uploaded Navtex file and returns valid Message along with highlighted coordinates
        /// </summary>
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ReturnDto<ParsedNavtexDto>>> ParseAsync(IFormFile file)
        {
            try
            {
                //local checks
                if (file == null || file.Length == 0)
                {
                    return BadRequest("Invalid File Uploaded!");
                }
                if (Path.GetExtension(file.FileName) != ".txt")
                {
                    return BadRequest("Only .txt files are allowed.");
                }

                var response = await _parseManager.ProcessAsync(new ParseNavtexCommand
                {
                    file = file
                });
                return Ok(new ReturnDto<ParsedNavtexDto>(message: "Success", content: response));
            }
            catch (Exception e)
            {
                _logger.LogError(e, "An error occurred while processing the NAVTEX file.");
                string errorMessage = ".";
                int statusCode;
                if (e is FileNotFoundException)
                {
                    errorMessage = "The NAVTEX file could not be found.";
                    statusCode = StatusCodes.Status404NotFound;
                }
                else
                {
                    errorMessage = e.Message;
                    statusCode = StatusCodes.Status500InternalServerError;
                }

                // Return the error response with proper HTML code
                return StatusCode(statusCode, new ReturnDto<string>(message: errorMessage, content: null));
            }
            
        }
    }
}
