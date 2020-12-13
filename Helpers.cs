using System;
using System.IO;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Collections.Generic;

namespace LifeServer
{
    public class GameSettings
    {
        // singletone with settings
    }
    public class ServerSettings
    {
        // singletone with settings
    }
    public class NetCodes
    {

        private static Dictionary<string, int> dict;
        private static NetCodes instance;

        private NetCodes()
        {
            dict = new Dictionary<string, int>
            {
                { "dialogue",             1 },
                { "connectionSuccessful", 2 },
                { "getFieldDimensions",   3 },
                { "getStream",            4 },
                { "login",                5 },
                { "loginSuccesscful",     6 },
                { "struct",               7 },
                { "acceptStruct",         8 }
            };
        }

        public static NetCodes getInst()
        {
            if (instance == null)
            {
                instance = new NetCodes();
            }
            return instance;
        }

        public int this[string key]
        {
            get =>
                dict[key];
            private set =>
                dict.Add(key, value);
        }
    }

    public static class Helpers
    {
        public static byte ToByte(this uint i)
        {
            return Convert.ToByte(i);
        }
    }
}