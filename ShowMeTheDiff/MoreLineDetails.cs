using System;
using System.ComponentModel;
using System.Data.SQLite;
using System.Windows.Forms;

namespace ShowMeTheDiff
{   //Screen with more line details
    public partial class MoreLineDetails : Form
    {
        private readonly int curLine;
        private object selectedValue;
        private SQLiteConnection sqlConnection;
        private int currentline;
        private string line_Text;

        //handle information such as the date, line, line number and comment
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
                label_LineNumber.Text = string.Format("{0}", (currentline +1));
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
        //control for delete button to delete a line version
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
        //handle the ok event and updates the comment in the database
        private void button2_Click(object sender, EventArgs e)
        {
            sqlConnection.Open();
            string sql = string.Format("UPDATE Version SET version_Comment = '{0}' where version_ID ={1}", textBox1.Text, selectedValue);
            SQLiteCommand cmnd = new SQLiteCommand(sql, sqlConnection);
            try { cmnd.ExecuteNonQuery(); } catch (Exception e1) { MessageBox.Show(e1.ToString()); }
            sqlConnection.Close();
            ActiveForm.Close();



        }

        //handles closing event and does the same thing as the OK button
        protected override void OnClosing(CancelEventArgs e)
        {
            
            sqlConnection.Open();
            string sql = string.Format("UPDATE Version SET version_Comment = '{0}' where version_ID ={1}", textBox1.Text, selectedValue);
            SQLiteCommand cmnd = new SQLiteCommand(sql, sqlConnection);
            try { cmnd.ExecuteNonQuery(); } catch (Exception e1) { MessageBox.Show(e1.ToString()); }
            sqlConnection.Close();
            
            base.OnClosing(e);
        }
        //Use this line instead from the more line details screen
        private void button3_Click(object sender, EventArgs e)
        {
            var fn = showDiffLines.Instance.currentFile;
            var everything = System.IO.File.ReadAllLines(fn);
            everything.SetValue(line_Text.Split('|')[1], currentline);

           System.IO.File.WriteAllLines(fn, everything);
            
        }
    }
}
