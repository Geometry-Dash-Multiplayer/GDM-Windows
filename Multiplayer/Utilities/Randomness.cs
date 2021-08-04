using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;

namespace Multiplayer.Utilities
{
    public static class Randomness
    {
        public static Random rand = new Random(DateTime.Now.Millisecond + DateTime.Now.Second + DateTime.Now.Minute);

        public static string[] quotes = new string[] { 
            "Not on 2.2!",
            "Please donate, servers aren't cheap.",
            "Go to discord.link/alizer!",
            "UDP is hell.",
            "Adafcaefc is god.",
            "Purely C#!",
            "Problems? discord.link/alizer",
        };
        public static string[] serverdesc = new string[] {
            "Pick one, preferably the one nearest to you...",
        };
        public static string RandomQuote()
        {
            return "";
                // quotes[rand.Next(0, quotes.Length)];
        }
        public static string RandomServerQuote()
        {
            return serverdesc[rand.Next(0, serverdesc.Length)];
        }
        public static byte[] RandomBytes(int length = 8)
        {
            byte[] b = new byte[length];
            rand.NextBytes(b);
            return b;
        }
        public static int RandomFunnyNumber()
        {
            var funnynumbers = new int[] { 69, 420, 1337, 69, 911 };
            return funnynumbers[rand.Next(0, funnynumbers.Length)];
        }
        public static int GetAvailablePort()
        {
            int startingPort = rand.Next(1000,65535);
            IPEndPoint[] endPoints;
            List<int> portArray = new List<int>();

            IPGlobalProperties properties = IPGlobalProperties.GetIPGlobalProperties();

            //getting active connections
            TcpConnectionInformation[] connections = properties.GetActiveTcpConnections();
            portArray.AddRange(from n in connections
                               where n.LocalEndPoint.Port >= startingPort
                               select n.LocalEndPoint.Port);

            //getting active tcp listners - WCF service listening in tcp
            endPoints = properties.GetActiveTcpListeners();
            portArray.AddRange(from n in endPoints
                               where n.Port >= startingPort
                               select n.Port);

            //getting active udp listeners
            endPoints = properties.GetActiveUdpListeners();
            portArray.AddRange(from n in endPoints
                               where n.Port >= startingPort
                               select n.Port);

            portArray.Sort();

            for (int i = startingPort; i < UInt16.MaxValue; i++)
                if (!portArray.Contains(i))
                    return i;

            return 0;
        }
    }
}
