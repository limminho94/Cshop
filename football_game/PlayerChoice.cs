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
        // 수비 플레이어 2
        private void player2_Click(object sender, EventArgs e)
        {
            int num = 2;
            Form1 form1 = new Form1(num);
            form1.Show();
        }
        // 공격 플레이어 1
        private void player1_Click(object sender, EventArgs e)
        {
            int num = 1;
            Form1 form1 = new Form1(num);
            form1.Show();
        }
    }
}
