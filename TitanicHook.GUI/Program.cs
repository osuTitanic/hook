using System;
using System.Windows.Forms;

namespace TitanicHook.GUI
{
    internal static class Program
    {
        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            // To customize application configuration such as set high DPI settings or default font,
            // see https://aka.ms/applicationconfiguration.
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            // TODO: Check -go argument & skip launcher gui if present

            string? pathToLaunch;
            using (Launcher launcher = new Launcher())
            {
                Application.Run(launcher);
                pathToLaunch = launcher.PathToLaunch;
            }

            if (pathToLaunch != null)
                Loader.OsuLoader.Start(pathToLaunch);
        }
    }
}
