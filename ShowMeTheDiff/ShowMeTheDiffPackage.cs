﻿//------------------------------------------------------------------------------
// <copyright file="ShowMeTheDiffPackage.cs" company="Company">
//     Copyright (c) Company.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.ComponentModel.Design;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Runtime.InteropServices;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.OLE.Interop;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.Win32;
using System.IO;
using System.Reflection;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.TextManager.Interop;
using Microsoft.VisualStudio.Editor;
using EnvDTE;
using System.Windows.Forms;
using System.Data.SQLite;
using System.Linq;

namespace ShowMeTheDiff
{
    /// <summary>
    /// This is the class that implements the package exposed by this assembly.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The minimum requirement for a class to be considered a valid package for Visual Studio
    /// is to implement the IVsPackage interface and register itself with the shell.
    /// This package uses the helper classes defined inside the Managed Package Framework (MPF)
    /// to do it: it derives from the Package class that provides the implementation of the
    /// IVsPackage interface and uses the registration attributes defined in the framework to
    /// register itself and its components with the shell. These attributes tell the pkgdef creation
    /// utility what data to put into .pkgdef file.
    /// </para>
    /// <para>
    /// To get loaded into VS, the package must be referred by &lt;Asset Type="Microsoft.VisualStudio.VsPackage" ...&gt; in .vsixmanifest file.
    /// </para>
    /// </remarks>
    [PackageRegistration(UseManagedResourcesOnly = true)]
    [InstalledProductRegistration("#110", "#112", "1.0", IconResourceID = 400)] // Info on this package for Help/About
    [ProvideMenuResource("Menus.ctmenu", 1)]
    [Guid(ShowMeTheDiffPackage.PackageGuidString)]
    [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1650:ElementDocumentationMustBeSpelledCorrectly", Justification = "pkgdef, VS and vsixmanifest are valid VS terms")]
    public sealed class ShowMeTheDiffPackage : Package
    {
        /// <summary>
        /// ShowMeTheDiffPackage GUID string.
        /// </summary>
        public const string PackageGuidString = "00389fa6-1862-41e5-959f-05928944a6b7";



        /// <summary>
        /// Initializes a new instance of the <see cref="ShowMeTheDiff"/> class.
        /// </summary>
        public ShowMeTheDiffPackage()
        {
            
            //Assembly asm = Assembly.GetExecutingAssembly();
            //var localPath = Path.GetDirectoryName(asm.Location);
            //TextWriter tx = File.CreateText("C:\\Users\\g12n1260\\Desktop\\a.txt"); //new StreamWriter("C:\\Users\\g12n1260\\Desktop\\hello.txt");//new TextWriter("C:\\Users\\g12n1260\\Desktop\\hello.txt");
            //tx.Write("hello");
            //tx.Close();
            // Inside this method you can place any initialization code that does not require
            // any Visual Studio service because at this point the package object is created but
            // not sited yet inside Visual Studio environment. The place to do all the other
            // initialization is the Initialize method.
        }

        #region Package Members

        /// <summary>
        /// Initialization of the package; this method is called right after the package is sited, so this is the place
        /// where you can put all the initialization code that rely on services provided by VisualStudio.
        /// </summary>
        protected override void Initialize()
        {   base.Initialize();
            ShowMeTheDiff.Initialize(this);
            
            UseThisLineInstead.Initialize(this);
            showDiffLines.Initialize(this);
        }

        #endregion



    }

    //package to intiialise database and basefile when the solution exists and is fully loaded
    [ProvideAutoLoad(VSConstants.UICONTEXT.SolutionExistsAndFullyLoaded_string)]
    [Guid(MyVSPackagePackage.guidMyVSPackagePkgString)]
    public sealed class MyVSPackagePackage : Package
    {

        public const string guidMyVSPackagePkgString = "10534154-102D-46E2-ABA8-A6BFA25BA0BE";
        private string GetAllText(IWpfTextViewHost viewHost) =>
            viewHost.TextView.TextSnapshot.GetText();
        //constructor
        public MyVSPackagePackage()
        {   
        }

        protected override void Initialize()
        {
            base.Initialize();
            ShowSolutionProperties();
           
        }
        //sort out setting up the database
        private void setUpDatabase()
        {
            databaseHouseKeeping();

            var basePath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            SQLiteConnection connMakeDB = new SQLiteConnection(Path.Combine(basePath, "TimeMachineDB.sqlite"));
            SQLiteConnection conn = new SQLiteConnection("Data Source = TimeMachineDB.sqlite; Version = 3");
            SqlConnection = conn;
            conn.Open();

            Assembly asm = Assembly.GetExecutingAssembly();
            var localPath = Path.GetDirectoryName(asm.Location);

            createTables(conn);
            conn.Close();

        }


        //Create the Line and Verison tables
        private void createTables(SQLiteConnection conn)
        {
            //make sure no pre-existing records exist
            string drop = "Drop table Version;";
            SQLiteCommand cmnd1 = new SQLiteCommand(drop, conn);
            try { cmnd1.ExecuteNonQuery(); } catch (Exception e) { MessageBox.Show(e.ToString()); }

            drop = "Drop table Line;";
            cmnd1 = new SQLiteCommand(drop, conn);
            try { cmnd1.ExecuteNonQuery(); } catch (Exception e) { MessageBox.Show(e.ToString()); }
            
            string TableLine = "CREATE TABLE IF NOT EXISTS Line (  line_ID INTEGER PRIMARY KEY AUTOINCREMENT, line_Text TEXT, Line_Number INT, line_Comment TEXT,  line_Date datetime  DEFAULT (datetime('now','localtime')) );";
            string TableLineVersions = "CREATE TABLE IF NOT EXISTS Version ( version_ID INTEGER PRIMARY KEY AUTOINCREMENT, line_id INTEGER REFERENCES Line(line_ID), version_Text TEXT,  version_Comment, version_Date datetime DEFAULT (datetime('now','localtime')) ); ";
            string sql = string.Format("{0} {1}", TableLine, TableLineVersions);
            SQLiteCommand cmnd = new SQLiteCommand(sql, conn);
            try { cmnd.ExecuteNonQuery(); } catch (Exception e) { MessageBox.Show(e.ToString()); }
        }

        //make sure Visual Studio can communicate with SQLite
        private void databaseHouseKeeping()
        {   
            Assembly asm = Assembly.GetExecutingAssembly();
            var localPath = Path.GetDirectoryName(asm.Location);

            if (!Directory.Exists(Path.Combine(localPath, "x86")))
            {
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

        private void ShowSolutionProperties()
        {
            SVsSolution solutionService;
            IVsSolution solutionInterface;
            bool isSolutionOpen;
            string solutionDirectory;
            

            // Get the Solution service
            solutionService = (SVsSolution)GetService(typeof(SVsSolution));

            // Get the Solution interface of the Solution service
            solutionInterface = solutionService as IVsSolution;

            // Get some properties

            isSolutionOpen = GetPropertyValue<bool>(solutionInterface, __VSPROPID.VSPROPID_IsSolutionOpen);

            //wait till solution is open
            if (isSolutionOpen)
            {
                //Get location to put the basefile
                solutionDirectory = GetPropertyValue<string>(solutionInterface, __VSPROPID.VSPROPID_SolutionDirectory);
                var thiss = solutionDirectory.Split('\\');
                var path = Path.Combine(solutionDirectory, thiss[thiss.Length - 2]) ;
                var placeToStore = "bin\\Debug";
                var innitFile = @"Program.cs";
                var baseF = @"baseFile.txt";
                var PathB = Path.Combine(path, placeToStore, baseF);
                pathToFile = PathB;

                string sql;
                if (!File.Exists(Path.Combine(path, placeToStore, baseF)))
                {   //The first time it comes through here so basefile and database needs to be updated
                    TextWriter baseFile = File.CreateText(PathB);
                    setUpDatabase();
                    var lines = File.ReadAllLines(Path.Combine(path, innitFile));
                    foreach (string line in lines)
                    {
                        baseFile.WriteLine(line);
                    }

                    baseFile.Close();
                    SqlConnection.Open();

                    for (int i = 0; i < lines.Length; i++)
                    {
                        sql = string.Format("INSERT INTO LINE (line_Text, Line_Number ) VALUES ( '{0}' , {1} )  ", lines[i], i);
                        SQLiteCommand cmnd = new SQLiteCommand(sql, SqlConnection);
                        try { cmnd.ExecuteNonQuery(); } catch (Exception e) { MessageBox.Show(e.ToString()); }
                    }

                    SqlConnection.Close();
                }
                else {
                    //else get the basePath and where the database is
                    solutionDirectory = GetPropertyValue<string>(solutionInterface, __VSPROPID.VSPROPID_SolutionDirectory);


                     thiss = solutionDirectory.Split('\\');
                     path = Path.Combine(solutionDirectory, thiss[thiss.Length - 2]);
                     placeToStore = "bin\\Debug";
                     innitFile = @"Program.cs";
                     baseF = @"baseFile.txt";
                     PathB = Path.Combine(path, placeToStore, baseF);
                    pathToFile = PathB;

                    databaseHouseKeeping();

                    var basePath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
                    SQLiteConnection conn = new SQLiteConnection("Data Source = TimeMachineDB.sqlite; Version = 3");
                    SqlConnection = conn;
                }


            }
        }



        private T GetPropertyValue<T>(IVsSolution solutionInterface, __VSPROPID solutionProperty)
        {
            object value = null;
            T result = default(T);

            if (solutionInterface.GetProperty((int)solutionProperty, out value) == VSConstants.S_OK)
            {
                result = (T)value;
            }
            return result;
        }

        public static SQLiteConnection SqlConnection { get; private set; }
        public static string pathToFile { get; private set; }


    }
}
