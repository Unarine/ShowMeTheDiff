using Microsoft.VisualStudio.Text.Editor;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShowMeTheDiff
{
    class HandlePatch
    {
        private static string GetAllText(IWpfTextViewHost viewHost) =>
        viewHost.TextView.TextSnapshot.GetText();
        //get the files


        public void getDiff()
        {
            var viewhost = showDiffLines.Instance.currentView;
            var Currentlines = GetAllText(viewhost).Split('\n');

            var BasefileLines = File.ReadAllLines(MyVSPackagePackage.pathToFile);

            var lineCount = Currentlines.Length;
            //var diff = my.utils.Diff.DiffText()

        }
    }
}
