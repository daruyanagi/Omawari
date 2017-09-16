using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Omawari.Models
{
    public class ScrapingResult
    {
        public string Url { get; set; }
        public string Selector { get; set; }
        public string Status { get; set; }
        public string Text { get; set; }
        public DateTime StartedAt { get; set; }
        public DateTime CompletedAt { get; set; }

        public string TextSingleLine { get { return Text?.Replace("\n", "").Trim(); } }
        public TimeSpan Duration { get { return CompletedAt - StartedAt; } }

        public string Diff(ScrapingResult result)
        {
            if (result.Text == Text) return "Defference is not find.";

            var diff = new HtmlDiff.HtmlDiff(result.Text, Text);
            var html = diff.Build();
            return $@"
<html>
    <head>
        <meta http-equiv='Content-Type' content='text/html;charset=UTF-8'>
        <style>
            ins.diffins {{ background-color: #cfc; text-decoration: none; }} 
            del.diffdel {{ color: #999; background-color:#FEC8C8; }}
        </style>
    </head>
    <body>
        {html}
    </body>
</html>";
        }
    }
}
