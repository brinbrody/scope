using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using OpenTK.Windowing.Desktop;
using DGScope.Nexrad;
using System.Net;
using System.IO;
using DGScope.Library;
using System.Threading;

namespace ScopeWindow
{
    static class Program
    {
        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            /*Application.SetHighDpiMode(HighDpiMode.SystemAware);
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);*/
            GameWindowSettings gameWindowSettings = GameWindowSettings.Default;
            NativeWindowSettings nativeWindowSettings = new NativeWindowSettings();
            ScopeWindowSettings scopeWindowSettings = new ScopeWindowSettings()
            {
                AdaptationFileName = "E:\\Users\\Dennis\\Source\\Repos\\scope\\build\\Debug\\bva.adaptjson",
                WSType = ScopeWindow.WSType.TCW
            };
            var window = new ScopeWindow(gameWindowSettings, nativeWindowSettings, scopeWindowSettings);
            window.Run();
        }
    }
}
