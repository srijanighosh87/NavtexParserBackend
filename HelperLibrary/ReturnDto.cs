using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace HelperLibrary
{
    public class ReturnDto<T>
    {
        public string Message { get; set; }
        public T Content { get; set; }

        /// <summary>
        /// Use this Constructor to create a standardized return object
        /// Use message for returning any relevant messages
        /// </summary>
        /// <param name="message"></param>
        /// <param name="content"></param>
        public ReturnDto(string message, T content)
        {
            Message = message;
            Content = content;
        }
    }
}
