using Microsoft.VisualStudio.Editor;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.TextManager.Interop;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShowMeTheDiff
{

        
    class dealWithLines
    {




        //get lines from viewhost 

        private static string GetAllText(IWpfTextViewHost viewHost) =>
            viewHost.TextView.TextSnapshot.GetText();

        public static string[]  getFiles(){

            //MyVSPackagePackage.SqlConnection.Open();

        //var baseLines = File.ReadAllLines(MyVSPackagePackage.pathToFile);
        
        var viewhost = showDiffLines.Instance.currentView;
            var lines = GetAllText(viewhost);
            return lines.Split('\n');
        }

        

    }
}
