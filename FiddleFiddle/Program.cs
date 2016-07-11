using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
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
                if (certpref!=null) FiddlerApplication.Prefs.SetStringPref("fiddler.certmaker.bc.cert", certpref);
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
            public delegate bool ConsoleEventDelegate(int eventType);
            [DllImport("kernel32.dll", SetLastError = true)]
            private static extern bool SetConsoleCtrlHandler(ConsoleEventDelegate callback, bool add);

            public static void StartMonitor(ConsoleEventDelegate eventHandler)
            {
                handler = eventHandler;
                SetConsoleCtrlHandler(eventHandler, true);
            }
        }

        static void Main(string[] args)
        {
            ConsoleEvents.StartMonitor(eventType => { if (eventType == 2) FiddlerApplication.Shutdown(); return false; });
            Certification.InstallCertificate();

            FiddlerApplication.BeforeRequest += delegate (Fiddler.Session oSession)
            {
                oSession.bBufferResponse = true;
            };
            FiddlerApplication.BeforeResponse += delegate (Fiddler.Session oSession)
            {
                if (oSession.url.Contains("/jsbin/player") && oSession.url.Contains("base.js"))
                {
                    oSession.utilDecodeResponse();
                    String oBody = System.Text.Encoding.UTF8.GetString(oSession.responseBodyBytes);
                    {
                        oBody = oBody.Replace("_(this,\"start\",!0,!0)", "_(this,\"skip\",!0,!0)");
                    }
                    oSession.utilSetResponseBody(oBody);
                    Console.WriteLine("gotcha: " + oSession.fullUrl);
                }
            };
            //FiddlerApplication.AfterSessionComplete += FiddlerApplication_AfterSessionComplete;
            FiddlerApplication.Startup(8888, true, true, true);

            Console.WriteLine("I am looking for the ads...! Don't see any messages after opening Youtube? Try delete your cache files.");
            for (;;) Thread.Sleep(100000);
        }
    }
}
