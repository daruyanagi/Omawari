using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Omawari.Models
{
    public class GlobalSettings : BindableBase
    {
        private int waitTimeForChangingToPending = 5;
        private int defaultInterval = 3 * 60;
        private bool autoStart = false;

        public int WaitTimeForChangingToPending
        {
            get { return waitTimeForChangingToPending; }
            set { SetProperty(ref waitTimeForChangingToPending, value); }
        }

        public int DefaultInterval
        {
            get { return defaultInterval; }
            set { SetProperty(ref defaultInterval, value); }
        }

        public bool AutoStart
        {
            get { return autoStart; }
            set { SetProperty(ref autoStart, value); }
        }

        private GlobalSettings() { }

        [JsonIgnore]
        private string Path { get; set; }

        public static GlobalSettings Load(string path)
        {
            try
            {
                var json = File.ReadAllText(path);
                var temp = JsonConvert.DeserializeObject<GlobalSettings>(json);

                temp.Path = path;

                return temp;
            }
            catch
            {
                return new GlobalSettings() { Path = path, };
            }
        }

        public void Save()
        {
            try
            {
                var json = JsonConvert.SerializeObject(this, Formatting.Indented);
                File.WriteAllText(Path, json);
            }
            catch (Exception e)
            {
                Debug.WriteLine(e.Message);
            }
        }
    }
}
