﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.IO;

namespace Omawari.Models
{
    public class ScrapingResult
    {
        public ScrapingResult() { }
        public ScrapingResult(ScrapingRule scraper) : this() { ScraperId = scraper.Id; }

        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid ScraperId { get; set; }
        public string Target { get; set; }
        public string Selector { get; set; }
        public string Status { get; set; }
        public string Text { get; set; }
        public DateTime StartedAt { get; set; }
        public DateTime CompletedAt { get; set; }

        [JsonIgnore]
        public ScrapingRule Scraper { get { return App.Instance.ScraperCollection.FirstOrDefault(_ => _.Id == ScraperId); } }
        [JsonIgnore]
        public TimeSpan Duration { get { return CompletedAt - StartedAt; } }
        [JsonIgnore]
        public string Location
        {
            get
            {
                if (Scraper == null)
                {
                    System.Windows.Forms.MessageBox.Show("Parent rule is not found. It may be deleted.");

                    return null;
                };

                return Path.Combine(Scraper.Location, Id.ToString() + ".json");
            }
        }
        [JsonIgnore]
        public string TextSingleLine { get { return Text?.Replace("\n", "").Trim(); } }

        public string Diff(ScrapingResult result)
        {
            if (result?.Text == null) return "Target is null";

            if (result?.Text == Text) return "Defference is not find.";

            var diff = new HtmlDiff.HtmlDiff(result?.Text, Text);
            var html = diff.Build();
            return html;
        }
    }
}
