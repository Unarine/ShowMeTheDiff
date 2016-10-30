using System;
using System.Windows.Forms;
using System.Data.SQLite;

namespace ShowMeTheDiff
{   //main lines page, lists the different versions of the line 

    public partial class MainLines : Form
    {
        private int curLine;
        private SQLiteConnection sqlConnection;



        public MainLines(int curLine, SQLiteConnection sqlConnection)
        {
            InitializeComponent();
            
            this.curLine = curLine;
            
            this.sqlConnection = sqlConnection;
            sqlConnection.Open();
            listBox1.Items.Clear();
             listBox1.DisplayMember = "line_Text";
            
            listBox1.ValueMember = "line_id";
            listBox1.ValueMember = "currLineNumber";



            string sql = string.Format("SELECT * FROM Version INNER JOIN Line ON Version.line_id = Line.line_ID WHERE Line_Number = {0} ORDER BY version_Date DESC ", curLine);
            SQLiteCommand cmd3 = new SQLiteCommand(sql, sqlConnection);

            SQLiteDataReader reader = cmd3.ExecuteReader();
            var y = listBox1.Items.Count;

            if (reader.Read() == false && listBox1.Items.Count < 1)
            {
                label1.Text = "No previous versions of this line";
            }
            else {
                label1.Text = "Click on line to see more line details";
                string todisplay = string.Format("{0} \t| {1}", RelativeDate.relativedate(reader["version_Date"]), reader["version_Text"]);
                listBox1.Items.Add(new LineToDisplay { line_Text = todisplay.Trim(), line_id = reader["version_ID"], currLineNumber = curLine });
            }
            while (reader.Read())
            {

                string todisplay = string.Format("{0} \t| {1}", RelativeDate.relativedate(reader["version_Date"]), reader["version_Text"]);
                listBox1.Items.Add(new LineToDisplay { line_Text = todisplay.Trim(), line_id = reader["version_ID"], currLineNumber = curLine });
                
            

            }
            sqlConnection.Close();

            listBox1.Click += OnListBoxItemClick;
        }

        private void OnListBoxItemClick(object sender, EventArgs e)
        {
            MoreLineDetails md = new MoreLineDetails((listBox1.SelectedItem as LineToDisplay).line_Text, (listBox1.SelectedItem as LineToDisplay).line_id, (listBox1.SelectedItem as LineToDisplay).currLineNumber, MyVSPackagePackage.SqlConnection);           
            md.Show();
        }


        

        
    }
    //properties to be made available in the more detals class                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                   
    class LineToDisplay
    {
        public  string line_Text { get; set; }
        public  object line_id { get; set; }
        public int currLineNumber { get; set; }

    }


}
