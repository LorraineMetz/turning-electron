using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace turning_electron.Services
{
    public class MapsService
    {
        List<MapPair> maplist = new List<MapPair>();

        public IJSRuntime jsruntime;

        public MapsService(IJSRuntime _jsruntime)
        {
            jsruntime = _jsruntime;
        }

        public EventHandler Updated { get; set; }


        public async void Load()
        {
            if (File.Exists("maps.json"))
            {
                var data = await File.ReadAllTextAsync("maps.json");
                maplist = JsonSerializer.Deserialize<List<MapPair>>(data);
            }
            else
            {
                maplist = new List<MapPair>();
            }


            if (Updated != null)
            {
                Updated(this, null);
            }
        }

        public async void Save()
        {
            var data = JsonSerializer.Serialize(maplist);
            await File.WriteAllTextAsync("maps.json", data);

            if (Updated != null)
            {
                Updated(this, null);
            }
        }

        /// <summary>
        /// 新增一组映射
        /// </summary>
        /// <param name="in"></param>
        /// <param name="out">全局唯一的</param>
        public async Task<bool> Add(string @in, string @out)
        {
            UrlAddressParser inParser = @in;
            UrlAddressParser outParser = @out;


            if (inParser.IsIp)
            {
                throw new Exception("输入地址不能为IP");
            }

            if (maplist.Where(a => a.In == inParser.Url).Count() > 0)
            {
                var confirm = await jsruntime.InvokeAsync<bool>("confirm", new object[] {
                    "已存在相同的输入地址，是否禁用旧的映射？"
                });
                if (confirm)
                {
                    maplist.Where(a => a.In == inParser.Url && a.Enabled).First().Enabled = false;
                }
                else
                {
                    throw new Exception("已存在相同的输入地址");
                }
            }


            var pair = new MapPair()
            {
                In = inParser.Url,
                Out = outParser.Url,
                Id = Guid.NewGuid().ToString(),
                Enabled = true,
            };

            maplist.Add(pair);

            Save();

            return true;
        }

        public void Remove(string id)
        {
            var p = maplist.FirstOrDefault(o => o.Id == id);
            if (p != null)
            {
                maplist.Remove(p);

                Save();
            }
        }

        public void Update(string id, bool ison)
        {
            var p = maplist.FirstOrDefault(o => o.Id == id);
            if (ison)
            {
                var @in = p.In;
                var exists = maplist.Where(o => o.Id != id && o.In == @in && o.Enabled);
                foreach (var item in exists)
                {
                    item.Enabled = false;
                }
            }

            p.Enabled = ison;
            Save();
        }

        public void Update(MapPair pair)
        {
            UrlAddressParser inParser = pair.In;
            if (inParser.IsIp)
            {
                throw new Exception("输入地址不能为IP");
            }
            var p = maplist.FirstOrDefault(o => o.Id == pair.Id);
            p.In = pair.In;
            p.Out = pair.Out;
            p.Enabled = pair.Enabled;
            p.UserSetting = pair.UserSetting;
            Save();
        }

        public MapPair[] GetList()
        {
            return maplist.ToArray();
        }
    }

    public class MapPair
    {
        public string Id { get; set; }

        public string In { get; set; }

        public string Out { get; set; }

        public bool Enabled { get; set; }

        public string UserSetting { get; set; }
    }



    public class UrlAddressParser
    {
        public static implicit operator UrlAddressParser(string url)
        {
            return new UrlAddressParser(url);
        }

        static Regex ipRegex = new Regex("^([0-9]{1,3}\\.){3}[0-9]{1,3}$");

        public UrlAddressParser(string url)
        {
            string[] list = url.Split(new string[] { ":" }, StringSplitOptions.RemoveEmptyEntries);
            if (list.Length < 2)
            {
                throw new ArgumentException(nameof(url)+"不能为空");
            }

            #region 协议
            string protocol = list[0].ToLower();
            if (protocol == "http")
            {
                this.Protocol = Protocol.Http;
                this.Port = 80;
            }
            else if (protocol == "https")
            {
                this.Protocol = Protocol.Https;
                this.Port = 443;
            }
            else
            {
                throw new NotSupportedException("不支持的协议");
            }
            #endregion

            if (list[1].StartsWith("//"))
            {
                this.Host = list[1].Trim('/');
                this.IsIp = ipRegex.IsMatch(this.Host);
            }
            if (list.Length == 3)
            {
                if (int.TryParse(list[2].Trim('/'), out int port) && port >= 0 && port <= 65535)
                {
                    this.Port = port;
                }
                else
                {
                    throw new NotSupportedException("端口号错误");
                }
            }

            this.Url = this.Protocol.ToString().ToLower() + "://" + this.Host + ":" + this.Port;
        }

        public Protocol Protocol { get; private set; }
        public string Host { get; private set; }
        public int Port { get; private set; }

        public bool IsIp { get;private set; }

        public string Url { get;private set; }
    }

    public enum Protocol
    {
        Http  = 80,
        Https = 443,
    }
}
