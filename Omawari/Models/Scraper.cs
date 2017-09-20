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
        private int interval = App.GlobalSettings.DefaultInterval; // 分単位
        private bool isDynamic = false;
        private bool isEnabled = true;

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

        public bool IsEnabled
        {
            get { return isEnabled; }
            set { SetProperty(ref isEnabled, value); }
        }

        [JsonIgnore]
        public ScrapingStatus Status
        {
            get { return status; }
            set { SetProperty(ref status, value); }
        }

        [JsonIgnore]
        public string Location
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
            get { return AllResults.FirstOrDefault(); }
        }

        [JsonIgnore]
        public ScrapingResult LastUpdateResult
        {
            get { return UpdateResults.FirstOrDefault(); }
        }

        [JsonIgnore]
        public List<ScrapingResult> AllResults
        {
            get
            {
                return Directory
                  .EnumerateFiles(Location)
                  .Select(_ => File.ReadAllText(_).Deserialize<ScrapingResult>())
                  .OrderByDescending(_ => _.CompletedAt)
                  .ToList();
            }
        }

        [JsonIgnore]
        public List<ScrapingResult> UpdateResults
        {
            get
            {
                return AllResults.GroupBy(_ => _.Text)
                    .Select(_ => _.OrderByDescending(__ => __.CompletedAt).Last())
                    .ToList();
            }
        }

        public async Task CheckAsync()
        {
            if (!IsEnabled) return;

            var result = await RunAsync();
            var old = LastResult;

            // スクレイピング結果の保存
            File.WriteAllText(result.Location, result.Serialize());

            // プロパティ更新。更新チェックの先にやっておかないと、App.Updated() がイヤんなことになる
            OnPropertyChanged(nameof(AllResults));
            OnPropertyChanged(nameof(UpdateResults));
            OnPropertyChanged(nameof(LastResult));
            OnPropertyChanged(nameof(LastUpdateResult));

            // 更新（新規も含む）のチェック
            if (old == null || old.Text != result.Text)
            {
                Status = ScrapingStatus.Updated;

                App.Updated(this, result); // イベントにしたいものだ
            }
        }

        public async Task<ScrapingResult> RunAsync()
        {
            Status = ScrapingStatus.Running;
            var result = new ScrapingResult(this);
            
            result.StartedAt = DateTime.UtcNow;
            result.Target = Target?.ToString();
            result.Selector = Selectors;

            try
            {
                // スクレイピング処理のコア（時間がかかります！）
                var (status, text) = IsDynamic
                    ? await RunDynamicAsync()
                    : await RunStaticAsync();

                result.Status = status;
                result.Text = text;
                Status = status == "success"
                    ? ScrapingStatus.Succeeded
                    : ScrapingStatus.Failed;
            }
            catch (Exception e)
            {
                result.Status = "exception";
                result.Text = e.Message;
                Status = ScrapingStatus.Failed;
            }

            result.CompletedAt = DateTime.UtcNow;

            return result;
        }

        public async Task<(string, string)> RunStaticAsync()
        {
            try
            {
                using (var client = new HttpClient())
                using (var stream = await client.GetStreamAsync(Target))
                {
                    var doc = default(IHtmlDocument);
                    var parser = new HtmlParser();
                    doc = await parser.ParseAsync(stream);
                    
                    return ("success", doc.QuerySelector(Selectors).OuterHtml);
                }
            }
            catch
            {
                return ("fail", string.Empty);
            }
        }

        public async Task<(string, string)> RunDynamicAsync()
        {
            return await Task.Factory.StartNew(new Func<(string, string)>(() =>
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

                        // エラー出力をちょん切る（もしかするともっといい方法があるのかもしれない）
                        var r = new System.Text.RegularExpressions.Regex("{.+}");
                        output = r.Match(output).Value;

                        // シンプルにしたいけど、処理を壊すのもめんどいので後回し
                        var result = JsonConvert.DeserializeObject<ScrapingResult>(output);
                        return (result.Status, result.Text);
                    }
                }
                catch
                {
                    return ("fail", string.Empty);
                }
            }));
        }

        public Scraper Clone()
        {
            return (this.Serialize()).Deserialize<Scraper>();
        }
    }
}
