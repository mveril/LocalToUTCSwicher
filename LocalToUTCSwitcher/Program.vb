Imports Microsoft.Win32
Imports System.ServiceProcess
Imports System.Globalization
Imports System.Resources
Imports System.Reflection

Class Program
    Shared Sub Main(args As String())
        Dim isuniversal As Boolean
        'Dim culture = CultureInfo.GetCultureInfoByIetfLanguageTag("de-DE")
        'With Threading.Thread.CurrentThread
        '.CurrentUICulture = culture
        '.CurrentCulture = culture
        'End With
        Dim tzi = Registry.LocalMachine.OpenSubKey("SYSTEM\CurrentControlSet\Control\TimeZoneInformation", True)
        Dim valname = "RealTimeIsUniversal"
        isuniversal = Convert.ToBoolean(tzi.GetValue(valname, 0))
        Console.WriteLine(locale.ClockSet, TimeSettingsString(isuniversal))
        Dim hasrep As Boolean
        Do
            Console.WriteLine(locale.askset, TimeSettingsString(Not isuniversal), locale.Y, locale.N)
            Dim strrep = Console.ReadLine()
            hasrep = {locale.Y, locale.N}.Contains(strrep, StringComparer.CurrentCultureIgnoreCase)
            If hasrep Then
                If strrep.ToUpper = locale.Y Then
                    Dim ts As New ServiceController("W32Time")
                    If ts.CanStop Then
                        Console.WriteLine(locale.srvstop, ts.DisplayName)
                        ts.Stop()
                        ts.WaitForStatus(ServiceControllerStatus.Stopped)
                    End If
                    Console.WriteLine(locale.appmod)
                    isuniversal = Not isuniversal
                    tzi.SetValue(valname, Convert.ToInt32(isuniversal), RegistryValueKind.DWord)
                    ts.Refresh()
                    If ts.Status <> ServiceControllerStatus.Running And ts.Status <> ServiceControllerStatus.StartPending Then
                        Console.WriteLine(locale.srvreboot, ts.DisplayName)
                        ts.Start()
                    End If
                    If ts.Status <> ServiceControllerStatus.Running Then
                        ts.WaitForStatus(ServiceControllerStatus.Running)
                    End If
                    ts.Refresh()
                    Console.WriteLine(locale.gethour)
                    Dim proc = Process.Start(New ProcessStartInfo("w32tm.exe ", "/resync") With {.UseShellExecute = False, .RedirectStandardOutput = True, .RedirectStandardInput = True})
                    proc.WaitForExit()
                    With DateTime.Now.TimeOfDay
                        Console.WriteLine(New TimeSpan(.Hours, .Minutes, .Seconds))
                    End With
                End If
            End If
        Loop Until hasrep
        tzi.Dispose()
        Console.WriteLine(locale.presstocont)
        Console.ReadKey()
    End Sub
    Shared Function TimeSettingsString(isuniversal As Boolean) As String
        Return String.Format(locale.htype, If(isuniversal, locale.UTC, locale.Locale))
    End Function
End Class