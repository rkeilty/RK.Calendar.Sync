using NLog;
using System;
using System.Drawing;
using System.Threading;
using System.Windows.Forms;
using RK.CalendarSync.Core;

namespace RK.CalendarSync
{
    public class CalendarSyncApplication : Form
    {
        /// <summary>
        /// Logger for the class
        /// </summary>
        private static readonly Logger LOGGER = LogManager.GetCurrentClassLogger();

        //  The main entry point for the process
        [MTAThread]
        public static void Main(string[] args)
        {
            if (args.Length > 0 && (args[0].Equals("--console") || args[0].Equals("-c")))
            {
                // Command line given, display console
                AllocConsole();
                var service = new CalendarSyncService();
                service.Start();

                // Spin until a "q" is pressed in the console.
                Console.WriteLine("Press 'q' to quit application");
                while (Console.ReadKey().Key != ConsoleKey.Q)
                { }

                service.Stop();
            }
            else
            {
                Application.ThreadException += new ThreadExceptionEventHandler(Application_ThreadException);
                AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler(CurrentDomain_UnhandledException);
                Application.Run(new CalendarSyncApplication());
            }
        }

        private NotifyIcon _trayIcon;
        private ContextMenu _trayMenu;
        private CalendarSyncService _calendarSyncService;

        public CalendarSyncApplication()
        { 
            // Setup the shutdown action
            Application.ApplicationExit += OnApplicationExit;


            // Create the exit icon.
            _trayMenu = new ContextMenu();
            _trayMenu.MenuItems.Add("Exit", OnTrayExit);

            // Create a tray icon. In this example we use a
            // standard system icon for simplicity, but you
            // can of course use your own custom icon too.
            _trayIcon = new NotifyIcon
                {
                    Text = "RK Calendar Sync",
                    Icon = new Icon(SystemIcons.Application, 40, 40),
                    ContextMenu = _trayMenu,
                    Visible = true,
                    
                };

            // Spin up a Calendar Sync Service
            _calendarSyncService = new CalendarSyncService();
        }

        /// <summary>
        /// On load hide everything except the system bar.
        /// </summary>
        /// <param name="e"></param>
        protected override void OnLoad(EventArgs e)
        {
            // Start with window/taskbar not showing
            ShowWindowAndTaskbar(false);

            // Start the service in a background thread
            _calendarSyncService.Start();

            base.OnLoad(e);
        }


        private void OnTrayExit(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void ShowWindowAndTaskbar(bool show)
        {
            if (show)
            {
                Visible = true; // Show form window.
                ShowInTaskbar = true; // Show from taskbar.
            }
            else
            {
                Visible = false; // Hide form window.
                ShowInTaskbar = false; // Remove from taskbar.
            }
        }

        /// <summary>
        /// Actions to take when the application is exiting.
        /// </summary>
        private void OnApplicationExit(object sender, EventArgs e)
        {
            _calendarSyncService.Stop();
        }

        /// <summary>
        /// Dispose of the tray icon.
        /// </summary>
        /// <param name="isDisposing"></param>
        protected override void Dispose(bool isDisposing)
        {
            if (isDisposing)
            {
                // Release the icon resource.
                _trayIcon.Dispose();
            }

            base.Dispose(isDisposing);
        }
        /// <summary>
        /// Log any unhandled applications thrown from a thread.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private static void Application_ThreadException(object sender, ThreadExceptionEventArgs args)
        {
            LOGGER.Log(LogLevel.Error, "Unexpected thread exception.", args.Exception);
        }

        /// <summary>
        /// Log any unhandled exceptions from the application.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs args)
        {
            var exception = args.ExceptionObject as Exception;
            if (exception == null)
            {
                LOGGER.Log(LogLevel.Error, "Unexpected application exception, unable to cast to exception object.");
                return;
            }

            LOGGER.Log(LogLevel.Error, "Unexpected application exception", exception);
        }


        /// <summary>
        /// Useful when we want to invoke our application via a command line argument like "-c" or "--console"
        /// </summary>
        /// <returns></returns>
        [System.Runtime.InteropServices.DllImport("kernel32.dll")]
        private static extern bool AllocConsole();
    }
}
