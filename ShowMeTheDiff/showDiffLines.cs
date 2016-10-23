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
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.TextManager.Interop;
using Microsoft.VisualStudio.Editor;
using System.Collections.Generic;

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



        private IWpfTextViewHost GetCurrentViewHost()
        {
            var textManager = this.ServiceProvider.GetService(typeof(SVsTextManager)) as IVsTextManager;
            IVsTextView textView = null;
            int mustHaveFocus = 1;
            textManager.GetActiveView(mustHaveFocus, null, out textView);

            var userData = textView as IVsUserData;
            if (userData == null)
            {
                return null;
            }
            else
            {
                Guid guidViewHost = DefGuidList.guidIWpfTextViewHost;
                object holder;
                userData.GetData(ref guidViewHost, out holder);
                var viewHost = (IWpfTextViewHost)holder;
                return viewHost;
            }

        }


        private static string GetAllText(IWpfTextViewHost viewHost) =>
        viewHost.TextView.TextSnapshot.GetText();

        /// <summary>
        /// This function is the callback used to execute the command when the menu item is clicked.
        /// See the constructor to see how the menu item is associated with this function using
        /// OleMenuCommandService service and MenuCommand class.
        /// </summary>
        /// <param name="sender">Event sender.</param>
        /// <param name="e">Event args.</param>
        private void MenuItemCallback(object sender, EventArgs e)
        {

            var viewhost = GetCurrentViewHost();
            var position = viewhost.TextView.Caret.Position.BufferPosition.Position;
            var screengrab = GetAllText(viewhost);

            //work out which line was clicked on
            int curLine = 0;

            var sP = 0; // startPosition the begiinging
            while (sP < position) {
                if (screengrab[sP] == '\r' && screengrab[sP+1] == '\n') {
                    curLine++;
                }

                sP++;
            }


            
            //Diff.Item [] hey = Diff.DiffText("heasdfy", "hey", false, false, false);
            MainLines main = new MainLines(curLine, MyVSPackagePackage.SqlConnection );
            main.Show();
            
            //handleLines(screengrab);
            

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



        private void handleLines(string screengrab)
        {
            
            
            //var viewhost = showDiffLines.Instance.currentView;

            var Currentlines = screengrab.Split(new string[] { "\r\n" }, StringSplitOptions.None);

            //for (int i = 0; i < Currentlines.Length; i++) {
            //    Currentlines[i] = Currentlines[i].Substring(0, Currentlines[i].Length - 1);
            //}
            var BasefileLines = File.ReadAllLines(MyVSPackagePackage.pathToFile);
            File.WriteAllLines(MyVSPackagePackage.pathToFile, Currentlines);
            List<string> toWriteBack = new List<string>();
            var conn = MyVSPackagePackage.SqlConnection;
            conn.Open();
            //File.WriteAllLines(MyVSPackagePackage.pathToFile, )
            //TextWriter bx = File
            //easy case --number of lines are still the same
            if (BasefileLines.Length == Currentlines.Length)
            {
                for (int i = 0; i < BasefileLines.Length; i++)
                {
                    if (Diff.DiffText(BasefileLines[i], Currentlines[i], false, false, false).Length > 0)
                    {
                        string sql = string.Format("SELECT line_ID  FROM Line WHERE Line_Number = {0}", i); //string.Format("INSERT INTO LINE (line_Text, Line_Number,  line_Date ) VALUES ( '{0}' , {1}  , CURRENT_TIMESTAMP)  ", lines[i], i);
                        SQLiteCommand cmnd = new SQLiteCommand(sql, conn);

                        //try { SQLiteDataReader reader = cmnd.ExecuteReader(); } catch (Exception ee) { MessageBox.Show(ee.ToString()); }
                        //SQLiteDataReader reader = cmnd.ExecuteReader();
                        SQLiteDataReader reader = cmnd.ExecuteReader();

                        // try { var id = reader["line_ID"]; } catch (Exception eee) { MessageBox.Show(eee.ToString()); }
                        //( version_ID INTEGER PRIMARY KEY AUTOINCREMENT, line_id INTEGER REFERENCES Line(line_ID), version_Text TEXT,   version_LineNumber INT, version_Comment, version_Date DATETIME );
                        //sql = string.Format("INSERT INTO LINE (line_Text, Line_Number,  line_Date ) VALUES ( '{0}' , {1}  , CURRENT_TIMESTAMP)  ", lines[i], i);
                        while (reader.Read())
                        {
                            sql = string.Format("INSERT INTO VERSION  (line_id, version_Text, version_LineNumber,version_Date ) VALUES ({0}, '{1}',{2},  CURRENT_TIMESTAMP)", reader["line_ID"], Currentlines[i], i);
                            SQLiteCommand cmnd1 = new SQLiteCommand(sql, conn);
                            try { cmnd1.ExecuteNonQuery(); } catch (Exception er) { MessageBox.Show(er.ToString()); }
                        }

                    }

                }
                conn.Close();
            }
            //lines have been added
            else if (BasefileLines.Length < Currentlines.Length)
            {
                for (int i = 0; i < BasefileLines.Length; i++)
                {
                    if (Diff.DiffText(BasefileLines[i], Currentlines[i], false, false, false).Length > 0)
                    {
                        string sql = string.Format("SELECT line_ID  FROM Line WHERE Line_Number = {0}", i); //string.Format("INSERT INTO LINE (line_Text, Line_Number,  line_Date ) VALUES ( '{0}' , {1}  , CURRENT_TIMESTAMP)  ", lines[i], i);
                        SQLiteCommand cmnd = new SQLiteCommand(sql, conn);

                        //try { SQLiteDataReader reader = cmnd.ExecuteReader(); } catch (Exception ee) { MessageBox.Show(ee.ToString()); }
                        //SQLiteDataReader reader = cmnd.ExecuteReader();
                        SQLiteDataReader reader = cmnd.ExecuteReader();

                        // try { var id = reader["line_ID"]; } catch (Exception eee) { MessageBox.Show(eee.ToString()); }
                        //( version_ID INTEGER PRIMARY KEY AUTOINCREMENT, line_id INTEGER REFERENCES Line(line_ID), version_Text TEXT,   version_LineNumber INT, version_Comment, version_Date DATETIME );
                        //sql = string.Format("INSERT INTO LINE (line_Text, Line_Number,  line_Date ) VALUES ( '{0}' , {1}  , CURRENT_TIMESTAMP)  ", lines[i], i);
                        while (reader.Read())
                        {
                            sql = string.Format("INSERT INTO VERSION  (line_id, version_Text, version_LineNumber,version_Date ) VALUES ({0}, '{1}',{2},  CURRENT_TIMESTAMP)", reader["line_ID"], Currentlines[i], i);
                            SQLiteCommand cmnd1 = new SQLiteCommand(sql, conn);
                            try { cmnd1.ExecuteNonQuery(); } catch (Exception er) { MessageBox.Show(er.ToString()); }
                        }
                    }


                }
                for (int i = BasefileLines.Length - 1; i < Currentlines.Length; i++)
                {
                    string sql = string.Format("INSERT INTO LINE (line_Text, Line_Number,  line_Date ) VALUES ( '{0}' , {1}  , CURRENT_TIMESTAMP)  ", Currentlines[i], i);
                    SQLiteCommand cmnd = new SQLiteCommand(sql, conn);
                    try { cmnd.ExecuteNonQuery(); } catch (Exception en) { MessageBox.Show(en.ToString()); }

                }
                //add the newLines to the Lines into the database
                conn.Close();

            }
            //else lines have been deleted
            else
            {
                for (int i = 0; i < Currentlines.Length; i++)
                {
                    if (Diff.DiffText(BasefileLines[i], Currentlines[i], false, false, false).Length > 0)
                    {
                        string sql = string.Format("SELECT line_ID  FROM Line WHERE Line_Number = {0}", i); //string.Format("INSERT INTO LINE (line_Text, Line_Number,  line_Date ) VALUES ( '{0}' , {1}  , CURRENT_TIMESTAMP)  ", lines[i], i);
                        SQLiteCommand cmnd = new SQLiteCommand(sql, conn);

                        //try { SQLiteDataReader reader = cmnd.ExecuteReader(); } catch (Exception ee) { MessageBox.Show(ee.ToString()); }
                        //SQLiteDataReader reader = cmnd.ExecuteReader();
                        SQLiteDataReader reader = cmnd.ExecuteReader();

                        // try { var id = reader["line_ID"]; } catch (Exception eee) { MessageBox.Show(eee.ToString()); }
                        //( version_ID INTEGER PRIMARY KEY AUTOINCREMENT, line_id INTEGER REFERENCES Line(line_ID), version_Text TEXT,   version_LineNumber INT, version_Comment, version_Date DATETIME );
                        //sql = string.Format("INSERT INTO LINE (line_Text, Line_Number,  line_Date ) VALUES ( '{0}' , {1}  , CURRENT_TIMESTAMP)  ", lines[i], i);
                        while (reader.Read())
                        {
                            sql = string.Format("INSERT INTO VERSION  (line_id, version_Text, version_LineNumber,version_Date ) VALUES ({0}, '{1}',{2},  CURRENT_TIMESTAMP)", reader["line_ID"], Currentlines[i], i);
                            SQLiteCommand cmnd1 = new SQLiteCommand(sql, conn);
                            try { cmnd1.ExecuteNonQuery(); } catch (Exception er) { MessageBox.Show(er.ToString()); }
                        }
                    }

                }

                for (int i = Currentlines.Length; i < BasefileLines.Length - 1; i++)
                {
                    string sql = string.Format("SELECT line_ID  FROM Line WHERE Line_Number = {0}", i); //string.Format("INSERT INTO LINE (line_Text, Line_Number,  line_Date ) VALUES ( '{0}' , {1}  , CURRENT_TIMESTAMP)  ", lines[i], i);
                    SQLiteCommand cmnd = new SQLiteCommand(sql, conn);

                    //try { SQLiteDataReader reader = cmnd.ExecuteReader(); } catch (Exception ee) { MessageBox.Show(ee.ToString()); }
                    //SQLiteDataReader reader = cmnd.ExecuteReader();
                    SQLiteDataReader reader = cmnd.ExecuteReader();

                    // try { var id = reader["line_ID"]; } catch (Exception eee) { MessageBox.Show(eee.ToString()); }
                    //( version_ID INTEGER PRIMARY KEY AUTOINCREMENT, line_id INTEGER REFERENCES Line(line_ID), version_Text TEXT,   version_LineNumber INT, version_Comment, version_Date DATETIME );
                    //sql = string.Format("INSERT INTO LINE (line_Text, Line_Number,  line_Date ) VALUES ( '{0}' , {1}  , CURRENT_TIMESTAMP)  ", lines[i], i);
                    while (reader.Read())
                    {
                        sql = string.Format("INSERT INTO VERSION  (line_id, version_Text, version_LineNumber,version_Date ) VALUES ({0}, '{1}',{2},  CURRENT_TIMESTAMP)", reader["line_ID"], " ", i);
                        SQLiteCommand cmnd1 = new SQLiteCommand(sql, conn);
                        try { cmnd1.ExecuteNonQuery(); } catch (Exception er) { MessageBox.Show(er.ToString()); }
                    }


                }
                //insert nulls for the remainder
                conn.Close();
            } //else
        }

        public IWpfTextViewHost currentView { get; private set; }
        


    }
}
