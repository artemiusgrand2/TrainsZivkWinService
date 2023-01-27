using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Threading.Tasks;
using System.Collections;
using System.ComponentModel;
using System.ServiceModel;
using System.ServiceProcess;
using System.Reflection;
using System.Configuration;
using System.Configuration.Install;
using TrainsZivkWinService.Enums;

namespace TrainsZivkWinService
{
    class Program
    {

        public static List<string> Stations { get; private set; } = new List<string>();

        public static List<string> Trains { get; private set; } = new List<string>();

        /// <summary>
        /// интервалы пассажирских поездов
        /// </summary>
        public static IList<TrainInterval> TrainsPass { get; private set; } = new List<TrainInterval>() { new TrainInterval() { Start = 1, End = 998 }, new TrainInterval() { Start = 6001, End = 7628 } };

        static void Main(string[] args)
        {
            //разбираю командную строку
            var configFile = Assembly.GetExecutingAssembly().Location;
            configFile = configFile.Replace(".exe", ".txt");
            for (int i = 0; i < args.Length; i++)
            {
                if (args[i].IndexOf("-c=") != -1)
                {
                    configFile = args[i].Replace("-c=", string.Empty);
                    continue;
                }
                if (string.Compare(args[i], "-install", true) == 0
                  || string.Compare(args[i], "/install", true) == 0)
                {
                    InstallService();
                    return;
                }
                else if (string.Compare(args[i], "-uninstall", true) == 0
                   || string.Compare(args[i], "/uninstall", true) == 0)
                {
                    UninstallService();
                    return;
                }
            }
            //
            HelpFunctions.ReadConfig(configFile);
             ServiceBase.Run(new ZivkService());

            //var data = new OperationDataBase();
            //var end = DateTime.Now;
            //var start = end.AddHours(-Math.Abs(int.Parse(ConfigurationManager.AppSettings["countHour"])));
            //data.GetLastTrainEvents(start, end, ConfigurationManager.ConnectionStrings["zivk"].ConnectionString);
        }

        private static void InstallService()
        {
            AssemblyInstaller installer = new AssemblyInstaller();
            IDictionary savedState = new Hashtable();
            string[] commandLine = { "/logFile=zivkService.log" };

            installer.Assembly = Assembly.GetExecutingAssembly();
            installer.CommandLine = commandLine;
            installer.UseNewContext = true;
            try
            {
                installer.Install(savedState);
            }
            catch
            {
                installer.Rollback(savedState);
                return;
            }
            finally
            {
                //Console.WriteLine("Нажмите любую клавишу для выхода...");
                //Console.ReadLine();
            }
        }

        private static void UninstallService()
        {
            AssemblyInstaller installer = new AssemblyInstaller();
            IDictionary savedState = new Hashtable();
            string[] commandLine = { "/logFile=zivkService.log" };

            installer.Assembly = Assembly.GetCallingAssembly();
            installer.UseNewContext = true;
            installer.CommandLine = commandLine;
            try
            {
                installer.Uninstall(savedState);
            }
            catch
            {
                return;
            }
            finally
            {
                //Console.WriteLine("Нажмите любую клавишу для выхода...");
                //Console.ReadLine();
            }
        }
    }   
}
