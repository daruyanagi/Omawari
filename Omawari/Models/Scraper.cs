using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Omawari.Models
{
    using System.Reflection;
    using System.IO;
    using Newtonsoft.Json;
    using AngleSharp.Dom.Html;
    using System.Net.Http;
    using AngleSharp.Parser.Html;

    public class Scraper : BindableBase
    {
        private Uri target = default(Uri);
        private string selectors = default(string);
        private ScrapingStatus status = ScrapingStatus.Pending;
        private Guid id = Guid.NewGuid();
        private string name = default(string);
        private int interval = 3 * 60; // 分単位
        private bool isDynamic = false;

        public Guid Id
        {
            get { return id; }
            set { SetProperty(ref id, value); }
        }

        public string Name
        {
            get { return name; }
            set { SetProperty(ref name, value); }
        }

        public Uri Target
        {
            get { return target; }
            set { SetProperty(ref target, value); }
        }

        public string Selectors
        {
            get { return selectors; }
            set { SetProperty(ref selectors, value); }
        }

        public int Interval
        {
            get { return interval; }
            set { SetProperty(ref interval, value); }
        }

        public bool IsDynamic
        {
            get { return isDynamic; }
            set { SetProperty(ref isDynamic, value); }
        }

        [JsonIgnore]
        public ScrapingStatus Status
        {
            get { return status; }
            set { SetProperty(ref status, value); }
        }

        [JsonIgnore]
        public string ScrapingResultPath
        {
            get
            {
                var path = Path.Combine(App.DataFolder, Id.ToString());
                Directory.CreateDirectory(path);
                return path;
            }
        }

        [JsonIgnore]
        public ScrapingResult LastResult
        {
            get { return Results.FirstOrDefault(); }
        }

        [JsonIgnore]
        public List<ScrapingResult> Results
        {
            get
            {
                return Directory
                  .EnumerateFiles(ScrapingResultPath)
                  .Select(_ => File.ReadAllText(_).Deserialize<ScrapingResult>())
                  .OrderByDescending(_ => _.StartedAt)
                  .ToList();
            }
        }
        
        public async Task CheckAsync()
        {
            var result = await RunAsync();

            var root = Path.Combine(Utilities.EnvironmentHelper.GetAppFolderOnOneDrive(), Id.ToString());
            Directory.CreateDirectory(root);

            var old = string.Empty;
            var latest = Directory.EnumerateFiles(root).LastOrDefault(); // ちゃんと時系列順かなぁ？
            if (latest != null) old = File.ReadAllText(latest).Deserialize<ScrapingResult>().Text;
            if (!string.IsNullOrEmpty(old) && old != result.Text)
            {
                Status = ScrapingStatus.Updated;
                App.ShowInformation($"{Name} is updated.");
            }

            var path = Path.Combine(root, DateTime.UtcNow.Ticks.ToString());
            path = Path.ChangeExtension(path, ".json");

            File.WriteAllText(path, result.Serialize());

            OnPropertyChanged(nameof(Results));
            OnPropertyChanged(nameof(LastResult));
        }

        public async Task<ScrapingResult> RunAsync()
        {
            return IsDynamic
                ? await RunDynamicAsync()
                : await RunStaticAsync();
        }

        public async Task<ScrapingResult> RunStaticAsync()
        {
            Status = ScrapingStatus.Running;

            var result = new ScrapingResult();
            result.StartedAt = DateTime.UtcNow;
            result.Url = Target.ToString();
            result.Selector = Selectors;

            try
            {
                var doc = default(IHtmlDocument);
                using (var client = new HttpClient())
                using (var stream = await client.GetStreamAsync(Target))
                {
                    var parser = new HtmlParser();
                    doc = await parser.ParseAsync(stream);
                }

                result.Status = "success";
                result.Text = doc.QuerySelector(Selectors).OuterHtml;
                Status = ScrapingStatus.Succeeded;
            }
            catch
            {
                result.Status = "fail";
                Status = ScrapingStatus.Exception;
            }

            result.CompletedAt = DateTime.UtcNow;
            return result;
        }

        public async Task<ScrapingResult> RunDynamicAsync()
        {
            Status = ScrapingStatus.Running;
            
            var result = default(ScrapingResult);

            await Task.Factory.StartNew(() =>
            {
                try
                { 
                    var root_dir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                    var work_dir = Path.Combine(root_dir, "Tools");
                    var script_name = "scrape.js";

                    var info = new System.Diagnostics.ProcessStartInfo()
                    {
                        Arguments = $@"--web-security=no ""{script_name}"" ""{Target}"" ""{Selectors}""",
                        FileName = System.IO.Path.Combine(work_dir, "phantomjs.exe"),
                        CreateNoWindow = true,
                        RedirectStandardOutput = true,
                        StandardOutputEncoding = System.Text.Encoding.UTF8,
                        UseShellExecute = false,
                        WorkingDirectory = work_dir,
                    };

                    using (var process = new System.Diagnostics.Process() { StartInfo = info, })
                    {
                        var output = string.Empty;

                        process.OutputDataReceived += (s, a) =>
                        {
                            output += a.Data;
                            System.Diagnostics.Debug.WriteLine(a.Data);
                        };

                        process.Start();
                        process.BeginOutputReadLine();
                        process.WaitForExit();

                        // エラー出力をちょん切る
                        var r = new System.Text.RegularExpressions.Regex("{.+}");
                        output = r.Match(output).Value;

                        result = JsonConvert.DeserializeObject<ScrapingResult>(output);

                        Status = result.Status == "success"
                            ? ScrapingStatus.Succeeded
                            : ScrapingStatus.Failed;
                    }
                }
                catch
                {
                    Status = ScrapingStatus.Exception;
                }
            });
            
            return result;
        }

        public Scraper Clone()
        {
            return (this.Serialize()).Deserialize<Scraper>();
        }
    }
}
