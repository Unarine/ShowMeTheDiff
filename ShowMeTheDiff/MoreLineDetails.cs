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
            
            
            label1.Text = selectedValue.ToString();
        }
    }
}
