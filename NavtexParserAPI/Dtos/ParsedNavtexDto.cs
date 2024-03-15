using NavtexPositionParser.Base;

namespace NavtexPositionParser.Dtos
{
    public class ParsedNavtexDto : IDto
    {
        public string validMessage { get; set; }
        public List<string> coordinates { get; set; }
    }
}