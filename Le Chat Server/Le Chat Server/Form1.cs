using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Web;

namespace Le_Chat_Server
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            serverClass.logList = log;
            List<string> x = new List<string>();
            ListBox l = new ListBox();
        }

        private void Form1_Shown(object sender, EventArgs e)
        {
            serverClass.setup();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            foreach(var i in serverClass.userList.Values)
            {
                MessageBox.Show(i.name);
            }
        }

        
    }
}
