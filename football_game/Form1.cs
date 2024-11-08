using System;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using Microsoft.VisualBasic.ApplicationServices;

namespace football_game
{
    public partial class Form1 : Form
    {
        private TcpClient client;
        private NetworkStream stream;
        List<string> KeeperPosition = new List<string> { "left", "right", "top" };
        List<PictureBox> goalTarget;
        int ballX = 0;
        int ballY = 0;
        int goal = 0;
        int miss = 0;
        int num = 0;
        int gameState = 0;
        string roomNum;
        string state;
        string playerTarget;
        bool aimSet = false;
        bool choice = false;
        string myChoice;
        string yourChoice;
        string player;
        Random random = new Random();

        public Form1(int a)
        {
            // PlayerChoice���� ���� �Ű����� ���� num�� ��´�.
            num = a;
            player = num.ToString();
            InitializeComponent();
            ConnectToServer();
            WriteDataAsync("welcome", player);

            goalTarget = new List<PictureBox> { left, right, top };
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
                    //Debug.WriteLine("�������� ���� ��: " + response);
                    string[] resList = response.Split(',');
                    string anw = resList[0];
                    if (anw == "room_num")
                    {
                        roomNum = resList[1];
                        gameStatelbl.Text = ("���ȣ:" + roomNum);
                    }
                    // ���濡�� ������ �����ߴٰ� �˷��ش�.
                    else if (anw == "ready")
                    {
                        gameStatelbl.Text = ("������ �����Ͽ����ϴ�");
                        // true�� ���� �� �� ����
                        choice = true;
                        gameState = 1;
                    }
                    // ���ο��� ������ �����ߴٰ� �˷��ش�.
                    else if (anw == "selfReady")
                    {
                        gameStatelbl.Text = ("���ӽ���");
                        choice = true;
                        gameState = 1;
                    }
                    // ���濡�� �� Ÿ���� �˷��ش�.
                    else if(anw == "yourChoice")
                    {
                        // �� Ÿ���� yourChoice�� �����Ѵ�.
                        yourChoice = resList[1];
                        if(player == "1")
                        {
                            gameState++;
                            Score(myChoice, yourChoice);
                        }
                        else if(player == "2")
                        {
                            gameState++;
                            Score(yourChoice, myChoice);
                        }
                    }
                    // �ڱ� �ڽſ��� �˷��ش�.
                    else if(anw == "start")
                    {
                        if (player == "1")
                        {
                            gameState++;
                            Score(myChoice, yourChoice);
                            
                        }
                        else if (player == "2")
                        {
                            gameState++;
                            Score(yourChoice, myChoice);
                            
                        }
                    }
                }
                
            }
            catch (Exception ex)
            {
                Debug.WriteLine("�޽����� �޴� �� ������ �߻��߽��ϴ�: " + ex.Message);
            }

        }
        // ������ ������ ������
        private async Task WriteDataAsync(string request, string msg)
        {
            // playerTarget ���� ���ڵ� �� ����Ʈ�迭 data�� ���
            byte[] data = Encoding.UTF8.GetBytes(request + "," + msg);
            // data ������ ����
            await stream.WriteAsync(data, 0, data.Length);
        }

        // ��/���� ����ϴ� �Լ�
        private void Score(string onePlayer, string twoPlayer)
        {
            
           
            if (gameState != 3)
            {
                return;
            }
            
            BallTimer.Start();
            KeeperTimer.Start();
            ChangeGoalKeeperImage(twoPlayer);

            if (onePlayer == "right")
            {
                ballX = -11;
                ballY = 15;
            }
            if (onePlayer == "top")
            {
                ballX = 0;
                ballY = 20;
            }
            if (onePlayer == "left")
            {
                ballX = 7;
                ballY = 8;
            }

            CheckScore();
            
        }
        private async void SetGoalTargetEvent(object sender, EventArgs e)
        {
            // false�� ���� �� �� ���� ����
            if (choice == false)
            {
                gameStatelbl.Text = ("������ ��ٸ��� �ֽ��ϴ�");
                return;
            }
            choice = false;

            var senderObject = (PictureBox)sender;
            //senderObject.BackColor = Color.Beige;
            // ���� ������ Ÿ�ٹ���
            myChoice = senderObject.Tag.ToString();
            WriteDataAsync("choice", player + "," + roomNum + "," + myChoice);

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
            // 1p�� 2p Ÿ���� �����Ҷ�
            if (myChoice == yourChoice)
            {
                if(player == "1")
                {
                    miss++;
                    lblMissed.Text = "����: " + miss;
                    gameStatelbl.Text = ("�� �̱������� ���ϳ���");
                }
                if(player == "2")
                {
                    goal++;
                    lblScore.Text = "����: " + goal;
                    gameStatelbl.Text = ("�� ������ ���� ���Ƴ��ϴ�");
                }
            }
            else
            {
                if (player == "1")
                {
                    goal++;
                    lblScore.Text = "����: " + goal;
                    gameStatelbl.Text = ("�̱��� �� ģ�� �����̳׿�");
                }
                if (player == "2")
                {
                    miss++;
                    lblMissed.Text = "����: " + miss;
                    gameStatelbl.Text = ("�Ÿ�ȣ Ű�� ���� ���������׿�");
                }
            }
            
            // Ÿ�� ���� �ʱ�ȭ
            choice = true;
            // ���ӻ��� 1�� �ʱ�ȭ
            gameState = 1;
            if(player == "1" && goal == 5 )
            {
                gameStatelbl.Text = ("���ϵ帳�ϴ� 1p �¸�!");
            }
            else if (player == "2" && goal == 5)
            {
                gameStatelbl.Text = ("���ϵ帳�ϴ� 2p �¸�!");
            }
            else if (player == "2" && miss == 5)
            {
                gameStatelbl.Text = ("�ƽ����� ������ȸ��..");
            }
            else if (player == "1" && miss == 5)
            {
                gameStatelbl.Text = ("�ƽ����� ������ȸ��..");
            }
        }

        private void ChangeGoalKeeperImage(string twoPlayer_image)
        {
            KeeperTimer.Start();
            state = twoPlayer_image;
            switch (twoPlayer_image)
            {
                case "left":
                    goalKeeper.Image = Properties.Resources.left_save_small;
                    break;
                case "right":
                    goalKeeper.Image = Properties.Resources.right_save_small;
                    break;
                case "top":
                    goalKeeper.Image = Properties.Resources.top_save_small;
                    break;
            }
        }

        
    }
}
