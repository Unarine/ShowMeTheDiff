//------------------------------------------------------------------------------
// <copyright file="showDiffLines.cs" company="Company">
//     Copyright (c) Company.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.ComponentModel.Design;
using System.Globalization;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using my.utils;
using System.Reflection;
using System.IO;
using System.Linq;
using System.Data.SQLite;
using System.Windows.Forms;

namespace ShowMeTheDiff
{
    /// <summary>
    /// Command handler
    /// </summary>
    internal sealed class showDiffLines
    {
        /// <summary>
        /// Command ID.
        /// </summary>
        public const int CommandId = 256;

        /// <summary>
        /// Command menu group (command set GUID).
        /// </summary>
        public static readonly Guid CommandSet = new Guid("7b76ce58-4e97-4537-9b53-b2ab093ee2d2");

        /// <summary>
        /// VS Package that provides this command, not null.
        /// </summary>
        private readonly Package package;

        /// <summary>
        /// Initializes a new instance of the <see cref="showDiffLines"/> class.
        /// Adds our command handlers for menu (commands must exist in the command table file)
        /// </summary>
        /// <param name="package">Owner package, not null.</param>
        private showDiffLines(Package package)
        {
            if (package == null)
            {
                throw new ArgumentNullException("package");
            }

            this.package = package;

            OleMenuCommandService commandService = this.ServiceProvider.GetService(typeof(IMenuCommandService)) as OleMenuCommandService;
            if (commandService != null)
            {
                var menuCommandID = new CommandID(CommandSet, CommandId);
                var menuItem = new MenuCommand(this.MenuItemCallback, menuCommandID);
                commandService.AddCommand(menuItem);
            }
        }

        /// <summary>
        /// Gets the instance of the command.
        /// </summary>
        public static showDiffLines Instance
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the service provider from the owner package.
        /// </summary>
        private IServiceProvider ServiceProvider
        {
            get
            {
                return this.package;
            }
        }

        /// <summary>
        /// Initializes the singleton instance of the command.
        /// </summary>
        /// <param name="package">Owner package, not null.</param>
        public static void Initialize(Package package)
        {
            Instance = new showDiffLines(package);
        }

        /// <summary>
        /// This function is the callback used to execute the command when the menu item is clicked.
        /// See the constructor to see how the menu item is associated with this function using
        /// OleMenuCommandService service and MenuCommand class.
        /// </summary>
        /// <param name="sender">Event sender.</param>
        /// <param name="e">Event args.</param>
        private void MenuItemCallback(object sender, EventArgs e)
        {
            //Diff.Item [] hey = Diff.DiffText("heasdfy", "hey", false, false, false);
            setUpDatabase();
            MainLines main = new MainLines();
            main.Show();

            

           /* string message = string.Format(CultureInfo.CurrentCulture, "Inside {0}.MenuItemCallback()", this.GetType().FullName);
            string title = "showDiffLines";

            // Show a message box to prove we were here
            VsShellUtilities.ShowMessageBox(
                this.ServiceProvider,
                message,
                title,
                OLEMSGICON.OLEMSGICON_INFO,
                OLEMSGBUTTON.OLEMSGBUTTON_OK,
                OLEMSGDEFBUTTON.OLEMSGDEFBUTTON_FIRST); */
        }

        private void setUpDatabase()
        {
            databaseHouseKeeping();
            //throw new NotImplementedException();
            
            var basePath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            SQLiteConnection connMakeDB = new SQLiteConnection(Path.Combine(basePath, "TimeMachineDB.sqlite"));
            SQLiteConnection conn = new SQLiteConnection("Data Source = TimeMachineDB.sqlite; Version = 3");
            conn.Open();

            createTables(conn);

            conn.Close();
        }

        private void createTables(SQLiteConnection conn)
        {
            string TableLine = "CREATE TABLE IF NOT EXISTS Line (  line_ID INTEGER PRIMARY KEY AUTOINCREMENT, line_Text TEXT, Line_Number INT,  line_Date DATETIME );";
            string TableLineVersions = "CREATE TABLE IF NOT EXISTS Version ( version_ID INTEGER PRIMARY KEY AUTOINCREMENT, line_id VARCHAR(255) REFERENCES Line(line_ID), version_Text TEXT,  version_Date DATETIME ,  version_LineNumber INT); ";
            string sql = string.Format("{0} {1}", TableLine, TableLineVersions);
            SQLiteCommand cmnd = new SQLiteCommand(sql, conn);
            try { cmnd.ExecuteNonQuery(); } catch (Exception e) { MessageBox.Show(e.ToString()); }
        }

        private void databaseHouseKeeping()
        {
            Assembly asm = Assembly.GetExecutingAssembly();
            var localPath = Path.GetDirectoryName(asm.Location);

            if (!Directory.Exists(Path.Combine(localPath, "x86"))) {
                var arch32 = asm.GetManifestResourceNames().First(x => x.Contains("32SQLite"));
                var file32 = asm.GetManifestResourceStream(arch32);
                var buf32 = new byte[file32.Length];
                file32.Read(buf32, 0, buf32.Length);
                Directory.CreateDirectory(Path.Combine(localPath, "x86"));                
                File.WriteAllBytes(Path.Combine(localPath, "x86", "SQLite.Interop.dll"), buf32);
            }

            if (!Directory.Exists(Path.Combine(localPath, "x64")))
            {
                var arch64 = asm.GetManifestResourceNames().First(x => x.Contains("64SQLite"));
                var file64 = asm.GetManifestResourceStream(arch64);
                var buf64 = new byte[file64.Length];
                file64.Read(buf64, 0, buf64.Length);

                Directory.CreateDirectory(Path.Combine(localPath, "x64"));
                File.WriteAllBytes(Path.Combine(localPath, "x64", "SQLite.Interop.dll"), buf64); 
            }

        }
    }
}
