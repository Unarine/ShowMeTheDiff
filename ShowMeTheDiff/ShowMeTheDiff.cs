//------------------------------------------------------------------------------
// <copyright file="ShowMeTheDiff.cs" company="Company">
//     Copyright (c) Company.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.ComponentModel.Design;
using System.Globalization;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using EnvDTE80;
using EnvDTE;
using System.Windows.Forms;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.TextManager.Interop;
using Microsoft.VisualStudio.Editor;
using System.IO;
using System.Text;

namespace ShowMeTheDiff
{
    /// <summary>
    /// Command handler
    /// </summary>
    internal sealed class ShowMeTheDiff
    {
        /// <summary>
        /// Command ID.
        /// </summary>
        public const int CommandId = 0x0100;

        /// <summary>
        /// Command menu group (command set GUID).
        /// </summary>
        public static readonly Guid CommandSet = new Guid("508596cf-e79a-4766-9042-851a7f1aa5c1");

        /// <summary>
        /// VS Package that provides this command, not null.
        /// </summary>
        private readonly Package package;

        /// <summary>
        /// Initializes a new instance of the <see cref="ShowMeTheDiff"/> class.
        /// Adds our command handlers for menu (commands must exist in the command table file)
        /// </summary>
        /// <param name="package">Owner package, not null.</param>
        private ShowMeTheDiff(Package package)
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
        public static ShowMeTheDiff Instance
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
            Instance = new ShowMeTheDiff(package);
            
        }

        private IWpfTextViewHost GetCurrentViewHost() {
            var textManager = this.ServiceProvider.GetService(typeof(SVsTextManager)) as IVsTextManager;
            IVsTextView textView = null;
            int mustHaveFocus = 1;
            textManager.GetActiveView(mustHaveFocus, null, out textView);

            var userData = textView as IVsUserData;
            if (userData == null)
            {
                return null;
            }
            else {
                Guid guidViewHost = DefGuidList.guidIWpfTextViewHost;
                object holder;
                userData.GetData(ref guidViewHost, out holder);
                var viewHost = (IWpfTextViewHost)holder;
                
                return viewHost;
            }

        }


        private string GetAllText(IWpfTextViewHost viewHost) =>
            viewHost.TextView.TextSnapshot.GetText();




        /// <summary>
        /// This function is the callback used to execute the command when the menu item is clicked.
        /// See the constructor to see how the menu item is associated with this function using
        /// OleMenuCommandService service and MenuCommand class.
        ///// </summary>
        /// <param name="sender">Event sender.</param>
        /// <param name="e">Event args.</param>
        private void MenuItemCallback(object sender, EventArgs e)
        {
            
            var viewhost = GetCurrentViewHost();
            var screengrab = GetAllText(viewhost);
            currentView = viewhost;
           // var warrisdis = screengrab.Split('\n');
            
            var dte = (DTE2)ServiceProvider.GetService(typeof(DTE));
            var lol = dte.MainWindow.Document;
            
            string filename = dte.ActiveDocument.FullName;
            string [] file_name = filename.Split('\\') ;
            string directory = "";
            for (int i = 0; i < file_name.Length-1; i++) {
                directory += file_name[i] + "\\";
            }

            string[] FName = file_name[file_name.Length - 1].Split('.');
            DateTime date = DateTime.Now;

            string toWriteinto = FName[0] + "@" + date.Day + "_" + date.Month + "_" + date.Year + "_" + "@" + date.Hour + "h" + date.Minute + "." + FName[1] ;
            
            //save the file and change the encoding 
            TextWriter txtResult = new StreamWriter(directory + toWriteinto, true, Encoding.UTF8);
            txtResult.Write(screengrab);
            txtResult.Close();


            string file1, file2;
            var dialog = new OpenFileDialog();
            dialog.InitialDirectory = directory;
            dialog.ShowDialog();

            file1 = "\"" + dialog.FileName + "\"";
            file2 = "\"" + filename + "\"";

            // store this filename as something that UseThisLineInstead can use.

            WorkingFile = filename;

            dte.ExecuteCommand("Tools.Difffiles", $"{file1} {file2}");
            


        }

        public string WorkingFile { get; private set; }
        public IWpfTextViewHost currentView { get; private set; }


    }
}
