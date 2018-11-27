using System;
using System.Collections.Generic;
using System.Text;
using CommandLine;

namespace UrlShortenerCrossPlatform
{
    public class Options
    {
        [Option('s', "service", Default = 2, Required = false, HelpText = "Set Service(Choose 1 To 6)\n1=Yon 2=Opizo 3=Bitly 4=Atrab 5=Plink 6=Do0 [Default=Opizo]")]
        public int Service { get; set; }

        [Option('l', "link", Required = true, HelpText = "Set Long url for shorting")]
        public string Link { get; set; }

        [Option('c', "custom", Required = false, HelpText = "Set Custom Name for Shorted Url [Yon, Plink]")]
        public string Custom { get; set; }

    }
}
