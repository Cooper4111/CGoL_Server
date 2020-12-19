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

            public Account(string login, string pass, byte[] color)
            {
                this.login = login;
                this.pass = pass;
                this.color = color;
            }
            public Account(string login, string pass, int color)
            {
                this.login = login;
                this.pass = pass;
                this.color = color.ToByte();
            }
            public Account(string login, string pass)
            {
                this.login = login;
                this.pass = pass;
            }

        }
        private static Dictionary<string, Account> Accs = new Dictionary<string, Account>
            {
                { "CCorax",      new Account("CCorax",      "CCorax",      new byte[]{0, 0,   255, 0  }) },
                { "Mephisto",    new Account("Mephisto",    "Mephisto",    new byte[]{0, 255, 255, 255}) },
                { "PlagueEater", new Account("PlagueEater", "PlagueEater", new byte[]{0, 168, 168, 0  }) },
                { "Nikolaj",     new Account("Nikolaj",     "Nikolaj",     new byte[]{0, 0,   255, 255}) },
                { "Alena",       new Account("Alena",       "Alena",       new byte[]{0, 168, 168, 168}) },
                { "Mozenrath",   new Account("Mozenrath",   "Mozenrath",   new byte[]{0, 255,   0, 255}) },
                { "Deadline",    new Account("Deadline",    "Deadline",    new byte[]{0, 255, 255, 64 }) },
                { "Egor",        new Account("Egor",        "Egor",        new byte[]{0, 255, 168, 128}) }
            };

        public static byte[] GetColor(string username)
        {
            if (Accs.ContainsKey(username))
            {
                return Accs[username].color;
            }
            else
                return null;
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
        public static byte[] ToByte(this int i)
        {
            int[] foo = new int[] { i };
            byte[] bar = new byte[sizeof(int)];
            Buffer.BlockCopy(foo, 0, bar, 0, sizeof(int));
            return bar;
        }
    }
}