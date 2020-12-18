using System;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Collections.Generic;
using System.Threading;
using System.Diagnostics;

namespace LifeServer
{
    class Program
    {
        static void Main(string[] args)
        {
            ThreadMaster.RUN();
        }
    }
}