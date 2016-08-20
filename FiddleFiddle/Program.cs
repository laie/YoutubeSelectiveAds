using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Fiddler;

namespace FiddleFiddle
{
    class Program
    {
        public static class Certification
        {
            private static string ReadText(string filename)
            {
                try
                {
                    using (var file = File.OpenText(filename))
                        return file.ReadToEnd();
                }
                catch (Exception)
                {
                }
                return null;
            }
            public static bool InstallCertificate()
            {
                var certpref = ReadText("cert.str");
                if (certpref != null) FiddlerApplication.Prefs.SetStringPref("fiddler.certmaker.bc.cert", certpref);
                var keypref = ReadText("certkey.str");
                if (keypref != null) FiddlerApplication.Prefs.SetStringPref("fiddler.certmaker.bc.key", keypref);

                if (!CertMaker.rootCertExists())
                {
                    Console.WriteLine("no cert found.. let's make one!");
                    if (!CertMaker.createRootCert())
                        return false;

                    if (!CertMaker.trustRootCert())
                        return false;

                    using (var file = File.OpenWrite("cert.str"))
                    using (var writer = new StreamWriter(file))
                        writer.Write(FiddlerApplication.Prefs.GetStringPref("fiddler.certmaker.bc.cert", null));
                    using (var file = File.OpenWrite("certkey.str"))
                    using (var writer = new StreamWriter(file))
                        writer.Write(FiddlerApplication.Prefs.GetStringPref("fiddler.certmaker.bc.key", null));
                }

                return true;
            }
            public static bool UninstallCertificate()
            {
                if (CertMaker.rootCertExists())
                {
                    if (!CertMaker.removeFiddlerGeneratedCerts(true))
                        return false;
                }
                return true;
            }
        }
        public static class ConsoleEvents
        {
            static ConsoleEventDelegate handler;   // Keeps it from getting garbage collected
                                                   // Pinvoke
            public delegate bool ConsoleEventDelegate(CtrlTypes eventType);
            [DllImport("kernel32.dll", SetLastError = true)]
            private static extern bool SetConsoleCtrlHandler(ConsoleEventDelegate callback, bool add);
            public enum CtrlTypes
            {
                CTRL_C_EVENT = 0,
                CTRL_BREAK_EVENT,
                CTRL_CLOSE_EVENT,
                CTRL_LOGOFF_EVENT = 5,
                CTRL_SHUTDOWN_EVENT
            }
            public static void StartMonitor(ConsoleEventDelegate eventHandler)
            {
                handler = eventHandler;
                SetConsoleCtrlHandler(eventHandler, true);
            }
        }

        private static void FiddlerApplication_AfterSocketAccept(object sender, ConnectionEventArgs e)
        {
            var ep = (e.Connection.RemoteEndPoint as System.Net.IPEndPoint);
            if (ep.AddressFamily != System.Net.Sockets.AddressFamily.InterNetworkV6)
            {
                Console.WriteLine("unexpected non Ipv6 connection!");
                e.Connection.Close();
                return;
            }
            if (ep.Address.IsIPv4MappedToIPv6)
            {
                if (ep.Address.MapToIPv4().Equals(System.Net.IPAddress.Loopback))
                {
                    // allowed
                    return;
                }
            }
            else
            {
                if (ep.Address.Equals(System.Net.IPAddress.IPv6Loopback))
                {
                    // allowed
                    return;
                }
            }
            Console.WriteLine("remote ep " + ep.ToString() + " isn't allowed to connect!");
            e.Connection.Close();
        }
        static void Main(string[] args)
        {
            ConsoleEvents.StartMonitor(eventType => { FiddlerApplication.Shutdown(); return false; });
            Certification.InstallCertificate();

            FiddlerApplication.BeforeRequest += delegate (Fiddler.Session oSession)
            {
                oSession.bBufferResponse = true;
            };
            FiddlerApplication.BeforeResponse += delegate (Fiddler.Session oSession)
            {
                if (oSession.url.Contains("yts/jsbin/player") && oSession.url.Contains("base.js"))
                {
                    oSession.utilDecodeResponse();
                    String originbody = System.Text.Encoding.UTF8.GetString(oSession.responseBodyBytes);
                    var replacedBody = Regex.Replace(originbody, @"([a-zA-Z_]*)\(this,""start"",\!([0-1]),\!([0-1])\)", @"$1(this,""skip"",!$2,!$3)");
                    bool replaced = !originbody.SequenceEqual(replacedBody);
                    oSession.utilSetResponseBody(replacedBody);
                    Console.WriteLine("gotcha: " + oSession.fullUrl);
                    if (replaced)
                        Console.WriteLine("modified the functionality of youtube!");
                    else Console.WriteLine("failed to modify the functionality. try up-to-date version.");
                }
            };
            //FiddlerApplication.AfterSessionComplete += FiddlerApplication_AfterSessionComplete;
            FiddlerApplication.AfterSocketAccept += FiddlerApplication_AfterSocketAccept;
            FiddlerApplication.Startup(8890, true, true, true);


            Console.WriteLine("I am looking for the ads...! Don't see any messages after opening Youtube? Try delete your cache files.");
            for (;;) Thread.Sleep(100000);
        }

    }
}
