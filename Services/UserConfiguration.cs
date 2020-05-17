using Microsoft.Win32;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace turning_electron.Services
{
    public class UserConfiguration
    {
        ConcurrentDictionary<string, string> configs = new ConcurrentDictionary<string, string>();

        public async void Load()
        {

            if (File.Exists("config.json"))
            {
                var data = await File.ReadAllTextAsync("config.json");
                configs = JsonSerializer.Deserialize<ConcurrentDictionary<string, string>>(data);
            }
            else
            {
                configs = new ConcurrentDictionary<string, string>();
            }


            if (Updated != null)
            {
                Updated(this, null);
            }
        }


        public async void Save()
        {
            var data = JsonSerializer.Serialize(configs);
            await File.WriteAllTextAsync("config.json", data);

            if (Updated != null)
            {
                Updated(this, null);
            }
        }


        public EventHandler Updated;

        public bool StartNginxOnBoot
        {
            get
            {
                return configs.ContainsKey(nameof(StartNginxOnBoot)) && 
                    configs[nameof(StartNginxOnBoot)] == true.ToString();
            }
            set
            {
                configs[nameof(StartNginxOnBoot)] = value.ToString();
                Save();
            }
        }

    }
}
