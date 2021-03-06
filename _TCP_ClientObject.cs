using System;
using System.Net.Sockets;
using System.Text;
using System.Net;
using System.Threading;
using System.Runtime.Serialization.Formatters.Binary;
using System.Collections.Generic;
using System.Diagnostics;

#pragma warning disable SYSLIB0011

namespace LifeServer
{
    public class ClientObj
    {
        TcpClient client;
        EventWaitHandle  wh;
        BinaryFormatter formatter;
        string ID;
        byte playerID;
        NetCodes NetCode;

        public ClientObj(TcpClient tcpClient, string ID)
        {
            formatter = new BinaryFormatter();
            this.wh   = new AutoResetEvent(false);
            client    = tcpClient;
            NetCode   = NetCodes.getInst();
            this.ID   = ID;
        }
        public void ConnectionResolver(){
            NetworkStream stream = null;
            int msg = 0; 
            try
            {
                Console.WriteLine($"Ожидание сообщений от клиента {Thread.CurrentThread.GetHashCode()}...");
                stream  = client.GetStream();
                msg = (int)formatter.Deserialize(stream);
                Console.WriteLine($"Сообщение\'{msg}\' от клиента {Thread.CurrentThread.GetHashCode()}...");
                if(msg == NetCode["dialogue"])
                {
                    Console.WriteLine($"Клиент {Thread.CurrentThread.GetHashCode()} запросил диалог");
                    formatter.Serialize(stream, NetCode["connectionSuccessful"]);
                    Dialogue(stream);
                }
                if(msg == NetCode["getStream"])
                {
                    Console.WriteLine($"Клиент {Thread.CurrentThread.GetHashCode()} запросил стрим");
                    StreamField(stream);
                }
                else{
                    formatter.Serialize(stream, NetCode["wrongConnectionCode"]);
                }
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            finally
            {
                if (stream != null)
                    stream.Close();
                if (client != null)
                    client.Close();
            }
        }

        void StreamField(NetworkStream stream)
        {
            if (stream == null) 
                return;
            ClientThreads.AddHandler(this.ID, wh);
            bool isAlive = true;
            int[] cells;
            byte[] data;
            while (isAlive)
            {
                wh.WaitOne();
                cells = ThreadMaster.ClientGetCells();
                // sending metadata
                data = new byte[sizeof(int)];
                Buffer.BlockCopy(new int[]{cells.Length}, 0, data, 0, data.Length);
                stream.Write(data, 0, data.Length);

                // sending data
                data = new byte[cells.Length * sizeof(int)];
                Buffer.BlockCopy(cells, 0, data, 0, data.Length);
                stream.Write(data, 0, data.Length);

            }
        }

        void Dialogue(NetworkStream stream){
            bool authorized = false;
            if(stream == null)
                return;
            int msg = 0;
            while (true)
            {
                if(!stream.DataAvailable)
                    continue;
                msg = (int)formatter.Deserialize(stream);
                if (msg == NetCode["authorizationRequest"])
                {
                    formatter.Serialize(stream, NetCode["authorizationBegin"]);
                    byte[] byteLoginPass  = new byte[128];
                    StringBuilder builder = new StringBuilder();
                    int bytes = 0;
                    do
                    {
                        bytes = stream.Read(byteLoginPass, 0, byteLoginPass.Length);
                        builder.Append(Encoding.Unicode.GetString(byteLoginPass, 0, bytes));
                    }
                    while (stream.DataAvailable);
                    string loginPass = builder.ToString();
                    if (Accounts.Authorize(loginPass.Split(' ')[0], loginPass.Split(' ')[1]))
                    {
                        authorized = true;
                        playerID = Accounts.GetPlayerID(loginPass.Split(' ')[0]);
                        Console.WriteLine($"{loginPass} authorized!");
                        formatter.Serialize(stream, NetCode["authorizationSuccessful"]);
                    }
                    else
                    {
                        formatter.Serialize(stream, NetCode["authorizationFailed"]);
                    }
                    
                }
                if (authorized)
                {
                    if (msg == NetCode["getFieldDimensions"])
                    {
                        Console.WriteLine($"Клиент {Thread.CurrentThread.GetHashCode()} запросил размеры поля");
                        Console.WriteLine($"Поле: {ThreadMaster.getDim()[0]}x{ThreadMaster.getDim()[1]}");
                        formatter.Serialize(stream, ThreadMaster.getDim());
                        Console.WriteLine($"Размеры отправлены.");
                    }

                    if (msg == NetCode["struct"])
                    {
                        formatter.Serialize(stream, NetCode["acceptStruct"]);
                        int[] lifeStructure = (int[])formatter.Deserialize(stream);
                        ThreadMaster.ClientAddCells(lifeStructure, playerID);
                    }
                }
            }
        }
    }
}   