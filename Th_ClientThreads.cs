using System;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Collections.Generic;
using System.Threading;

namespace LifeServer
{

    public static class ClientThreads{
        
        // NB! Lurk for  ConcurrentDictionary, should be much higher perfomance
        static Dictionary<string, EventWaitHandle> waitHandlers = new Dictionary<string, EventWaitHandle>();
        static object locker = new object();

        public static EventWaitHandle GetHandler(string Foo){
            lock(locker){
                return waitHandlers[Foo];
            }
        }
        public static void RemoveHandler(string Foo){
            lock(locker){
                waitHandlers.Remove(Foo);
            }
        }
        public static void AddHandler(string key, EventWaitHandle val){
            lock(locker){
                waitHandlers.Add(key,val);
            }
        }
        public static void StartEveryone(){
            lock(locker){
                foreach(EventWaitHandle wh in waitHandlers.Values){
                    wh.Set();
                }
            }
        }
    }
}