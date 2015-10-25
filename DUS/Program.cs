using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.IO;

namespace DBNUpdater.dediclient
{
    class Program
    {

        static public void Main()
        {
            Environment.CurrentDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);//Application.StartupPath;
            var updater = new Updater("http://www.repziw4.net/content/dediiw4/", "", false);
            updater.StatusChanged += updater_StatusChanged;
            updater.Start(new string[] { "RepZ-IW4" });
        }
        static Stopwatch speedWatch = new Stopwatch();

        static void updater_StatusChanged(object sender, StatusChangedEventArgs e)
        {
            if (e.Type == StatusChangedEnum.Start)
            {
                Console.WriteLine("Repz Dedicated Server Updater - Linux");
                Console.WriteLine("Modified By Freedom Designs");
                Console.WriteLine("Starting updater, wanted caches: RepZ-IW4");
            }
            else if (e.Type == StatusChangedEnum.Finish)
            {
                Console.WriteLine("Updater finished.");
                Bootstrap();
                Console.WriteLine("Press Any Key To Exit...");
                Console.ReadKey();
            }
            else if (e.Type == StatusChangedEnum.Fail)
            {
                Console.WriteLine("Updater failed. Error given: \n" + ((Exception)e.data).ToString());
            }
            else if (e.Type == StatusChangedEnum.DownloadsStart)
            {
                Console.WriteLine("Starting downloads");
            }
            else if (e.Type == StatusChangedEnum.CacheDownloading)
            {
                Console.WriteLine("Downloading update definitions");
            }
            else if (e.Type == StatusChangedEnum.FileDownloading)
            {
                var obj = (object[])e.data;
                Console.Write(String.Format("\rDownloading - {0}, {1} MB's.", 
                                            obj[0],
                                            (Convert.ToInt32(obj[1]) / 1024d / 1024d).ToString("0.00")));
            }
            else if (e.Type == StatusChangedEnum.FileDecompressStart)
            {
                Console.Write(String.Format("\rDecompressing - {0}                   ", e.data));
            }
            else if (e.Type == StatusChangedEnum.FileVerifyStart)
            {
                Console.Write(String.Format("\rVerifying - {0}                   ", e.data));
            }
            else if (e.Type == StatusChangedEnum.FileVerifyFinish)
            {
                Console.Write(String.Format("\rComplete - {0}                   ", e.data));
            }
            else if (e.Type == StatusChangedEnum.NoUpdateNeeded)
            {
                Console.WriteLine("No Update Needed");
            }
            else if (e.Type == StatusChangedEnum.CacheDownloaded)
            {
                var obj = (object[])e.data;
                var name = (String)obj[0];
                if (name == "caches.xml")
                {
                    var a = (Dictionary<string, int>)obj[1];
                    foreach (var b in a)
                    {
                        Console.WriteLine("{0}: Version {1}", b.Key, b.Value);
                    }
                }
            }
            else if (e.Type == StatusChangedEnum.FileStart)
            {
                Console.WriteLine();
            }
        }
        static void Bootstrap()
        {
            try
            {
                var fn = Path.Combine(Environment.CurrentDirectory, Path.Combine("bootstrap", Path.GetFileName(Assembly.GetExecutingAssembly().Location))); //problem?
                var tn = Path.GetFileName(Assembly.GetExecutingAssembly().Location);
                if (File.Exists(fn))
                {
                    if (Utilities.GetFileSHA1(fn) != Utilities.GetFileSHA1(tn))
                    {
                        //files don't match, not good!
                        if (Environment.OSVersion.Platform == PlatformID.Unix ||
                            Environment.OSVersion.Platform == PlatformID.MacOSX)
                        {
                            //can just overwrite files, simple (linux <3)
                            Log.Info("Copying files for updater updating. (linux)");
                            foreach (var file in Directory.GetFiles(Path.Combine(Environment.CurrentDirectory, "bootstrap")))
                            {
                                File.Move(file, Path.GetFileName(file));
                            }
                        }
                        else
                        {
                            //assume windows with locking filesystem (windows </3)
                            var us = Path.Combine(Environment.CurrentDirectory, "update.cmd");
                            if (File.Exists(us))
                            {
                                Log.Info("Starting update.cmd for updater updating.");
                                var psi = new ProcessStartInfo(us, "Updater.exe"); //start the updater again
                                psi.WorkingDirectory = Environment.CurrentDirectory;

                                Process.Start(psi);
                            }
                        }
                    } 
                }
            }
            catch(Exception e)
            {
                Log.Error("Bootstrap error: " + e.ToString());
            }
        }
    }
}