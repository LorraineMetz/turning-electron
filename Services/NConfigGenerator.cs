using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace turning_electron.Services
{
    public class NConfigGenerator
    {
        /// <summary>
        /// 应用
        /// </summary>
        /// <param name="validPairs"></param>
        public async void Apply(MapPair[] validPairs)
        {
            var confContents = new List<string>();

            var hostexist = new List<string>(await File.ReadAllLinesAsync(HostFile));
            hostexist = hostexist
                .Where(ln => !(ln.Contains(HostTag)))
                .ToList();

            hostexist.Add(HostTag + " Start ---------");

            foreach (var item in validPairs)
            {
                UrlAddressParser inParse = item.In;
                UrlAddressParser outParse = item.Out;

                #region Host文件
                hostexist.Add(string.Format("127.0.0.1  {0} {1} [{2} -> {3}]",
                    inParse.Host,
                    HostTag,
                    inParse.Url,
                    outParse.Url));
                #endregion

                #region Nginx.Conf
                var template = Properties.Resources.http;
                template = template.Replace("%端口%", inParse.Port.ToString());
                template = template.Replace("%主机%", inParse.Host);
                template = template.Replace("%终点%", outParse.Url.Trim('/') + "/");
                template = template.Replace("%自定义%", item.UserSetting ?? "");

                confContents.Add("");
                confContents.Add(template);
                #endregion
            }
            hostexist.Add(HostTag + " End ---------");

            Hosts = string.Join(Environment.NewLine, hostexist);

            NginxConf = Properties.Resources.nginx.Replace("%占位符%", string.Join(Environment.NewLine, confContents));


            #region 更新
            await File.WriteAllTextAsync(ConfFile, NginxConf);

            try
            {
                await File.WriteAllTextAsync(HostFile, Hosts);
            }
            catch (Exception)
            {
            }

            #endregion

            if (ModifyPatched != null)
            {
                ModifyPatched(this, null);
            }
        }

        public EventHandler ModifyPatched;

        const string HostTag = "# By Turning v1.0";

        public string NginxConf { get; set; }

        public string Hosts { get; set; }

        string _hostfile;
        string HostFile
        {
            get
            {
                if (string.IsNullOrWhiteSpace(_hostfile))
                {

                    if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                    {
                        _hostfile = @"C:\Windows\System32\drivers\etc\hosts";
                    }
                }
                return _hostfile;
            }
        }

        string _conffile;
        string ConfFile
        {
            get
            {
                if (string.IsNullOrWhiteSpace(_conffile))
                {
                    _conffile = Path.Combine(Directory.GetCurrentDirectory(),
                        "nginx",
                        "conf",
                        "nginx.conf");
                }
                return _conffile;
            }
        }

    }
}
