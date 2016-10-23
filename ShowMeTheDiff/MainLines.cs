using Microsoft.VisualStudio.Text.Editor;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Data.SQLite;

namespace ShowMeTheDiff
{
    public partial class MainLines : Form
    {
        private int curLine;
        private SQLiteConnection sqlConnection;

        public MainLines()
        {



            InitializeComponent();

            //var items = dealWithLines.getFiles();
            //var items = ["asdfasdf", "sdf"];
            //for (int i = 0; i < items.Length; i++) {
            //listBox1.Items.Add(items[i]);
            //}
            listBox1.Items.Add("asd");
            listBox1.Click += OnListBoxItemClick;

            //listBox1.Items.Add(my.utils.Diff.DiffText("hey", "hey", false, false, false));
        }

        public MainLines(int curLine, SQLiteConnection sqlConnection)
        {
            InitializeComponent();
            this.curLine = curLine;
            this.sqlConnection = sqlConnection;
            sqlConnection.Open();

             listBox1.DisplayMember = "line_Text";
            
            listBox1.ValueMember = "line_id";


            string sql = string.Format("SELECT * FROM Version WHERE version_LineNumber = {0} ORDER BY version_Date DESC ", curLine);
            SQLiteCommand cmd3 = new SQLiteCommand(sql, sqlConnection);
            
            SQLiteDataReader reader = cmd3.ExecuteReader();
            while (reader.Read())
            {
                //object todisplay = reader["version_Text"];


                string todisplay = string.Format("{0} | {1}", relativedate(reader["version_Date"]), reader["version_Text"]);
                //string l;
                listBox1.Items.Add(new LineToDisplay { line_Text = todisplay, line_id = reader["version_ID"]});
                

            //Console.WriteLine(reader["Line_ID"] + ","); //better way of getting it keh :-)
            

            }
            sqlConnection.Close();

            listBox1.Click += OnListBoxItemClick;
        }

        private void OnListBoxItemClick(object sender, EventArgs e)
        {
            MoreLineDetails md = new MoreLineDetails((listBox1.SelectedItem as LineToDisplay).line_id, MyVSPackagePackage.SqlConnection);
            md.Show();
        }


        public object relativedate(object d)
        {
            DateTime theDate = (DateTime)d; // come back and delete this
            Dictionary<long, string> thresholds = new Dictionary<long, string>();
            int minute = 60;
            int hour = 60 * minute;
            int day = 24 * hour;
            thresholds.Add(60, "{0} seconds ago");
            thresholds.Add(minute * 2, "a minute ago");
            thresholds.Add(45 * minute, "{0} minutes ago");
            thresholds.Add(120 * minute, "an hour ago");
            thresholds.Add(day, "{0} hours ago");
            thresholds.Add(day * 2, "yesterday");
            thresholds.Add(day * 30, "{0} days ago");
            thresholds.Add(day * 365, "{0} months ago");
            thresholds.Add(long.MaxValue, "{0} years ago");

            long since = (DateTime.Now.Ticks - theDate.Ticks) / 10000000;
            foreach (long threshold in thresholds.Keys)
            {
                if (since < threshold)
                {
                    TimeSpan t = new TimeSpan((DateTime.Now.Ticks - theDate.Ticks));
                    return string.Format(thresholds[threshold], (t.Days > 365 ? t.Days / 365 : (t.Days > 0 ? t.Days : (t.Hours > 0 ? t.Hours : (t.Minutes > 0 ? t.Minutes : (t.Seconds > 0 ? t.Seconds : 0))))).ToString());
                }
            }
            return "";
        }
    }

    class LineToDisplay
    {
        public  string line_Text { get; set; }
        public  object line_id { get; set; }

       
    }
}
