using Microsoft.Extensions.FileSystemGlobbing.Internal;
using NavtexPositionParser.Base;
using NavtexPositionParser.Commands;
using NavtexPositionParser.Dtos;
using System.Text;
using System.Text.RegularExpressions;

namespace NavtexPositionParser.Managers
{
    public class ParseNavtexManager : IBaseManager<ParseNavtexCommand, ParsedNavtexDto>
    {
        private readonly ILogger<ParseNavtexManager> _logger;
        private readonly string StartString = "ZCZC";
        private readonly string EndString = "NNNN";

        private readonly string[] regexes =
        {@"\d{1,2}\s?[N|S]\s?\d{1,3}\s?[E|W]",
         @"\d{1,2}-?\s?\d{2},?.?\d{2}\s?[N|S]\s?-?\s?\d{3}\s?-?\d{2},?.?\d{1}(\d{1})?\s?[W|E]",
         @"\d{1,2}-\d{2},?.?\d{1}[N|S]\s\s?\d{3}-\d{2},?.?\d{1}(\d{1})?[W|E]",
         @"\d{2}-\d{2}.\d{1}[N|S]\s:\d{2}-\d{2}.?\d{1}[W|E]",
         @"\d{2}-\d{2}[N|S]\s\d{3}-\d{2}[W|E]"
        };
        public ParseNavtexManager(ILogger<ParseNavtexManager> logger)
        { 
            _logger = logger;
        }

        public async Task<ParsedNavtexDto> ProcessAsync(ParseNavtexCommand input)
        {
            try
            {
                using (var streamReader = new StreamReader(input.file.OpenReadStream(), Encoding.UTF8))
                {
                    string fileContent = await streamReader.ReadToEndAsync();
                    ParsedNavtexDto parsedData = ParseNavtexContent(fileContent);
                    return parsedData;
                }
            }
            catch (Exception e)
            {
                _logger.LogError(e, "An error occurred while reading the uploaded file.");
                throw;
            }
        }

        private ParsedNavtexDto ParseNavtexContent(string fileContent)
        {
            var validContent = RetrieveValidContent(fileContent);
            var coordinates = HighlightCoordinates(validContent);
            return new ParsedNavtexDto
            {
                validMessage = validContent,
                coordinates = coordinates,
            };
        }

        /// <summary>
        /// Checks data from all regex in the regexes list
        /// </summary>
        /// <param name="validContent"></param>
        /// <returns></returns>
        private List<string> HighlightCoordinates(string validContent)
        {
            var returnList = new List<string>();    

            foreach(var regexCheck in regexes)
            {
                var res = checkMatch(validContent, regexCheck);
                if (res.Count > 0)
                {
                    // add only if it does not exist already
                    returnList.AddRange(res.Except(returnList));
                }
            }
            return returnList;
        }

        /// <summary>
        /// Regex Checker
        /// </summary>
        /// <param name="validContent"></param>
        /// <param name="regexString"></param>
        /// <returns></returns>
        private List<string> checkMatch(string validContent, string regexString)
        {
            var returnList = new List<string>();
            Regex regex = new Regex(regexString, RegexOptions.Singleline);
            MatchCollection matches = regex.Matches(validContent);
            foreach (Match match in matches)
            {
                returnList.Add(match.Value);
            }
            return returnList;
        }

        
        /// <summary>
        /// Reads Data between "ZCZC" and "NNNN" and trims before returning it
        /// </summary>
        /// <param name="navtexContent"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        private string RetrieveValidContent(string navtexContent)
        {
            string patternForNavtextMessage = @"ZCZC.*?NNNN";
            Regex regex = new Regex(patternForNavtextMessage, RegexOptions.Singleline);
            MatchCollection matches = regex.Matches(navtexContent);
            if (matches.Count != 1)
                throw new Exception("Invalid Navtex file");
            var message = matches[0].Value;
            message = message.Replace(StartString, "").Replace(EndString, "").Trim();

            return message;
        }
    }
}
