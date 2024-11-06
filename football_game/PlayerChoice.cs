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
        private TcpClient client;
        private NetworkStream stream;
        public PlayerChoice()
        {
            InitializeComponent();
            ConnectToServer();
        }
        private async void ConnectToServer()
        {
            try
            {
                client = new TcpClient();
                // 서버 IP 주소와 포트
                await client.ConnectAsync("127.0.0.1", 9900);
                stream = client.GetStream();
                //MessageBox.Show("서버에 연결되었습니다.");
            }
            catch (Exception ex)
            {
                MessageBox.Show("서버에 연결할 수 없습니다: " + ex.Message);
            }
        }

        private void player2_Click(object sender, EventArgs e)
        {

        }

        private void player1_Click(object sender, EventArgs e)
        {
            Form1 form1 = new Form1();
            form1.Show();
            //this.Hide();
        }
    }
}
