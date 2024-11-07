using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace football_game
{
    public partial class PlayerChoice : Form
    {
        public PlayerChoice()
        {
            InitializeComponent();
        }
        
        private void player2_Click(object sender, EventArgs e)
        {
            Form1 form1 = new Form1();
            form1.Show();
        }

        private void player1_Click(object sender, EventArgs e)
        {
            Form1 form1 = new Form1();
            form1.Show();
            //this.Hide();
        }
    }
}
