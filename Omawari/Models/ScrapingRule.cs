﻿using System;
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
    using System.Runtime.CompilerServices;

    public class ScrapingRule : BindableBase
    {
        private Uri target = default(Uri);
        private string selectors = default(string);
        private ScrapingStatus status = ScrapingStatus.Pending;
        private Guid id = Guid.NewGuid();
        private string name = default(string);
        private int interval = App.Instance.GlobalSettings.DefaultInterval; // 分単位
        private bool isDynamic = false;
        private bool isEnabled = true;
        private List<ScrapingResult> allResults = null;

        protected override void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            base.OnPropertyChanged(propertyName);

            App.Instance.ScraperCollection?.Save(); // Add/Edit してる時も動いちゃうけど、まぁ、いいか
        }

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
                var path = Path.Combine(App.Instance.DataFolder, Id.ToString());
                Directory.CreateDirectory(path);
                return path;
            }
        }

        [JsonIgnore]
        public ScrapingResult LastResult
        {
            get { return AllResults?.FirstOrDefault(); }
        }

        [JsonIgnore]
        public ScrapingResult LastUpdateResult
        {
            get { return UpdateResults?.FirstOrDefault(); }
        }

        [JsonIgnore]
        public List<ScrapingResult> AllResults
        {
            get { return allResults; }
        }

        [JsonIgnore]
        public List<ScrapingResult> UpdateResults
        {
            get
            {
                return AllResults?
                    .Where(_ =>
                        _.Status.ToLower() == "success" ||
                        _.Status.ToLower() == "succeeded" ||
                        _.Status.ToLower() == "update" ||
                        _.Status.ToLower() == "updated" ||
                        _.Status.ToLower() == "new") // 失敗は弾く v1.4 追加
                    .GroupBy(_ => _.Text)
                    .Select(_ => _.OrderByDescending(__ => __.CompletedAt).Last())
                    .ToList();
            }
        }

        public async Task CheckAsync()
        {
            if (!IsEnabled) return;
            
            // 新しい結果を保存する前に古い結果を保管
            var old_result = LastUpdateResult; 

            // 新しい結果を取得・ファイルとして保存
            var result = await RunAsync();
            if (result == null) return;
            else if (result.Status == "fail") result.Status = "Failed";
            else if (string.IsNullOrEmpty(result.Text)) result.Status = "Empty";
            else if (old_result == null) result.Status = "New";
            else if (old_result.Text != result.Text) result.Status = "Updated";
            else result.Status = "No Changed";
            File.WriteAllText(result.Location, JsonConvert.SerializeObject(result, Formatting.Indented));

            // プロパティ更新。更新チェックの先にやっておかないと、App.Updated() がイヤんなことになる
            await UpdateResultsAsync();
            
            switch (result.Status.ToLower())
            {
                case "succeeded":
                case "success":
                    // 実質廃止になっているはず ToDo: うまく動いていたら次のバージョンで削除する
                    Status = ScrapingStatus.Succeeded;
                    break;
                case "no changed":
                    Status = ScrapingStatus.NoChanged;
                    break;
                case "new":
                    Status = ScrapingStatus.New;
                    App.Instance.NotifyUpdateDetected(this, result);
                    break;
                case "update":
                case "updated":
                    Status = ScrapingStatus.Updated;
                    App.Instance.NotifyUpdateDetected(this, result);
                    break;
                case "empty":
                    Status = ScrapingStatus.Empty;
                    // ToDo: 失敗イベントを新設する
                    break;
                case "fail":
                case "failed":
                    Status = ScrapingStatus.Failed;
                    // ToDo: 失敗イベントを新設する
                    break;
                default:
                    //
                    break;
            }
        }

        public async Task UpdateResultsAsync()
        {
            var files = Directory.EnumerateFiles(Location);

            await Task.Factory.StartNew(async () =>
            {
                while (true)
                {
                    try
                    {
                        allResults = Directory
                          .EnumerateFiles(Location)
                          .Select(_ => File.ReadAllText(_))
                          .Select(_ => JsonConvert.DeserializeObject<ScrapingResult>(_))
                          .OrderByDescending(_ => _.CompletedAt)
                          .ToList();

                        return;
                    }
                    catch
                    {
                        await Task.Delay(1000);
                    }
                }
            });
            
            OnPropertyChanged(nameof(AllResults));
            OnPropertyChanged(nameof(UpdateResults));
            OnPropertyChanged(nameof(LastResult));
            OnPropertyChanged(nameof(LastUpdateResult));
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
                    
                    return ("success", doc.QuerySelector(Selectors)?.OuterHtml);
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

        public ScrapingRule Duplicate()
        {
            // シリアライズ・デシリアライズでオブジェクトのコピーを作ったつもり
            return JsonConvert.DeserializeObject<ScrapingRule>((JsonConvert.SerializeObject(this)));
        }
    }
}
