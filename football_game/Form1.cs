using System;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;

namespace football_game
{
    public partial class Form1 : Form
    {
        private TcpClient client;
        private NetworkStream stream;
        List<string> KeeperPosition = new List<string> { "left", "right", "top"};
        List<PictureBox> goalTarget;
        int ballX = 0;
        int ballY = 0;
        int goal = 0;
        int miss = 0;
        int num = 0;
        string state;
        string playerTarget;
        bool aimSet = false;
        Random random = new Random();
        
        public Form1(int a)
        {
            // PlayerChoice���� ���� �Ű����� ���� num�� ��´�.
            num = a;
            InitializeComponent();
            ConnectToServer();
            goalTarget = new List<PictureBox> { left, right, top};
        }
        // ��������
        private async void ConnectToServer()
        {
            try
            {
                client = new TcpClient();
                // ���� IP �ּҿ� ��Ʈ
                await client.ConnectAsync("10.10.20.116", 9900);
                stream = client.GetStream();
                await ReceiveDataAsync();

            }
            catch (Exception ex)
            {
                MessageBox.Show("������ ������ �� �����ϴ�: " + ex.Message);
            }
        }
        // �������� ���� ������ �ޱ�
        private async Task ReceiveDataAsync()
        {
            try
            {
                byte[] buffer = new byte[256];
                int bytesRead;
                while ((bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length)) != 0)
                {
                    string response = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                    Debug.WriteLine("�������� ���� ��: " + response);
                }

                
            }
            catch (Exception ex)
            {
                Debug.WriteLine("�޽����� �޴� �� ������ �߻��߽��ϴ�: " + ex.Message);
            }
        }
        private async void SetGoalTargetEvent(object sender, EventArgs e)
        {
            if (aimSet == true) { return; }

            if (num == 1)
            {
                BallTimer.Start();
                MessageBox.Show("������ ��ٸ��� �ֽ��ϴ�.");
            }
            else if (num == 2)
            {
                //BallTimer.Start();
                KeeperTimer.Start();
                ChangeGoalKeeperImage();
            }

            var senderObject = (PictureBox)sender;
            senderObject.BackColor = Color.Beige;
            
            if (senderObject.Tag.ToString() == "right")
            {
                ballX = -11;
                ballY = 15;
                playerTarget = senderObject.Tag.ToString();
                aimSet = true;
            }
            if (senderObject.Tag.ToString() == "top")
            {
                ballX = 0;
                ballY = 20;
                playerTarget = senderObject.Tag.ToString();
                aimSet = true;
            }
            if (senderObject.Tag.ToString() == "left")
            {
                ballX = 7;
                ballY = 8;
                playerTarget = senderObject.Tag.ToString();
                aimSet = true;
            }

            Debug.WriteLine(num + playerTarget);

            // playerTarget ���� ���ڵ� �� ����Ʈ�迭 data�� ���
            string user = num.ToString();
            byte[] data = Encoding.UTF8.GetBytes(user + playerTarget);
            // data ������ ����
            await stream.WriteAsync(data, 0, data.Length);

            CheckScore();
        }

        private void KeeperTimerEvent(object sender, EventArgs e)
        {
            switch (state)
            {
                case "left":
                    goalKeeper.Left -= 6;
                    goalKeeper.Top = 204;
                    break;
                case "right":
                    goalKeeper.Left += 6;
                    goalKeeper.Top = 204;
                    break;
                case "top":
                    goalKeeper.Top -= 6;
                    break;
            }

            foreach (PictureBox x in goalTarget)
            {
                if (goalKeeper.Bounds.IntersectsWith(x.Bounds))
                {
                    KeeperTimer.Stop();
                    goalKeeper.Location = new Point(418, 169);
                    goalKeeper.Image = Properties.Resources.stand_small;
                }
            }
        }
        private void BallTimerEvent(object sender, EventArgs e)
        {
            football.Left -= ballX;
            football.Top -= ballY;

            foreach (PictureBox x in goalTarget)
            {
                if (football.Bounds.IntersectsWith(x.Bounds))
                {
                    football.Location = new Point(430, 500);
                    ballX = 0;
                    ballY = 0;
                    aimSet = false;
                    BallTimer.Stop();
                }
            }
        }

        private void CheckScore()
        {
            if (state == playerTarget)
            {
                miss++;
                lblMissed.Text = "����: " + miss;
            }
            else
            {
                goal++;
                lblScore.Text = "����: " + goal;
            }
        }

        private void ChangeGoalKeeperImage()
        {
            KeeperTimer.Start();
            int i = random.Next(0, KeeperPosition.Count);
            state = KeeperPosition[i];

            switch (i)
            {
                case 0:
                    goalKeeper.Image = Properties.Resources.left_save_small;
                    break;
                case 1:
                    goalKeeper.Image = Properties.Resources.right_save_small;
                    break;
                case 2:
                    goalKeeper.Image = Properties.Resources.top_save_small;
                    break;
            }
        }

        //private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        //{
        //    // â�� ���� �� ������ ����
        //    stream?.Close();
        //    client?.Close();
        //}
    }
}
