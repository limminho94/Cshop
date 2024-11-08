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
            // PlayerChoice에서 받은 매개변수 값을 num에 담는다.
            num = a;
            player = num.ToString();
            InitializeComponent();
            ConnectToServer();
            WriteDataAsync("welcome", player);

            goalTarget = new List<PictureBox> { left, right, top };
        }
        // 서버연결
        private async void ConnectToServer()
        {
            try
            {
                client = new TcpClient();
                // 서버 IP 주소와 포트
                await client.ConnectAsync("10.10.20.116", 9900);
                stream = client.GetStream();
                await ReceiveDataAsync();

            }
            catch (Exception ex)
            {
                MessageBox.Show("서버에 연결할 수 없습니다: " + ex.Message);
            }
        }
        // 서버에서 오는 데이터 받기
        private async Task ReceiveDataAsync()
        {
            try
            {
                byte[] buffer = new byte[256];
                int bytesRead;
                while ((bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length)) != 0)
                {
                    string response = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                    //Debug.WriteLine("서버에서 받은 값: " + response);
                    string[] resList = response.Split(',');
                    string anw = resList[0];
                    if (anw == "room_num")
                    {
                        roomNum = resList[1];
                        gameStatelbl.Text = ("방번호:" + roomNum);
                    }
                    // 상대방에게 본인이 입장했다고 알려준다.
                    else if (anw == "ready")
                    {
                        gameStatelbl.Text = ("상대방이 입장하였습니다");
                        // true가 공을 찰 수 있음
                        choice = true;
                        gameState = 1;
                    }
                    // 본인에게 상대방이 입장했다고 알려준다.
                    else if (anw == "selfReady")
                    {
                        gameStatelbl.Text = ("게임시작");
                        choice = true;
                        gameState = 1;
                    }
                    // 상대방에게 내 타겟을 알려준다.
                    else if(anw == "yourChoice")
                    {
                        // 내 타겟을 yourChoice에 저장한다.
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
                    // 자기 자신에게 알려준다.
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
                Debug.WriteLine("메시지를 받는 중 오류가 발생했습니다: " + ex.Message);
            }

        }
        // 서버에 데이터 보내기
        private async Task WriteDataAsync(string request, string msg)
        {
            // playerTarget 값을 인코딩 후 바이트배열 data에 담기
            byte[] data = Encoding.UTF8.GetBytes(request + "," + msg);
            // data 서버에 전송
            await stream.WriteAsync(data, 0, data.Length);
        }

        // 득/실점 계산하는 함수
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
            // false는 공을 찰 수 없는 상태
            if (choice == false)
            {
                gameStatelbl.Text = ("상대방을 기다리고 있습니다");
                return;
            }
            choice = false;

            var senderObject = (PictureBox)sender;
            //senderObject.BackColor = Color.Beige;
            // 내가 선택한 타겟방향
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
            // 1p와 2p 타겟이 동일할때
            if (myChoice == yourChoice)
            {
                if(player == "1")
                {
                    miss++;
                    lblMissed.Text = "실점: " + miss;
                    gameStatelbl.Text = ("아 이광영선수 뭐하나요");
                }
                if(player == "2")
                {
                    goal++;
                    lblScore.Text = "득점: " + goal;
                    gameStatelbl.Text = ("아 조현우 선수 막아냅니다");
                }
            }
            else
            {
                if (player == "1")
                {
                    goal++;
                    lblScore.Text = "득점: " + goal;
                    gameStatelbl.Text = ("이광영 이 친구 물건이네요");
                }
                if (player == "2")
                {
                    miss++;
                    lblMissed.Text = "실점: " + miss;
                    gameStatelbl.Text = ("신명호 키퍼 몸이 안좋나보네요");
                }
            }
            
            // 타겟 선택 초기화
            choice = true;
            // 게임상태 1로 초기화
            gameState = 1;
            if(player == "1" && goal == 5 )
            {
                gameStatelbl.Text = ("축하드립니다 1p 승리!");
            }
            else if (player == "2" && goal == 5)
            {
                gameStatelbl.Text = ("축하드립니다 2p 승리!");
            }
            else if (player == "2" && miss == 5)
            {
                gameStatelbl.Text = ("아쉬워요 다음기회에..");
            }
            else if (player == "1" && miss == 5)
            {
                gameStatelbl.Text = ("아쉬워요 다음기회에..");
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
