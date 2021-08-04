using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Media;

namespace Multiplayer.GDM
{
    public static class PlayerWatcher
    {
        public static int X1Position_StaticAddress = 0;
        public static int Y1Position_StaticAddress = 0;
        public static int X1Rotation_StaticAddress = 0;
        public static int Y1Rotation_StaticAddress = 0;
        public static int Size1_StaticAddress = 0;
        public static int Icon1FormAddr = 0;
        public static byte[] MyIcons = new byte[] { 0,0,0,0,0,0};
        public static int X2Position_StaticAddress = 0;
        public static int Y2Position_StaticAddress = 0;
        public static int X2Rotation_StaticAddress = 0;
        public static int Y2Rotation_StaticAddress = 0;
        public static int Size2_StaticAddress = 0;
        public static int Icon2FormAddr = 0;

        public static int LevelLength = 0;
        public static int FirstLevelAddr = 0;
        public static int ModuleBaseAddr = 0;
        public static int LevelID = -1;
        public static int LevelLobjectslist = 0;
        public static byte IsGlow = 0;
        public static IntPtr LevelObjectsListAddr;
        public static IntPtr LevelAddr;
        public static IntPtr CubeIconAddr1;
        public static IntPtr ShipIconAddr1;
        public static IntPtr BallIconAddr1;
        public static IntPtr UFOIconAddr1;
        public static IntPtr WaveIconAddr1;
        public static IntPtr RobotIconAddr1;
        public static IntPtr SpiderIconAddr1;
        public static IntPtr LevelLengthAddr;
        public static IntPtr IsDeadAddr;

        public static IntPtr Color1Addr;
        public static IntPtr Color2Addr;
        public static IntPtr Grav1Addr;
        public static IntPtr Grav2Addr;
        public static byte Col1 = 0x1, Col2 = 0x2;
        public static Process gd;
        public static Utilities.Memory.aMemory aMemory;

        public static bool HasSentDc = false;

        public static PositionMemory GetPositionMemoryFromAddress(
            IntPtr x_position,
            IntPtr y_position,
            IntPtr x_rotation,
            IntPtr y_rotation,
            IntPtr size,
            IntPtr form,
            IntPtr gravity)
        {
            var _form = GetForm(aMemory.ReadXBytes((IntPtr)form, 6));
            var icon_id = GetIcon(_form);
            return new PositionMemory(
                aMemory.ReadMemory<int>((IntPtr)x_position), 
                aMemory.ReadMemory<int>((IntPtr)y_position), 
                aMemory.ReadMemory<int>((IntPtr)x_rotation), 
                aMemory.ReadMemory<int>((IntPtr)y_rotation), 
                aMemory.ReadMemory<int>((IntPtr)size),
                _form,
                icon_id,
                aMemory.ReadMemory<byte>((IntPtr)gravity));
        }
       
