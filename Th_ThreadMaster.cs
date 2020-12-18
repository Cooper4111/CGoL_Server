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
        static Life life;
        static Thread serverThread;
        static TCP_Server server;
        static object localCellMapLocker = new object();
        static object lifeLocker = new object();

        static void startServer(){
            server       = new TCP_Server();
            serverThread = new Thread(server.Run);
            serverThread.Start();
        }

        static void Live(){
            while(true){
                generation++;
                lock(lifeLocker){
                    life.iterateOnce();
                }
                lock(localCellMapLocker){
                    life.getCellMap(ref localCellMap);
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
        public static void ClientAddCells(int[] cellHashes){
            lock(lifeLocker){
                life.addStructure(cellHashes);
            }
        }
        public static int[] getDim(){
            return new int[]{fieldWidth, fieldHeight};
        }

        public static void RUN(int Width = 150, int Height = 140){
            fieldWidth   = Width;
            fieldHeight  = Height;
            generation   = 0;
            life         = new Life(Width, Height);
            msPerFrame   = 55;
            localCellMap = null;
            startServer();
            Live();
        }
    }
}