using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using ElectronNET.API;
using ElectronNET.API.Entities;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using turning_electron.Services;

namespace turning_electron
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddRazorPages();
            services.AddServerSideBlazor();

            services.AddSingleton<MapsService>();
            services.AddSingleton<NConfigGenerator>();
            services.AddSingleton<UserConfiguration>();
            services.AddSingleton<XService>();
        }

        bool nginxLoaded = false;
        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Error");
            }



            var nconfigGenerator = app.ApplicationServices.GetService<NConfigGenerator>();
            
            var mapsservice = app.ApplicationServices.GetService<MapsService>();
            mapsservice.Updated += (s, e) =>
            {
                nconfigGenerator.Apply(mapsservice
                    .GetList()
                    .Where(o => o.Enabled)
                    .ToArray());
            };

            ///启动时加载配置
            mapsservice.Load();



            var userconfig = app.ApplicationServices.GetService<UserConfiguration>();
            userconfig.Load();
            if (userconfig.StartNginxOnBoot)
            {
                NginxService.Start();
            }


            ///绑定配置更新时间
            nconfigGenerator.ModifyPatched += (s, e) =>
            {
                if (nginxLoaded)
                {
                    NginxService.Reload();
                }
                else
                {
                    NginxService.Start();
                    nginxLoaded = true;
                }
            };

            app.UseStaticFiles();

            app.UseRouting();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapBlazorHub();
                endpoints.MapFallbackToPage("/_Host");
            });

            var xservice = app.ApplicationServices.GetService<XService>();
            xservice.Run();

        }
    }
}
