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
        private readonly int curLine;
        private object selectedValue;
        private SQLiteConnection sqlConnection;
        private int currentline;
        private string line_Text;


        public MoreLineDetails(string line_Text, object selectedValue, int currentline, SQLiteConnection sqlConnection)
        {
            InitializeComponent();
            this.selectedValue = selectedValue;
            this.sqlConnection = sqlConnection;
            this.currentline = currentline;
            this.line_Text = line_Text;
            sqlConnection.Open();


            string sql = string.Format("SELECT * FROM Version WHERE version_ID = {0}  ", selectedValue);
            SQLiteCommand cmnd = new SQLiteCommand(sql, sqlConnection);

            SQLiteDataReader reader = cmnd.ExecuteReader();
            while (reader.Read())
            {


                label_lineText.Text = string.Format("{0}", reader["version_Text"].ToString().Trim() );
                label_timeStamp.Text = string.Format("{0}", RelativeDate.relativedate( reader["version_Date"]));
                label_LineNumber.Text = string.Format("{0}", currentline);
                textBox1.Text = string.Format("{0}", reader["version_Comment"] );

            }

            var text = textBox1;
            
            if (textBox1.Text.Equals(""))
            {
                label3.Text = "Below is a space provided for you to write a comment about this line";
            }
            else { label3.Text = "Edit comment below"; }
            
            


            sqlConnection.Close();

        }

        private void button1_Click(object sender, EventArgs e)
        {
            sqlConnection.Open();
            string sql = string.Format("DELETE FROM Version WHERE version_ID = {0}  ", selectedValue);
            SQLiteCommand cmnd = new SQLiteCommand(sql, sqlConnection);
            try { cmnd.ExecuteNonQuery(); } catch (Exception e1) { MessageBox.Show(e1.ToString()); }
            sqlConnection.Close();
            ActiveForm.Close();

            ActiveForm.Close();
            
            MainLines main = new MainLines(currentline, sqlConnection);

            main.Show();

        }

        private void button2_Click(object sender, EventArgs e)
        {
            sqlConnection.Open();
            string sql = string.Format("UPDATE Version SET version_Comment = '{0}' where version_ID ={1}", textBox1.Text, selectedValue);
            SQLiteCommand cmnd = new SQLiteCommand(sql, sqlConnection);
            try { cmnd.ExecuteNonQuery(); } catch (Exception e1) { MessageBox.Show(e1.ToString()); }
            sqlConnection.Close();
            ActiveForm.Close();



        }


        protected override void OnClosing(CancelEventArgs e)
        {
            
            sqlConnection.Open();
            string sql = string.Format("UPDATE Version SET version_Comment = '{0}' where version_ID ={1}", textBox1.Text, selectedValue);
            SQLiteCommand cmnd = new SQLiteCommand(sql, sqlConnection);
            try { cmnd.ExecuteNonQuery(); } catch (Exception e1) { MessageBox.Show(e1.ToString()); }
            sqlConnection.Close();
            
            base.OnClosing(e);
        }

        private void button3_Click(object sender, EventArgs e)
        {
            var fn = showDiffLines.Instance.currentFile;
            var everything = System.IO.File.ReadAllLines(fn);
            everything.SetValue(line_Text.Split('|')[1], currentline);

           System.IO.File.WriteAllLines(fn, everything);
            
        }
    }
}
