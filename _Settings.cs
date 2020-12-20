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

    static class Accounts
    {
        private class Account
        {
            public readonly string login;
            public readonly string pass;
            public readonly byte[] color; // array size of 4: ARGB

            public Account(string login, string pass, byte[] color, int ID)
            {
                this.login = login;
                this.pass  = pass;
                this.color = color;
            }
            public Account(string login, string pass, int color, int ID)
            {
                this.login = login;
                this.pass  = pass;
                this.color = color.ToByte();
            }
            public Account(string login, string pass, int ID)
            {
                this.login = login;
                this.pass  = pass;
            }
        }
        private static Dictionary<string, Account> Accs = new Dictionary<string, Account>
            {
                { "CCorax",      new Account("CCorax",      "CCorax",      new byte[]{ 255, 0,   255, 0  }, 0) }, // Green
                { "Mephisto",    new Account("Mephisto",    "Mephisto",    new byte[]{ 255, 255, 0,   0  }, 1) }, // Red
                { "PlagueEater", new Account("PlagueEater", "PlagueEater", new byte[]{ 255, 255, 168, 0  }, 2) }, // Orange
                { "Nikolaj",     new Account("Nikolaj",     "Nikolaj",     new byte[]{ 255, 0,   255, 255}, 3) }, // Teal
                { "Alena",       new Account("Alena",       "Alena",       new byte[]{ 255, 200, 216, 240}, 4) }, // Gray
                { "Mozenrath",   new Account("Mozenrath",   "Mozenrath",   new byte[]{ 255, 255, 0,   192}, 5) }, // Purple
                { "Deadline",    new Account("Deadline",    "Deadline",    new byte[]{ 255, 0,   0,   255}, 6) }, // Blue
                { "Egor",        new Account("Egor",        "Egor",        new byte[]{ 255, 255, 255, 96 }, 7) }  // Yellow
            };
        public static byte[] GetByteColor(string username)
        {
            if (Accs.ContainsKey(username))
            {
                return Accs[username].color;
            }
            else
                return null;
        }
        public static int GetIntColor(string username)
        {
            if (Accs.ContainsKey(username))
            {
                return Accs[username].color.ToInt();
            }
            else
                return 0;
        }
        public static bool Authorize(string username, string pass)
        {
            if (Accs.ContainsKey(username))
            {
                if (Accs[username].pass == pass)
                    return true;
                else
                    return false;
            }
            else
                return false;
        }

    }
    public class NetCodes
    {
        // Singletone
        private static Dictionary<string, int> dict;
        private static NetCodes instance;

        private NetCodes()
        {
            dict = new Dictionary<string, int>
            {        
                { "wrongConnectionCode",    -1 },
                { "dialogue",                1 },
                { "connectionSuccessful",    2 },
                { "getFieldDimensions",      3 },
                { "getStream",               4 },
                { "authorizationRequest",    5 },
                { "authorizationBegin",      6 },
                { "authorizationSuccessful", 7 },
                { "authorizationFailed",     8 },
                { "struct",                  9 },
                { "acceptStruct",            10},
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
        public static byte[] ToByte(this int i)
        {
            int[] foo = new int[] { i };
            byte[] bar = new byte[sizeof(int)];
            Buffer.BlockCopy(foo, 0, bar, 0, sizeof(int));
            return bar;
        }
        public static int ToInt(this byte[] bar)
        {
            int[] foo  = new int[] { sizeof(int) };
            Buffer.BlockCopy(bar, 0, foo, 0, sizeof(int));
            return foo[0];
        }
        public static void Print(this int[] arr)
        {
            string S = "[ ";
            foreach (int i in arr)
                S += $"{i} ";
            S += "]";
            Console.WriteLine(S);
        }
    }
}