        public static void Start()
        {
            new Thread(() =>
            {
                while (true)
                {
                    var processes = GetFromWindowTitle(GDM.Globals.Global_Data.Main.UserPref.WindowName);
                    GDM.Globals.Global_Data.IsInjected = false;
                    GDM.Globals.Global_Data.IsGDThere = false;

                    Debug.WriteLine("init gd");
                    if (processes.Count() >= 1)
                    {
                        Debug.WriteLine("gd is there");

                        GDM.Globals.Global_Data.IsGDThere = true;

                        gd = processes.FirstOrDefault();

                        bool isAdmin = Utilities.ProcAdminDetect.IsProcessOwnerAdmin(gd);
                        Debug.WriteLine("Geometry Dash is admin? " + isAdmin.ToString());
                        if (isAdmin && !Utilities.ProcAdminDetect.AmIAdmin())
                        {
                            GDM.Globals.Global_Data.Main.ShowError("GD is running as admin.", "GeometryDash.exe is running as admin, please run GDM as admin too.");
                            return;
                        }

                        gd.EnableRaisingEvents = true;

                        // if (GDM.Globals.Global_Data.JSONCommunication == null)
                        // GDM.Globals.Global_Data.JSONCommunication = new FileStream(GDM.Globals.Global_Data.JsonInputFile, FileMode.Create, FileAccess.Write, FileShare.ReadWrite);

                        InitializeAnimations();



                        InitializeAddressList();

                        InitializeJSON();

                        int f = 0;
                        byte _is_dead = 0;

                        while (!gd.HasExited)
                        {
                            // wait till connected
                            while (!GDM.Globals.Global_Data.IsConnected) { Thread.Sleep(1000); }

                            LevelLength = aMemory.ReadMemory<int>(LevelLengthAddr);
                            // LevelID = GetLevelID();
                            GDM.Globals.Global_Data.LevelID = LevelID;
                            GDM.Globals.Global_Data.LevelLength = aMemory.ReadMemory<float>(LevelLengthAddr);

                            _is_dead = aMemory.ReadMemory<byte>((IntPtr)GDM.Globals.Global_Data.AttemptAddr);
                            f = aMemory.ReadMemory<int>((IntPtr)FirstLevelAddr);
                            // keep reading gd
                            // TODO add attempt and death
                            var P1 = GetPositionMemoryFromAddress(
                                (IntPtr)X1Position_StaticAddress,
                                (IntPtr)Y1Position_StaticAddress,
                                (IntPtr)X1Rotation_StaticAddress,
                                (IntPtr)Y1Rotation_StaticAddress,
                                (IntPtr)Size1_StaticAddress,
                                (IntPtr)Icon1FormAddr,
                                Grav1Addr);

                            var P2 = GetPositionMemoryFromAddress(
                                (IntPtr)X2Position_StaticAddress,
                                (IntPtr)Y2Position_StaticAddress,
                                (IntPtr)X2Rotation_StaticAddress,
                                (IntPtr)Y2Rotation_StaticAddress,
                                (IntPtr)Size2_StaticAddress,
                                (IntPtr)Icon2FormAddr,
                                Grav2Addr);


                            if (f == 0 || P1.y_position == 0) // if not on level
                            {

                                if (!HasSentDc)
                                {
                                    var Array = Utilities.PacketParser.OutsideLevel();
                                    GDM.Globals.Global_Data.Connection.Send(Array);
                                    HasSentDc = true;
                                }

                                GDM.Globals.Global_Data.IsPlayingLevel = false;

                                var Array2 = Utilities.PacketParser.CreateMessage(P1, P2, _is_dead, -1, Col1, Col2, IsGlow, GDM.Globals.Global_Data.Room, MyIcons);
                          
                                if (GDM.Globals.Global_Data.Connection != null)
                                    GDM.Globals.Global_Data.Connection.Send(Array2);

                                Debug.WriteLine("Initializing address");

                                InitializeAddressList();

                                // save cpu
                                Thread.Sleep(1000);
                            }
                            else // if on level
                            {

                                HasSentDc = false;
                                GDM.Globals.Global_Data.IsPlayingLevel = true;
                                // send to server
                                MyIcons = GetIconIDs();

                                var Array = Utilities.PacketParser.CreateMessage(P1, P2, _is_dead, LevelID, Col1, Col2, IsGlow, GDM.Globals.Global_Data.Room, MyIcons);
                                if (GDM.Globals.Global_Data.Connection != null)
                                    GDM.Globals.Global_Data.Connection.Send(Array);

                                // Debug.WriteLine("Sent: " + Utilities.Converter.BytesToString(Array));
                                // https://www.google.com/search?q=60fps+is+how+many+millseconds+per+second&oq=60fps+is+how+many+millseconds+per+second
                                // 60fps is 16ms per second
                                // 50fps is 20ms per second
                                // 30fps is 33ms per second
                                // 14ms because thread.sleep is not fukin accurate

                                Thread.Sleep(GDM.Globals.Global_Data.RefreshRate);
                            }
                        }
                    }
                    Thread.Sleep(1000);
                }
            }).Start();
        }
        public static int GetLevelID()
        {
            int clevelid = aMemory.ReadMemory<int>((IntPtr)LevelAddr);
            return clevelid;
        }
        public static void InitializeJSON()
        {
            if (GDM.Globals.Global_Data.JSONCommunication == null)
                GDM.Globals.Global_Data.JSONCommunication = new JSONData(Globals.Paths.JsonInputFile);
        }
        public static void InitializeAnimations()
        {
        }
        public static byte GetForm(byte[] reads)
        {
            byte j = 0;
            foreach (var h in reads)
            {
                j++;
                if (Convert.ToBoolean(h))
                {
                    return j;
                }
            }
            return 0;
        }
        public static byte[] GetIconIDs()
        {
            List<byte> bytes = new List<byte>();
            for (int i = 0; i < 7; i++)
            {
                bytes.Add(GetIcon((byte)i));
            }
            return bytes.ToArray();
        }
        public static byte GetIcon(byte form)
        {
            var g = (Multiplayer.Utilities.FormLongs)form;
            // MessageBox.Show("form " + (g).ToString());
            switch (g)
            {
                case Utilities.FormLongs.cube:
                    return (byte)(aMemory.ReadMemory<byte>(CubeIconAddr1) - aMemory.ReadMemory<byte>(CubeIconAddr1 + 0x4));
                case Utilities.FormLongs.ship:
                    return (byte)(aMemory.ReadMemory<byte>(ShipIconAddr1) - aMemory.ReadMemory<byte>(ShipIconAddr1 + 0x4));
                case Utilities.FormLongs.ball:
                    return (byte)(aMemory.ReadMemory<byte>(BallIconAddr1) - aMemory.ReadMemory<byte>(BallIconAddr1 + 0x4));
                case Utilities.FormLongs.ufo:
                    return (byte)(aMemory.ReadMemory<byte>(UFOIconAddr1) - aMemory.ReadMemory<byte>(UFOIconAddr1 + 0x4));
                case Utilities.FormLongs.wave:
                    return (byte)(aMemory.ReadMemory<byte>(WaveIconAddr1) - aMemory.ReadMemory<byte>(WaveIconAddr1 + 0x4));
                case Utilities.FormLongs.robot:
                    return (byte)(aMemory.ReadMemory<byte>(RobotIconAddr1) - aMemory.ReadMemory<byte>(RobotIconAddr1 + 0x4));
                case Utilities.FormLongs.spider:
                    return (byte)(aMemory.ReadMemory<byte>(SpiderIconAddr1) - aMemory.ReadMemory<byte>(SpiderIconAddr1 + 0x4));
            }
            return 0x0;
        }
        public static void TeleportTo(int x, int y) {
            aMemory.WriteMemory((IntPtr)X1Position_StaticAddress, x);
            aMemory.WriteMemory((IntPtr)Y1Position_StaticAddress, y);
        }
        public static void InitClient() {

            if (!GDM.Globals.Global_Data.PlayerIDLoaded)
            {
                var pLid = aMemory.ReadMemory<int>((IntPtr)ModuleBaseAddr + GDM.Globals.Global_Data.AddrPlayerID);
                if (pLid != GDM.Globals.Global_Data.PlayerID)
                {
                    GDM.Globals.Global_Data.PlayerID = pLid;
                    GDM.Globals.Global_Data.PlayerIDLoaded = true;
                }
            }

            if (GDM.Globals.Global_Data.ClientID == null)
            {
                GDM.Globals.Global_Data.ClientID = BitConverter.GetBytes(GDM.Globals.Global_Data.PlayerID);
                GDM.Globals.Global_Data.Initializer.SetAccountID(GDM.Globals.Global_Data.PlayerID);

                new Thread(() =>
                {
                    try
                    {
                        Application.Current.Dispatcher.Invoke(new Action(() =>
                        {
                            GDM.Globals.Global_Data.Main.playerW.Text = GDM.Globals.Global_Data.Lang.FetchingUsername;
                        }));
                
                        if (GDM.Globals.Global_Data.PlayerID <= 0)
                        {
                            GDM.Globals.Global_Data.Initializer.SetAccountID("Unregistered");
                            GDM.Globals.Global_Data.Initializer.SetPlayerName("Player");
                        }
                        else
                            GDM.Globals.Global_Data.Initializer.SetPlayerName(Utilities.TCP.GetUsernameFromPlayerID(GDM.Globals.Global_Data.PlayerID));
                    }
                    catch (Exception ex)
                    {
                        Utilities.Utils.HandleException(ex);
                    }
                }).Start();
            }
        }
        static bool isVipChecked = false;
        public static bool InitializeAddressList()
        {
            Debug.WriteLine("Initializing");
            if (GDM.Globals.Global_Data.Connection != null)
                if (GDM.Globals.Global_Data.Connection.model.players.Count > 0)
                    foreach (var g in GDM.Globals.Global_Data.Connection.model.players.ToList())
                    {
                        try
                        {
                            g.Disconnected();
                            GDM.Globals.Global_Data.Connection.model.players.Remove(g);
                        }
                        catch (Exception ex)
                        {
                            Utilities.Utils.HandleException(ex);
                        }
                    }
            // GDM.Globals.Global_Data.Connection.model.players.Clear();
            // clear stackpanel

            if (GDM.Globals.Global_Data.JSONCommunication != null) GDM.Globals.Global_Data.JSONCommunication.Clear();
            GDM.Globals.Global_Data.Initializer.ClearPlayers();
            // GDM.Globals.Global_Data.Initializer.ClearPlayers();
            if (aMemory == null)
            aMemory = new Utilities.Memory.aMemory(gd);
            aMemory.InitProc(gd);
            if (!aMemory.dllInject(GDM.Globals.Global_Data.DLLPath))
            {
                Debug.WriteLine("Injection didnt happen");
                return false;
            }
            Stopwatch st = Stopwatch.StartNew();
            Debug.WriteLine("Injection just happen");
            ModuleBaseAddr = aMemory.ReadMemory<int>((IntPtr)aMemory.GetModuleAddress(GDM.Globals.Global_Data.Main.UserPref.MainModule) + GDM.Globals.Global_Data.BaseAddr);
            FirstLevelAddr = ModuleBaseAddr + GDM.Globals.Global_Data.RealTimePlayerOffsets1[0];

            // get base addresses on realtime player
            int u1 = ModuleBaseAddr;
            foreach (var i in GDM.Globals.Global_Data.RealTimePlayerOffsets1) u1 = aMemory.ReadMemory<int>((IntPtr)u1 + i);
            int u2 = ModuleBaseAddr;
            foreach (var i in GDM.Globals.Global_Data.RealTimePlayerOffsets2) u2 = aMemory.ReadMemory<int>((IntPtr)u2 + i);

            X1Position_StaticAddress = u1 + GDM.Globals.Global_Data.Xpos;
            Y1Position_StaticAddress = u1 + GDM.Globals.Global_Data.Ypos;
            X1Rotation_StaticAddress = u1 + GDM.Globals.Global_Data.Xrotation;
            Y1Rotation_StaticAddress = u1 + GDM.Globals.Global_Data.Yrotation;
            Size1_StaticAddress = u1 + GDM.Globals.Global_Data.PlayerSize;
            Icon1FormAddr = u1 + GDM.Globals.Global_Data.IconFormOffset;
            Grav1Addr = (IntPtr)u1 + GDM.Globals.Global_Data.GravityOffset;

            X2Position_StaticAddress = u2 + GDM.Globals.Global_Data.Xpos;
            Y2Position_StaticAddress = u2 + GDM.Globals.Global_Data.Ypos;
            X2Rotation_StaticAddress = u2 + GDM.Globals.Global_Data.Xrotation;
            Y2Rotation_StaticAddress = u2 + GDM.Globals.Global_Data.Yrotation;
            Size2_StaticAddress = u2 + GDM.Globals.Global_Data.PlayerSize;
            Icon2FormAddr = u2 + GDM.Globals.Global_Data.IconFormOffset;
            Grav2Addr = (IntPtr)u2 + GDM.Globals.Global_Data.GravityOffset;

            InitClient();

            int lvAddr = 0;
            lvAddr = ModuleBaseAddr;
            lvAddr = aMemory.ReadMemory<int>((IntPtr)lvAddr + GDM.Globals.Global_Data.LevelLengthOffets1[0]);
            GDM.Globals.Global_Data.LevelLength = aMemory.ReadMemory<float>((IntPtr)lvAddr + GDM.Globals.Global_Data.LevelLengthOffets1[1]);
            LevelLengthAddr = (IntPtr)lvAddr + GDM.Globals.Global_Data.LevelLengthOffets1[1];

            int lvlobjcount = ModuleBaseAddr;
            lvlobjcount = aMemory.ReadMemory<int>((IntPtr)lvlobjcount + GDM.Globals.Global_Data.LevelObjectsCountOffsets[0]);
            lvlobjcount = aMemory.ReadMemory<int>((IntPtr)lvlobjcount + GDM.Globals.Global_Data.LevelObjectsCountOffsets[1]);
            lvlobjcount = aMemory.ReadMemory<int>((IntPtr)lvlobjcount + GDM.Globals.Global_Data.LevelObjectsCountOffsets[2]);
            LevelObjectsListAddr = (IntPtr)lvlobjcount + GDM.Globals.Global_Data.LevelObjectsCountOffsets[3];


            GDM.Globals.Global_Data.AttemptAddr = ModuleBaseAddr;
            GDM.Globals.Global_Data.AttemptAddr = aMemory.ReadMemory<int>((IntPtr)GDM.Globals.Global_Data.AttemptAddr + GDM.Globals.Global_Data.AttemptOffsets1[0]);
            GDM.Globals.Global_Data.AttemptAddr = GDM.Globals.Global_Data.AttemptAddr + GDM.Globals.Global_Data.AttemptOffsets1[1];
            LevelAddr = (IntPtr)ModuleBaseAddr + GDM.Globals.Global_Data.LevelIDOffset;
            int nLevelID = GetLevelID();

            if (LevelID != nLevelID)
            {
                LevelID = nLevelID;
                new Thread(() => {
                    GDM.Globals.Global_Data.Initializer.LoadLevelID(nLevelID);
                }).Start();
             
            }

            // init icons
            CubeIconAddr1 = (IntPtr)ModuleBaseAddr + 0x1E8;
            ShipIconAddr1 = (IntPtr)ModuleBaseAddr + 0x1F4;
            BallIconAddr1 = (IntPtr)ModuleBaseAddr + 0x200;
            UFOIconAddr1 = (IntPtr)ModuleBaseAddr + 0x20C;
            WaveIconAddr1 = (IntPtr)ModuleBaseAddr + 0x218;
            RobotIconAddr1 = (IntPtr)ModuleBaseAddr + 0x224;
            SpiderIconAddr1 = (IntPtr)ModuleBaseAddr + 0x230;
            Color1Addr = (IntPtr)ModuleBaseAddr + 0x23C;
            Col1 = (byte)(aMemory.ReadMemory<byte>(Color1Addr) - aMemory.ReadMemory<byte>(Color1Addr + 0x4));
            Color2Addr = (IntPtr)ModuleBaseAddr + 0x248;
            Col2 = (byte)(aMemory.ReadMemory<byte>(Color2Addr) - aMemory.ReadMemory<byte>(Color2Addr + 0x4));
            IsGlow = aMemory.ReadMemory<byte>((IntPtr)ModuleBaseAddr + GDM.Globals.Global_Data.IsGlow);


            MyIcons = GetIconIDs();
            var pLid2 = aMemory.ReadMemory<int>((IntPtr)ModuleBaseAddr + GDM.Globals.Global_Data.AddrPlayerID);

            if (!GDM.Globals.Global_Data.PlayerIDLoaded)
            {
                GDM.Globals.Global_Data.PlayerID = pLid2;
                GDM.Globals.Global_Data.PlayerIDLoaded = true;
                GDM.Globals.Global_Data.Initializer.SetAccountID(GDM.Globals.Global_Data.PlayerID);
            }

            GDM.Globals.Global_Data.IsInjected = true;

            return Size1_StaticAddress != 0;
        }
        public static Process[] GetFromWindowTitle(string title)
        {
            return Process.GetProcesses().Where(p => p.MainWindowTitle.ToLower().Trim() == title.ToLower().Trim()).ToArray();
        }
    }
}
