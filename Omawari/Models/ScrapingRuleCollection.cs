using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Omawari.Models
{
    public class ScrapingRuleCollection : ObservableCollection<ScrapingRule>
    {
        private ScrapingRuleCollection() { }

        [JsonIgnore]
        private string Path { get; set; }

        public static ScrapingRuleCollection Load(string path)
        {
            try
            {
                var json = File.ReadAllText(path);
                var temp = JsonConvert.DeserializeObject<ObservableCollection<ScrapingRule>>(json);

                // 一発でシリアライズしてくれないから自分でやる
                var collection = new ScrapingRuleCollection() { Path = path, };
                foreach (var item in temp) collection.Add(item);

                return collection;
            }
            catch
            {
                return new ScrapingRuleCollection() { Path = path, };
            }
        }

        public void Save()
        {
            try
            {
                var json = JsonConvert.SerializeObject(this);
                File.WriteAllText(Path, json);
            }
            catch (Exception e)
            {
                Debug.WriteLine(e.Message);
            }
        }

        public void Exchange(ScrapingRule item1, ScrapingRule item2)
        {
            this[Items.IndexOf(item1)] = item2;
        }
    }
}
