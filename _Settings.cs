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
            public readonly byte ID;

            public Account(string login, string pass, byte[] color, byte ID)
            {
                this.login = login;
                this.pass  = pass;
                this.color = color;
                this.ID = ID;
            }
            public Account(string login, string pass, int color, byte ID)
            {
                this.login = login;
                this.pass  = pass;
                this.color = color.ToByte();
                this.ID = ID;
            }
            public Account(string login, string pass, byte ID)
            {
                this.login = login;
                this.pass  = pass;
                this.ID = ID;
            }
        }
        private static Dictionary<string, Account> Accs = new Dictionary<string, Account>
            {
                // Zero ID is reserved for "dead" cell of black color;
                { "CCorax",      new Account("CCorax",      "CCorax",      new byte[]{ 255, 0,   255, 0  },   1) }, // Green
                { "Mephisto",    new Account("Mephisto",    "Mephisto",    new byte[]{ 255, 255, 0,   0  },   2) }, // Red
                { "PlagueEater", new Account("PlagueEater", "PlagueEater", new byte[]{ 255, 255, 168, 0  },   3) }, // Orange
                { "Nikolaj",     new Account("Nikolaj",     "Nikolaj",     new byte[]{ 255, 0,   255, 255},   4) }, // Teal
                { "Alena",       new Account("Alena",       "Alena",       new byte[]{ 255, 200, 216, 240},   5) }, // Gray
                { "Mozenrath",   new Account("Mozenrath",   "Mozenrath",   new byte[]{ 255, 255, 0,   192},   6) }, // Purple
                { "Deadline",    new Account("Deadline",    "Deadline",    new byte[]{ 255, 0,   0,   255},   7) }, // Blue
                { "Egor",        new Account("Egor",        "Egor",        new byte[]{ 255, 255, 255, 96 },   8) }, // Yellow
                { "Creep",       new Account("Creep",       "Creep",       new byte[]{ 255, 255, 255, 255}, 255) }  // White
            };
        public static Dictionary<int, string> ID2username = new Dictionary<int, string>
            {
                // Zero ID is reserved for "dead" cell of black color;
                { 1, "CCorax"       },
                { 2, "Mephisto"     },
                { 3, "PlagueEater"  },
                { 4, "Nikolaj"      },
                { 5, "Alena"        },
                { 6, "Mozenrath"    },
                { 7, "Deadline"     },
                { 8, "Egor"         },
                { 255, "Creep"        }
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
        public static byte GetPlayerID(string username)
        {
            if (Accs.ContainsKey(username))
            {
                return Accs[username].ID;
            }
            else
                return 0;
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
        /// <returns>Sum of byte values in array</returns>
        public static int Sum(this byte[] bar)
        {
            int result = 0;
            foreach(byte b in bar)
            {
                result += b;
            }
            return result;
        }
        /// <returns>true if any byte in array is > 0</returns>
        public static bool NotEmptyN(this byte[] bar, int N)
        {
            int m = 0;
            int i = 0;
            while(m < N && i < bar.Length)
            {
                if (bar[i] > 0)
                    m++;
                i++;
            }
            if (m + 1 == N)
                return true;
            else
                return false;
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