using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Win32;
using mshtml;
using SHDocVw;

namespace YoutubeSelectiveAds
{
    //GUID reference of IF
    [
    ComVisible(true),
    InterfaceType(ComInterfaceType.InterfaceIsIUnknown),
    Guid("FC4801A3-2BA9-11CF-A229-00AA003D7352")
    ]
    //Declaration of the interface
    public interface IObjectWithSite
    {
        [PreserveSig]
        int SetSite([MarshalAs(UnmanagedType.IUnknown)]object site);
        [PreserveSig]
        int GetSite(ref Guid guid, out IntPtr ppvSite);
    }

    [
    ComVisible(true),
    //Guid("8a194578-81ea-4850-9911-13ba2d71efbd"),
        Guid("159d2c36-c31f-4a90-97b8-ce9a50ee185b"),
    ClassInterface(ClassInterfaceType.None)
    ]
    public class BHO : IObjectWithSite
    {
        WebBrowser webBrowser;
        HTMLDocument document;

        public void OnDocumentComplete(object pDisp, ref object URL)
        {
            //HTMLDocument document = (HTMLDocument)webBrowser.Document;

            //IHTMLElement head = (IHTMLElement)((IHTMLElementCollection)
            //                       document.all.tags("head")).item(null, 0);
            //IHTMLScriptElement scriptObject =
            //  (IHTMLScriptElement)document.createElement("script");
            //scriptObject.type = @"text/javascript";
            //scriptObject.text = "\nfunction hidediv(){document.getElementById" +
            //                    "('myOwnUniqueId12345').style.visibility = 'hidden';}\n\n";
            //((HTMLHeadElement)head).appendChild((IHTMLDOMNode)scriptObject);


            //string div = "<div id=\"myOwnUniqueId12345\" style=\"position:" +
            //             "fixed;bottom:0px;right:0px;z-index:9999;width=300px;" +
            //             "height=150px;\"> <div style=\"position:relative;" +
            //             "float:right;font-size:9px;\"><a " +
            //             "href=\"javascript:hidediv();\">close</a></div>" +
            //    "My content goes here ...</div>";

            //document.body.insertAdjacentHTML("afterBegin", div);

        }
        private void WebBrowser_WebWorkerStarted(uint dwUniqueID, string bstrWorkerLabel)
        {
            object a = null;
            WebBrowser_NavigateComplete2(null, ref a);
        }

        private void WebBrowser_WebWorkerFinsihed(uint dwUniqueID)
        {
            object a = null;
            WebBrowser_NavigateComplete2(null, ref a);
        }


        private void WebBrowser_UpdatePageStatus(object pDisp, ref object nPage, ref object fDone)
        {
            object a = null;
            WebBrowser_NavigateComplete2(null, ref a);
        }

        private void WebBrowser_DownloadComplete()
        {
            object a = null;
            WebBrowser_NavigateComplete2(null, ref a);
        }
        private void WebBrowser_BeforeScriptExecute(object pDispWindow)
        {
            object a = null;
            WebBrowser_NavigateComplete2(null, ref a);
        }

        private void WebBrowser_TitleChange(string Text)
        {

            object a = null;
            WebBrowser_NavigateComplete2(null, ref a);
        }

        private void WebBrowser_PropertyChange(string szProperty)
        {

            object a = null;
            WebBrowser_NavigateComplete2(null, ref a);
        }

        private void WebBrowser_ProgressChange(int Progress, int ProgressMax)
        {

            object a = null;
            WebBrowser_NavigateComplete2(null, ref a);
        }
        private void WebBrowser_NavigateComplete2(object pDisp, ref object URL)
        {
            var doc = (HTMLDocument)webBrowser.Document;
            var scripts = doc.all.tags("script");
            for (int i = 0; i < scripts.length; i++)
            {
                var elem = scripts.item(i);
                //var elem = .getElementsByName("www/base");
                var basescript = ((string)elem.outerHTML).Contains("name=\"www/base\"");
                //if (basescript)
                {
                    //IHTMLScriptElement newscript =
                    //  (IHTMLScriptElement)doc.createElement("script");
                    //newscript.type = @"text/javascript";
                    //newscript.text = "\nalert(\"\");\n";

                    //doc.insertBefore((IHTMLDOMNode)newscript, elem);
                    //elem.inner = "";
                    //doc.removeChild(elem);
                    //elem.parentNode.insertBefore(newscript, elem);

                    elem.parentNode.removeChild(elem);
                    elem.type = "text/javascript";
                    elem.src = "";
                    elem.text = "\nalert(\"\");\n";
                    elem.outerHTML = "";

                    //elem.src = "";
                    //elem.text = "alert(\"\");";
                }
            }
            //throw new NotImplementedException();
        }



