using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Diagnostics;
using System.Net.Http.Headers;

namespace server;

class Program
{
    // 공격 플레이어 클라이언트 담는 리스트
    private static List<TcpClient> onepList = new List<TcpClient>();
    // 수비 플레이어 클라이언트 담는 리스트
    private static List<TcpClient> twopList = new List<TcpClient>();
    static int one_cnt = 0;
    static int two_cnt = 0;
    
    static async Task Main(string[] args)
    {
        TcpListener server = new TcpListener(IPAddress.Any, 9900);
        server.Start();
        Console.WriteLine("서버가 시작되었습니다... 클라이언트를 기다리는 중입니다.");

        while (true)
        {
            TcpClient client = await server.AcceptTcpClientAsync();
            Console.WriteLine("클라이언트가 연결되었습니다.");

            // 클라이언트 연결을 비동기적으로 처리
            _ = HandleClientAsync(client);
        }
    }
    
    private static async Task HandleClientAsync(TcpClient client)
    {
        try
        {
            // 클라이언트 연결되면
            using (client)
            {
                // stream 객체 생성
                NetworkStream stream = client.GetStream();
                // 크기 256byte인 buffer 생성
                byte[] buffer = new byte[256];
                int bytesRead;

                // 클라이언트로부터 스트림을 통해 들어오는 data를 buffer에 받고 읽는다.
                while ((bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length)) != 0)
                {
                    // UTF-8 인코딩으로 수신한 메시지를 디코딩
                    string message = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                    Console.WriteLine("수신된 메시지: " + message);
                    string[] messageList = message.Split(',');
                    
                    // welcome , 플레이어 번호(1 또는 2)
                    if (messageList[0] == "welcome")
                    {
                        // 플레이어번호 저장
                        string player = messageList[1];
                        if(player == "1")
                        {
                            // 클라이언트 리스트에 저장
                            onepList.Add(client);
                            // one_cnt - 1p 방번호
                            sendMsg(client, "room_num," + one_cnt.ToString());
                            if (one_cnt < twopList.Count && twopList[one_cnt] != null)
                            {
                                sendMsg(twopList[one_cnt], "ready,");
                                sendMsg(client, "selfReady,");
                            }
                            one_cnt++;

                        }
                        else if(player == "2")
                        {
                            twopList.Add(client);
                            sendMsg(client, "room_num," + two_cnt.ToString());
                            if (two_cnt < onepList.Count && onepList[two_cnt] != null)
                            {
                                sendMsg(onepList[two_cnt], "ready,");
                                sendMsg(client, "selfReady,");
                            }
                            two_cnt++;
                        }
                    }
                    // choice, player, roomNum, myChoice(내가 선택한 타겟방향)
                    else if (messageList[0] == "choice")
                    {
                        // 방번호
                        string r_num = messageList[2];
                        // 1p 플레이어일때
                        if (messageList[1] == "1")
                        {
                            // 2p의 방번호(상대방에게 보낸다는 뜻) , yourChoice + myChoice
                            sendMsg(twopList[int.Parse(r_num)], "yourChoice," + messageList[3]);
                            sendMsg(client, "start");
                        }
                        // 2p 플레이어일때
                        else if(messageList[1] == "2")
                        {
                            // 1p의 방번호(상대방에게 보낸다는 뜻), yourChoice + myChoice
                            sendMsg(onepList[int.Parse(r_num)], "yourChoice," + messageList[3]);
                            sendMsg(client, "start");
                        }
                    }
                }
            }
        }
        catch (Exception e)
        {
            Console.WriteLine("오류: " + e.Message);
        }
        finally
        {
            Console.WriteLine("클라이언트 연결이 종료되었습니다.");
        }
    }
    // 메세지를 보내는 함수
    private static async Task sendMsg(TcpClient client, string message)
    {
        NetworkStream stream = client.GetStream();
        byte[] response = Encoding.UTF8.GetBytes(message);
        await stream.WriteAsync(response, 0, response.Length);
    }
}
