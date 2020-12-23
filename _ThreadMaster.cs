using System;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Collections.Generic;
using System.Threading;

namespace LifeServer
{
    public static class ThreadMaster{

        static int[] localCellMap;
        static int generation;
        static int msPerFrame;
        static int fieldWidth;
        static int fieldHeight;
        static _Life life;
        static Thread serverThread;
        static _TCP_server server;
        static object localCellMapLocker = new object();
        static object lifeLocker = new object();

        static void startServer(){
            server       = new _TCP_server();
            serverThread = new Thread(server.Run);
            serverThread.Start();
        }

        static void Live(){
            while(true){
                generation++;
                lock(lifeLocker){
                    life.IterateOnce();
                }
                lock (lifeLocker)
                {
                    lock (localCellMapLocker)
                    {
                        life.GetCellMap(ref localCellMap);
                    }
                }
                ClientThreads.StartEveryone();
                Thread.Sleep(msPerFrame);
            }
        }

        public static int[] ClientGetCells(){
            lock(localCellMapLocker){
                return localCellMap;
            }
        }
        public static void ClientAddCells(int[] cellHashes, byte playerID){
            lock(lifeLocker){
                life.AddStructure(cellHashes, playerID);
            }
        }
        public static int[] getDim(){
            return new int[]{fieldWidth, fieldHeight};
        }

        public static void RUN(int Width = 150, int Height = 140){
            fieldWidth   = Width;
            fieldHeight  = Height;
            generation   = 0;
            life         = new _Life(Width, Height);
            msPerFrame   = 25;
            localCellMap = null;
            startServer();
            Live();
        }
    }
}