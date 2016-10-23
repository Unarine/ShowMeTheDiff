using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SQLite;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ShowMeTheDiff
{
    public partial class MoreLineDetails : Form
    {
        private object selectedValue;
        private SQLiteConnection sqlConnection;
        
        public MoreLineDetails()
        {
            InitializeComponent();
        }

        public MoreLineDetails(object selectedValue, SQLiteConnection sqlConnection)
        {
            InitializeComponent();
            this.selectedValue = selectedValue;
            this.sqlConnection = sqlConnection;
            sqlConnection.Open();


            string sql = string.Format("SELECT * FROM Version WHERE version_ID = {0}  ", selectedValue);
            SQLiteCommand cmnd = new SQLiteCommand(sql, sqlConnection);

            SQLiteDataReader reader = cmnd.ExecuteReader();
            while (reader.Read())
            {


                label1.Text = string.Format("{0}", reader["version_Text"] );
                //string todisplay = string.Format("{0} | {1}", relativedate(reader["version_Date"]), reader["version_Text"]);
                //string l;
               // listBox1.Items.Add(new LineToDisplay { line_Text = todisplay, line_id = reader["version_ID"] });


            }


            //label1.Text = selectedValue.ToString();


            sqlConnection.Close();

        }
    }
}
