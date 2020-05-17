using ElectronNET.API;
using ElectronNET.API.Entities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace turning_electron.Services
{
    public class XService
    {
        BrowserWindow mainWindow;

        public void Run()
        {
            Task.Run(async () => {
                var options = new BrowserWindowOptions()
                {
                    AutoHideMenuBar = true,
                    TitleBarStyle = TitleBarStyle.customButtonsOnHover,
                    Icon = Path.Combine(Directory.GetCurrentDirectory(), "icon.ico"),
                    Width = 1366,
                    Height = 768
                };

                mainWindow = await Electron.WindowManager.CreateWindowAsync(options);
                mainWindow.OnClose += () =>
                {
                    NginxService.Stop();
                };
            });
        }

        public async Task ExportConfig()
        {

            var saveoptions = new SaveDialogOptions()
            {
                Title = "导出配置",
                Message = "请选择文件保存位置",
                Filters = new FileFilter[]
                {
                    new FileFilter()
                    {
                        Extensions = new string[]{ "json"},
                        Name = "JSON配置文件"
                    }
                },
                DefaultPath = "maps.json"
            };
            var path = await Electron.Dialog.ShowSaveDialogAsync(mainWindow, saveoptions);
            if (!string.IsNullOrWhiteSpace(path))
            {
                try
                {
                    File.Copy(Path.Combine(Directory.GetCurrentDirectory(), "maps.json"),
                    path);
                }
                catch (Exception)
                {
                }
            }
        }


        public async Task ImportConfig()
        {
            var openoptions = new OpenDialogOptions()
            {
                Title = "导入配置",
                Message = "请选择配置文件",
                Filters = new FileFilter[]
                {
                    new FileFilter()
                    {
                        Extensions = new string[]{ "json"},
                        Name = "JSON配置文件"
                    }
                },
            };
            var paths = await Electron.Dialog.ShowOpenDialogAsync(mainWindow, openoptions);
            if (paths != null && paths.Length > 0 && !string.IsNullOrWhiteSpace(paths[0]))
            {
                try
                {
                    var local = Path.Combine(Directory.GetCurrentDirectory(), "maps.json");
                    var remote = paths[0];

                    if (File.Exists(local))
                    {
                        var msgoptions = new MessageBoxOptions("是否覆盖当前的配置文件？")
                        {
                            Buttons = new string[] { "是", "否" },
                        };

                        var result = await Electron.Dialog.ShowMessageBoxAsync(mainWindow, msgoptions);
                        if (result.Response == 0)
                        {
                            File.Delete(local);
                        }
                        else
                        {
                            return;
                        }
                    }

                    File.Copy(remote, local);
                }
                catch (Exception)
                {
                }
            }
        }


    }
}
