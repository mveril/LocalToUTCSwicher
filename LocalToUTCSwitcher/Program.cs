using Microsoft.Win32;
using System;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using LocalToUTCSwitcher.locales;

namespace LocalToUTCSwitcher
{
    class Program
    {
        public static void Main(string[] args)
        {   
            bool isuniversal;
            var tzi = Registry.LocalMachine.OpenSubKey(@"SYSTEM\CurrentControlSet\Control\TimeZoneInformation", true);
            string valname = "RealTimeIsUniversal";
            isuniversal = Convert.ToBoolean(tzi.GetValue(valname, 0));
            Console.WriteLine(locale.ClockSet, TimeSettingsString(isuniversal));
            bool hasrep;
            do
            {
                Console.WriteLine(locale.askset, TimeSettingsString(!isuniversal), locale.Y, locale.N);
                string strrep = Console.ReadLine();
                hasrep = new[] { locale.Y, locale.N }.Contains(strrep, StringComparer.CurrentCultureIgnoreCase);
                if (hasrep)
                {
                    if (strrep.ToUpper() == locale.Y)
                    {
                        var ts = new ServiceController("W32Time");
                        if (ts.CanStop)
                        {
                            Console.WriteLine(locale.srvstop, ts.DisplayName);
                            ts.Stop();
                            ts.WaitForStatus(ServiceControllerStatus.Stopped);
                        }

                        Console.WriteLine(locale.appmod);
                        isuniversal = !isuniversal;
                        tzi.SetValue(valname, Convert.ToInt32(isuniversal), RegistryValueKind.DWord);
                        ts.Refresh();
                        if (ts.Status != ServiceControllerStatus.Running & ts.Status != ServiceControllerStatus.StartPending)
                        {
                            Console.WriteLine(locale.srvreboot, ts.DisplayName);
                            ts.Start();
                        }

                        if (ts.Status != ServiceControllerStatus.Running)
                            ts.WaitForStatus(ServiceControllerStatus.Running);
                        ts.Refresh();
                        Console.WriteLine(locale.gethour);
                        var proc = Process.Start(new ProcessStartInfo("w32tm.exe ", "/resync") { UseShellExecute = false, RedirectStandardOutput = true, RedirectStandardInput = true });
                        proc.WaitForExit();
                        Console.WriteLine(new TimeSpan(DateTime.Now.TimeOfDay.Hours, DateTime.Now.TimeOfDay.Minutes, DateTime.Now.TimeOfDay.Seconds));
                    }
                }
            }
            while (!hasrep);
            tzi.Dispose();
            Console.WriteLine(locale.presstocont);
            Console.ReadKey();
        }
        public static string TimeSettingsString(bool isuniversal)
        {
            return string.Format(locale.htype, isuniversal ? locale.UTC : locale.Locale);
        }
    }
}
