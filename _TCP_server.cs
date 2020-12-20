using System;
using System.Net.Sockets;
using System.Text;
using System.Net;
using System.Threading;

namespace LifeServer
{
    class _TCP_server
    {
        static TcpListener listener;       
        const int port        = 8888;
        int client_thread_ID  = 0;
        bool BC_is_Alive      = true;

        public _TCP_server(){}

        public void Run()
        {
            try
            {
                listener = new TcpListener(IPAddress.Parse("127.0.0.1"), port);
                listener.Start();
                Console.WriteLine("Ожидание подключений...");
                while(BC_is_Alive)
                {
                    string ctID = $"{client_thread_ID}";
                    client_thread_ID++;
                    TcpClient client    = listener.AcceptTcpClient();
                    ClientObj CO        = new ClientObj(client, ctID);
                    Thread clientThread = new Thread(new ThreadStart(CO.ConnectionResolver));
                    clientThread.Start();
                    Console.WriteLine($"Клиент {clientThread.GetHashCode()} подключился");
                }
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            finally
            {
                if(listener != null)
                    listener.Stop();
            }
        }
    }
}