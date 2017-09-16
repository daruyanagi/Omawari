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
    public class ScraperCollection : ObservableCollection<Scraper>
    {
        private ScraperCollection() { }

        [JsonIgnore]
        private string Path { get; set; }

        public static ScraperCollection Load(string path)
        {
            try
            {
                var json = File.ReadAllText(path);
                var temp = JsonConvert.DeserializeObject<ObservableCollection<Scraper>>(json);

                // 一発でシリアライズしてくれないから自分でやる
                var collection = new ScraperCollection() { Path = path, };
                foreach (var item in temp) collection.Add(item);

                return collection;
            }
            catch
            {
                return new ScraperCollection() { Path = path, };
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

        public void Exchange(Scraper item1, Scraper item2)
        {
            this[Items.IndexOf(item1)] = item2;
        }
    }
}
