using NavtexPositionParser.Base;

namespace NavtexPositionParser.Commands
{
    public class ParseNavtexCommand : ICommand
    {
        public IFormFile file { get; set; }
    }
}