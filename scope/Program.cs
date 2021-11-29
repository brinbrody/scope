﻿using System;
using System.Windows.Forms;
using System.IO;
using System.Reflection;
using System.Linq;
using DGScope.Receivers;
using System.Security.Principal;

namespace DGScope
{
    static class Program
    {
        static void Start(bool screensaver = false)
        {
            string settingsPath = (screensaver || IsAdministrator()) ? Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "DGScope.xml") :
               Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "DGScope.xml");
            RadarWindow radarWindow;
            if (File.Exists(settingsPath))
            {
                radarWindow = TryLoad(settingsPath);
            }
            else
            {
                radarWindow = new RadarWindow();
                if (!screensaver)
                {
                    MessageBox.Show("No config file found. Starting a new config.");
                    PropertyForm propertyForm = new PropertyForm(radarWindow);
                    propertyForm.ShowDialog();
                    radarWindow.SaveSettings(settingsPath);
                }
            }
            radarWindow.Run(screensaver);
        }
        [STAThread]
        static void Main(string[] args)
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            LoadReceiverPlugins();
            if (args.Length > 0)
            {
                string arg = args[0].ToUpper().Trim();
                if (arg.Contains("/C")) 
                {
                    string settingsPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "DGScope.xml");
                    RadarWindow radarWindow;
                    if (File.Exists(settingsPath))
                    {
                        radarWindow = TryLoad(settingsPath);
                    }
                    else
                    {
                        radarWindow = new RadarWindow();
                    }
                    PropertyForm propertyForm = new PropertyForm(radarWindow);
                    propertyForm.ShowDialog();
                    radarWindow.SaveSettings(settingsPath);
                }
                else if (arg.Contains("/S"))
                {
                    Start(true);
                }
                else if (arg.Contains("/P"))
                {
                    //do nothing
                }
                else
                {
                    Start(false);
                }
            }
            else
            {
                Start(false);
            }
        }

        public static bool IsAdministrator()
        {
            var identity = WindowsIdentity.GetCurrent();
            var principal = new WindowsPrincipal(identity);
            return principal.IsInRole(WindowsBuiltInRole.Administrator);
        }
        static RadarWindow TryLoad(string settingsPath)
        {
            try
            {
                return XmlSerializer<RadarWindow>.DeserializeFromFile(settingsPath);
            }
            catch (Exception ex)
            {
                RadarWindow radarWindow = new RadarWindow();
                Console.WriteLine(ex.StackTrace);
                var mboxresult = MessageBox.Show("Error reading settings file "+ settingsPath + "\n" + 
                    ex.Message + "\nPress Abort to exit, Retry to try again, or Ignore to destroy " +
                    "the file and start a new config.", "Error reading settings file", 
                    MessageBoxButtons.AbortRetryIgnore, MessageBoxIcon.Error);
                if (mboxresult == DialogResult.Abort)
                    Environment.Exit(1);
                else if (mboxresult == DialogResult.Retry)
                    return TryLoad(settingsPath);
                else
                {
                    radarWindow = new RadarWindow();
                    PropertyForm propertyForm = new PropertyForm(radarWindow);
                    propertyForm.ShowDialog();
                    return radarWindow;
                }
            }
            return new RadarWindow();
        }

        static void LoadReceiverPlugins()
        {
            String path = Application.StartupPath;
            string[] pluginFiles = Directory.GetFiles(path, "DGScope.*.dll");
            var ipi = (from file in pluginFiles let asm = Assembly.LoadFile(file)
                      from plugintype in asm.GetExportedTypes()
                      where typeof(Receiver).IsAssignableFrom(plugintype)
                      select (Receiver)Activator.CreateInstance(plugintype)).ToArray();
        }
        
    }
    
    
    

    
    
    
}
