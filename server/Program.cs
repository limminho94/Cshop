using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Diagnostics;

namespace server;

class Program
{
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

                    string num = message.Substring(0,1);
                    string tar = message.Substring(1);

                    // 인코딩하여 다시 서버에 전송
                    byte[] response = Encoding.UTF8.GetBytes(message);
                    await stream.WriteAsync(response, 0, response.Length);

                    // 바이트는 콘솔창에 출력이 되지않아 16진수로 출력
                    foreach (var b in response)
                    {
                        Console.Write($"16진수 : {b:X2} ");
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
}
