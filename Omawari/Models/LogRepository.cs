using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Omawari.Models
{
    using Newtonsoft.Json;
    using System.Collections.ObjectModel;
    using System.IO;
    using Omawari.Utilities;

    public class LogRepository : BindableBase
    {
        public static LogRepository Instance { get; } = new LogRepository();

        private FileSystemWatcher watcher;

        public ObservableCollection<ScrapingResult> Items { get; private set; }

        private LogRepository()
        {
            Items = Directory
                .EnumerateFiles(App.Instance.DataFolder, "*.json", SearchOption.AllDirectories)
                .Where(_ => !_.EndsWith("scrapers.json"))
                .Where(_ => !_.EndsWith("settings.json"))
                .Select(async _ => await ActionHelper.TryUntilCompleteAsync(() => File.ReadAllText(_)))
                .Select(_ => JsonConvert.DeserializeObject<ScrapingResult>(_.Result))
                .OrderByDescending(_ => _.CompletedAt)
                .ToObservableCollection();

            watcher = new FileSystemWatcher(App.Instance.DataFolder)
            {
                NotifyFilter = NotifyFilters.FileName,
                IncludeSubdirectories = true,
                EnableRaisingEvents = true
            };

            watcher.Created += async (sender, args) =>
            {
                if (args.FullPath.EndsWith("scrapers.json")) return;
                if (args.FullPath.EndsWith("settings.json")) return;

                var result = await ActionHelper.TryUntilCompleteAsync(() => Load(args.FullPath));
                if (result == null) return;
                await ActionHelper.TryUntilCompleteAsync(() => { if (!Items.Contains(result)) Items.Insert(0, result); });
            };

            watcher.Deleted += async (sender, args) =>
            {
                if (args.FullPath.EndsWith("scrapers.json")) return;
                if (args.FullPath.EndsWith("settings.json")) return;

                var id = Path.GetFileNameWithoutExtension(args.FullPath);
                var item = Items.FirstOrDefault(_ => _.Id.ToString() == id);
                await ActionHelper.TryUntilCompleteAsync(() => { if (item != null) Items.Remove(item); });
            };
        }

        private ScrapingResult Load(string path)
        {
            var json = File.ReadAllText(path);
            var result = JsonConvert.DeserializeObject<ScrapingResult>(json);
            return result;
        }
    }
}
