using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace Multiplayer.GDM
{
    public class Server
    {
        public byte[] buffer = new byte[4096];
        public bool isBinded=false;
        public UdpClient sock;
        public IPEndPoint EndP;
        public IPEndPoint MyPP;
        public Utilities.JSON_Models.Players model;
        // public Stopwatch PingCounter = new Stopwatch();
        private Initialize Main;
        private string IPaddr;
        private Stopwatch PingCounter = new Stopwatch();
        public bool isHelloAcked = false;
        Thread HelloThread, ReadThread;
        public int PlayerCount = 0;
        public bool AckedIcons = false;
        public Server(string ip, Initialize _main)
        {
            model = new Utilities.JSON_Models.Players();
            Globals.Global_Data.ActiveModel = model;
            Lobby_Status.Start();
            Init(ip, _main);
        }

        public void Init(string ip, Initialize _main, bool newEp = true)
        {
            isBinded = false;
            Globals.Global_Data.IsConnected = false;
            Globals.Global_Data.Connection = this;
            Debug.WriteLine("Initializing connection...");
            if (newEp)
            {
                Debug.WriteLine("Reinit");
                IPaddr = ip;
                Main = _main;
                Main.SetPing("Connecting...");

                var gsock = new UdpClient();
                IPAddress serverAddr = IPAddress.Parse(ip);
                if (EndP is null)
                    EndP = new IPEndPoint(serverAddr, Globals.Global_Data.StandardPort);
                if (MyPP is null)
                    MyPP = new IPEndPoint(IPAddress.Any, Utilities.Randomness.GetAvailablePort());
                try
                {
                    gsock.Client.Bind(MyPP);
                    sock = gsock;
                }
                catch (Exception ex)
                {
                    Utilities.Utils.HandleException(ex);
                }
            }
            isBinded = true;
            if (ReadThread == null)
            {
                Main.SetPing("Authenticating...");
            }
            StartRead();
            SendHello();
        }
        public void SendDiconnect()
        {
            var di = Utilities.PacketParser.CreateDisconnect();
            Send(di);
            Globals.Global_Data.IsConnected = false;
        }
        public void SendHello()
        {
            isHelloAcked = false;
            HelloThread = new Thread(() =>
            {
                while (!isHelloAcked)
                {
                    if (Globals.Global_Data.VIPKeyOk)
                    {
                        var helloPacket = Utilities.PacketParser.CreateHello();
                        Send(helloPacket);
                        Main.SetLocalPort(MyPP.Port);
                        Debug.WriteLine("Trying to hello. " + Utilities.Converter.BytesToString(Globals.Global_Data.Main.UserPref.Key));
                    }
                    Thread.Sleep(1000);
                }
                if (GDM.Player_Watcher.Memory.LevelID > 0)
                {
                    Globals.Global_Data.Initializer.LoadLevelID(GDM.Player_Watcher.Memory.LevelID);
                }

                StartPingSend();
            }); HelloThread.Start();

        }
        public void Send(byte[] array)
        {
            try
            {
                sock.Send(array, array.Length, EndP);
            }
            catch (Exception ex)
            {
                Globals.Global_Data.HandleException(ex);
            }
        }
        // keep alive
        public void StartPingSend()
        {
            byte[] pingPacket = Utilities.PacketParser.CreatePing();
            Globals.Global_Data.IsConnected = true;
            while (true)
            {
                PingCounter.Restart();
                Send(pingPacket);
                int count = 2;
                while (count >= 0 && PingCounter.IsRunning)
                {
                    count--;
                    Thread.Sleep(1000);
                }
            }
        }
        public void StartRead()
        {
            ReadThread = new Thread(() =>
            {
                while (isBinded)
                {
                    try
                    {
                        byte[] received = sock.Receive(ref EndP);
                        // Debug.WriteLine(Utilities.Converter.BytesToString(received));
                        Utilities.ReceivePacketParser r = new Utilities.ReceivePacketParser(received);
                        switch (r.prefix)
                        {
                            case Utilities.Prefixes.Ping:
                                    Main.SetPing((int)PingCounter.ElapsedMilliseconds);
                                    if (PlayerCount != r.PlayersCount)
                                    {
                                        PlayerCount = r.PlayersCount;
                                        Main.SetPlayerCount(r.PlayersCount);
                                    }
                                PingCounter.Stop();
                                break;
                            case Utilities.Prefixes.AckHello:
                                // Main.SetRoom(Globals.Global_Data.Room);
                                if (!isHelloAcked)
                                {
                                    isHelloAcked = true;
                                    Main.Main.StartAnimation("ShowLevelsAndStats");
                                    Main.SetPing("Connected!");
                                    new Thread(() => {
                                        Thread.Sleep(500); // because im a pogger
                                        GDM.Globals.Global_Data.VIPKeyOk = true;
                                    }).Start();
                                }
                                break;
                            case Utilities.Prefixes.ServerData:
                                break;
                            case Utilities.Prefixes.Disconnext:
                                if (Globals.Global_Data.VIPKeyOk)
                                {
                                    Main.Main.ShowError("Server sent disconnect", r.ErrorMessage);
                                    Main.SetPing("Disconnected...");
                                    Globals.Global_Data.Room = 0;
                                    Main.SetRoom(Globals.Global_Data.Room);
                                    Reconnect();
                                }
                                break;
                            case Utilities.Prefixes.Message:
                                if (Globals.Global_Data.ReceiveNewClients)
                                if (Globals.Global_Data.IsPlayingLevel 
                                && !Main.Main.UserPref.BlockedIDs.Contains(r.clientID) 
                                && r.clientID > 1
                                )
                                    {
                                        var clinet = model.players.FindIndex(x => x.id == r.clientID);
                                    if (clinet == -1)
                                    {
                                        var op = new Client.Client(r.clientID, r.player1, r.player2, r.col1, r.col2, r.isglow, r.IconIDs);
                                        op.Lobby = Globals.Global_Data.Room;
                                        op.IsVIP = r.isvip;
                                        if (op.IsVIP == 0x1)
                                        {
                                            op.colorR = 158;
                                            op.colorG = 255;
                                            op.colorB = 194;
                                        }
                                        model.players.Add(op);
                                        op.Initialize();
                                        clinet = model.players.IndexOf(op);
                                        }

                                        if (clinet >= 0 && clinet < model.players.Count)
                                    if (model.players[clinet].isIconIDDownloaded)
                                    {
                                        model.players[clinet].Set(r.player1, r.player2, r.isded, r.col1, r.col2, r.IconIDs);
                                        model.players[clinet].IsVIP = r.isvip;
                                        var j = model.players.Where(x => x.isIconIDDownloaded);
                                        if (Globals.Global_Data.JSONCommunication != null)
                                        {
                                            Globals.Global_Data.JSONCommunication.Write(j.ToList());
                                        }
                                    }

                                }
                                break;
                            case Utilities.Prefixes.PlayerDisconnect:
                                var clinet2 = model.players.FindIndex(x => x.id == r.clientID);
                                if (clinet2 != -1)
                                {
                                    Globals.Global_Data.Initializer.Announce(Globals.Global_Data.Lang.Left.Replace("%username%", model.players[clinet2].username));
                                    model.players[clinet2].Disconnected(); 
                                    model.players.RemoveAt(clinet2);
                                    var j = model.players.Where(x => x.isIconIDDownloaded);

                                    if (Globals.Global_Data.JSONCommunication != null)
                                        Globals.Global_Data.JSONCommunication.Write(j.ToList());
                                }
                                break;
                            case Utilities.Prefixes.PlayerIcons:
                                var clinet4 = model.players.FindIndex(x => x.id == r.clientID);
                                model.players[clinet4].IconIDs = r.IconIDs;
                                break;
                            case Utilities.Prefixes.ReceivedPlayerIcons:
                                AckedIcons = true;
                                break;
                            case Utilities.Prefixes.BadKey:
                                Globals.Global_Data.VIPKeyOk = false;
                                Globals.Global_Data.Initializer.Relogin();
                                Disconnect();
                                // show bad key anim
                                break;
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.ToString());
                    }

                }
            }); ReadThread.Start();
        }

        public void Reconnect()
        {
            isHelloAcked = false;
            new Thread(() => {
                ReadThread.Abort();
                Thread.Sleep(2000);
                Main.Announce("Reconnecting...");
                Globals.Global_Data.IsConnected = false;
                isBinded = false;
                try
                {
                    sock.Close();
                    HelloThread.Abort();
                }
                catch (Exception ex)
                {
                    Utilities.Utils.HandleException(ex);
                }
                this.Init(IPaddr, Main);
            }).Start();
        }
        public void Disconnect() {
            try
            {
                Thread.Sleep(100);
                if (Globals.Global_Data.JSONCommunication != null)
                Globals.Global_Data.JSONCommunication.Clear();
                ReadThread.Abort();
                sock.Close();
                HelloThread.Abort();
            }
            catch (Exception ex)
            {
                Globals.Global_Data.HandleException(ex);
            }
            Globals.Global_Data.Connection = null;
            Globals.Global_Data.IsConnected = false;
        }
    }
}