        public void OnBeforeNavigate2(object pDisp, ref object URL, ref object Flags, ref object TargetFrameName, ref object PostData, ref object Headers, ref bool Cancel)
        {
            //document = (HTMLDocument)webBrowser.Document;

            //foreach (IHTMLInputElement tempElement in document.getElementsByTagName("INPUT"))
            //{
            //    if (tempElement.type.ToLower() == "password")
            //    {

            //        System.Windows.Forms.MessageBox.Show(tempElement.value);
            //    }

            //}

        }

        #region BHO Internal Functions
        public static string BHOKEYNAME = "Software\\Microsoft\\Windows\\CurrentVersion\\Explorer\\Browser Helper Objects";

        [ComRegisterFunction]
        public static void RegisterBHO(Type type)
        {
            RegistryKey registryKey = Registry.LocalMachine.OpenSubKey(BHOKEYNAME, true);

            if (registryKey == null)
                registryKey = Registry.LocalMachine.CreateSubKey(BHOKEYNAME);

            string guid = type.GUID.ToString("B");
            RegistryKey ourKey = registryKey.OpenSubKey(guid);

            if (ourKey == null)
                ourKey = registryKey.CreateSubKey(guid);

            ourKey.SetValue("NoExplorer", 1, RegistryValueKind.DWord);
            registryKey.Close();
            ourKey.Close();
        }
        [ComUnregisterFunction]
        public static void UnregisterBHO(Type type)
        {
            RegistryKey registryKey = Registry.LocalMachine.OpenSubKey(BHOKEYNAME, true);
            string guid = type.GUID.ToString("B");

            if (registryKey != null)
                registryKey.DeleteSubKey(guid, false);
        }
        public int SetSite(object site)
        {
            if (site != null)
            {
                webBrowser = (WebBrowser)site;
                webBrowser.DocumentComplete += new DWebBrowserEvents2_DocumentCompleteEventHandler(this.OnDocumentComplete);
                webBrowser.BeforeNavigate2 += new DWebBrowserEvents2_BeforeNavigate2EventHandler(this.OnBeforeNavigate2);
                webBrowser.NavigateComplete2 += WebBrowser_NavigateComplete2;
                webBrowser.BeforeScriptExecute += WebBrowser_BeforeScriptExecute;
                webBrowser.DownloadComplete += WebBrowser_DownloadComplete;
                webBrowser.UpdatePageStatus += WebBrowser_UpdatePageStatus;
                webBrowser.WebWorkerStarted += WebBrowser_WebWorkerStarted;
                webBrowser.WebWorkerFinsihed += WebBrowser_WebWorkerFinsihed;
                webBrowser.ProgressChange += WebBrowser_ProgressChange;
                webBrowser.PropertyChange += WebBrowser_PropertyChange;
                webBrowser.TitleChange += WebBrowser_TitleChange;
            }
            else
            {
                webBrowser.DocumentComplete -= new DWebBrowserEvents2_DocumentCompleteEventHandler(this.OnDocumentComplete);
                webBrowser.BeforeNavigate2 -= new DWebBrowserEvents2_BeforeNavigate2EventHandler(this.OnBeforeNavigate2);
                webBrowser.NavigateComplete2 -= WebBrowser_NavigateComplete2;
                webBrowser.BeforeScriptExecute -= WebBrowser_BeforeScriptExecute;
                webBrowser = null;
            }

            return 0;

        }


        public int GetSite(ref Guid guid, out IntPtr ppvSite)
        {
            IntPtr punk = Marshal.GetIUnknownForObject(webBrowser);
            int hr = Marshal.QueryInterface(punk, ref guid, out ppvSite);
            Marshal.Release(punk);

            return hr;
        }
        #endregion


    }
}
