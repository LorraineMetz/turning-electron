using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace turning_electron.Services
{
    public class NginxService
    {
        public static void Start()
        {
            try
            {
                Task.Run(() =>
                {
                    string dir = Path.Combine(Directory.GetCurrentDirectory(), "nginx");

                    new Process()
                    {
                        StartInfo = new ProcessStartInfo()
                        {
                            WorkingDirectory = dir,
                            FileName = Path.Combine(dir, "nginx.exe"),
                        }
                    }.Start();

                });
            }
            catch (Exception)
            {
            }
        }

        public static void Reload()
        {
            try
            {
                string dir = Path.Combine(Directory.GetCurrentDirectory(), "nginx");

                new Process()
                {
                    StartInfo = new ProcessStartInfo()
                    {
                        WorkingDirectory = dir,
                        FileName = Path.Combine(dir, "nginx.exe"),
                        Arguments = "-s reload"
                    }
                }.Start();
            }
            catch (Exception)
            {

            }

        }

        public static void Stop()
        {

            try
            {
                string dir = Path.Combine(Directory.GetCurrentDirectory(), "nginx");

                new Process()
                {
                    StartInfo = new ProcessStartInfo()
                    {
                        WorkingDirectory = dir,
                        FileName = Path.Combine(dir, "nginx.exe"),
                        Arguments = "-s stop"
                    }
                }.Start();

            }
            catch (Exception)
            {

            }
        }
    }
}
