using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sprache;

namespace Breaker.Core.Parsing
{
    public class SearchParser
    {
        public static readonly Parser<string> Command = Parse.String("g:").Or(Parse.String("f:")).Text().Token();
        //public static (string command, string term) ParseSearchTerm(string input)
        //{
        //    var result = Command.Parse(input);
        //}
    }
}
