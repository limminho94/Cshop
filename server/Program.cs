using System.Net.Sockets;
using System.Net;
using System.Text;

namespace server;

class Program
{
    static async Task Main(string[] args)
    {
        // 
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
            using (client)
            {
                NetworkStream stream = client.GetStream();
                byte[] buffer = new byte[256];
                int bytesRead;

                while ((bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length)) != 0)
                {
                    // UTF-8 인코딩으로 수신한 메시지를 디코딩
                    string message = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                    Console.WriteLine("수신된 메시지: " + message);

                    // UTF-8 인코딩으로 응답 메시지 전송
                    byte[] response = Encoding.UTF8.GetBytes("서버: 메시지를 받았습니다.");
                    await stream.WriteAsync(response, 0, response.Length);
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
