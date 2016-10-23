//------------------------------------------------------------------------------
// <copyright file="UseThisLineInstead.cs" company="Company">
//     Copyright (c) Company.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.ComponentModel.Design;
using System.Globalization;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.TextManager.Interop;
using Microsoft.VisualStudio.Editor;
using EnvDTE80;
using EnvDTE;
using Microsoft.VisualStudio.Text.Operations;
using Microsoft.VisualStudio.Text;

namespace ShowMeTheDiff
{
    /// <summary>
    /// Command handler
    /// </summary>
    internal sealed class UseThisLineInstead
    {
        /// <summary>
        /// Command ID.
        /// </summary>
        public const int CommandId = 256;

        /// <summary>
        /// Command menu group (command set GUID).
        /// </summary>
        public static readonly Guid CommandSet = new Guid("aa89b473-06ee-4bbd-8c0d-a3722431334b");

        /// <summary>
        /// VS Package that provides this command, not null.
        /// </summary>
        private readonly Package package;

        /// <summary>
        /// Initializes a new instance of the <see cref="UseThisLineInstead"/> class.
        /// Adds our command handlers for menu (commands must exist in the command table file)
        /// </summary>
        /// <param name="package">Owner package, not null.</param>
        private UseThisLineInstead(Package package)
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
        public static UseThisLineInstead Instance
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
            Instance = new UseThisLineInstead(package);
        }


        

        private string GetAllText(IWpfTextViewHost viewHost) =>
           // viewHost.TextView.Caret.ContainingTextViewLine;
            //viewHost.TextView.GetTextViewLineContainingBufferPosition(viewHost.TextView.Caret.Position); 
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

            var viewhost = ShowMeTheDiff.Instance.currentView;//GetCurrentViewHost();
            var line = viewhost.TextView.Caret.ContainingTextViewLine.Extent.GetText();
            var position = viewhost.TextView.Caret.Position.BufferPosition.Position;
            

            var screengrab = GetAllText(viewhost); //grab screen host
            
            var sP = position; // startPosition
            if (screengrab[sP] == '\r') sP--; //if the user clicked at the very end of the line
            while (sP > 0 && screengrab[sP] != '\r' && screengrab[sP] != '\n') sP--;
            var eP = position; // endPosition
            while (eP < screengrab.Length - 1 && screengrab[eP] != '\r' && screengrab[eP] != '\n') eP++;
            var myline = screengrab.Substring(sP - 1 , eP - sP +1); //the length of it should be start position - end position
            
            var fn = ShowMeTheDiff.Instance.WorkingFile;
            var everything = System.IO.File.ReadAllText(fn);
            

            var newLines = "";
            //int counterMyLine = 0;
            //everything[position] = "s";
            //for (int i = sP; i < eP; i++) {
            //everything[i] = myline[counterMyLine];
            //}





            var sP1 = sP;
            var eP1 = eP;
            while (sP1 > 0 && everything[sP1] != '\r' && everything[sP1] != '\n') sP1--;
            while (eP1 < everything.Length - 1 && everything[eP1] != '\r' && everything[eP1] != '\n') eP1++;


            newLines += everything.Substring(0, sP1-1);//, everything.Length-eP+1
            newLines +=  myline;
            newLines += everything.Substring(eP1);

            System.IO.File.WriteAllText(fn, newLines);
            //var lol = everything[position];
            //var lines = System.IO.File.ReadAllLines(fn);
            //lines[12] = "lol yas";



            //TextExtent extent = new TextExtent();
            //SnapshotSpan range = new SnapshotSpan() ;
            //ITrackingSpan trackingSpan = range.Snapshot.CreateTrackingSpan(extent.Span, SpanTrackingMode.EdgeInclusive);
            //ITextSnapshot m_snapshot = trackingSpan.TextBuffer.CurrentSnapshot;
            //var curline = trackingSpan.GetText(m_snapshot);
            //trackingSpan.TextBuffer.Replace(trackingSpan.GetSpan(m_snapshot), line);


            //var dte = (DTE2)ServiceProvider.GetService(typeof(DTE));
            //var lol = dte.MainWindow.Document;

            /*string title = "UseThisLineInstead";

            // Show a message box to prove we were here
            VsShellUtilities.ShowMessageBox(
                this.ServiceProvider,
                message,
                title,
                OLEMSGICON.OLEMSGICON_INFO,
                OLEMSGBUTTON.OLEMSGBUTTON_OK,
                OLEMSGDEFBUTTON.OLEMSGDEFBUTTON_FIRST); */

        }
    }
}
