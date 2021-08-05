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

namespace Multiplayer.GDM.Player_Watcher
{
    public static class Memory
    {
        public static byte[] Icons = new byte[] { 0, 0, 0, 0, 0, 0 };
        public static byte Col1 = 0x1, Col2 = 0x2, IsGlow;
        public static int LevelID;

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

                        bool isAdmin = Utilities.Processing.Administrator_Detection.IsProcessOwnerAdmin(gd);
                        Debug.WriteLine("Geometry Dash is admin? " + isAdmin.ToString());
                        if (isAdmin && !Utilities.Processing.Administrator_Detection.AmIAdmin())
                        {
                            GDM.Globals.Global_Data.Main.ShowError("GD is running as admin.", "GeometryDash.exe is running as admin, please run GDM as admin too.");
                            return;
                        }

                        gd.EnableRaisingEvents = true;

                        InitializeAddressList();

                        InitializeJSON();

                        byte IsDead = 0;
                        while (!gd.HasExited)
                        {
                            while (!GDM.Globals.Global_Data.IsConnected) { Thread.Sleep(1000); }
                            Globals.Global_Data.LevelID = LevelID;
                            LevelID = GetLevelID();
                            IsDead = aMemory.ReadMemory<byte>((IntPtr)Globals.Addresses.AttemptAddr);

                            var P1 = GetPositionMemoryFromAddress(
                                (IntPtr)Cached_Addresses.X1Position_StaticAddress,
                                (IntPtr)Cached_Addresses.Y1Position_StaticAddress,
                                (IntPtr)Cached_Addresses.X1Rotation_StaticAddress,
                                (IntPtr)Cached_Addresses.Y1Rotation_StaticAddress,
                                (IntPtr)Cached_Addresses.Size1_StaticAddress,
                                (IntPtr)Cached_Addresses.Icon1FormAddr,
                                Cached_Addresses.Player1GravityAddress);

                            var P2 = GetPositionMemoryFromAddress(
                                (IntPtr)Cached_Addresses.X2Position_StaticAddress,
                                (IntPtr)Cached_Addresses.Y2Position_StaticAddress,
                                (IntPtr)Cached_Addresses.X2Rotation_StaticAddress,
                                (IntPtr)Cached_Addresses.Y2Rotation_StaticAddress,
                                (IntPtr)Cached_Addresses.Size2_StaticAddress,
                                (IntPtr)Cached_Addresses.Icon2FormAddr,
                                Cached_Addresses.Player2GravityAddress);


                            Debug.WriteLine($"y post: {P1.y_position}");

                            RefreshLevelInformation();

                            if (P1.y_position == 0) // if not on level
                            {
                                if (!HasSentDc)
                                {
                                    var Array = Utilities.PacketParser.OutsideLevel();
                                    GDM.Globals.Global_Data.Connection.Send(Array);
                                    HasSentDc = true;
                                }

                                GDM.Globals.Global_Data.IsPlayingLevel = false;

                                var Array2 = Utilities.PacketParser.CreateMessage(P1, P2, IsDead, -1, Col1, Col2, IsGlow, GDM.Globals.Global_Data.Room, Icons);

                                if (GDM.Globals.Global_Data.Connection != null)
                                    GDM.Globals.Global_Data.Connection.Send(Array2);

                                Debug.WriteLine("Initializing address");

                                ClearPlayers();

                                RefreshPositionAddresses();

                                InitializeAddressList();

                                Thread.Sleep(1000);
                            }
                            else // if on level
                            {

                                RefreshPositionAddresses();

                                HasSentDc = false;

                                GDM.Globals.Global_Data.IsPlayingLevel = true;
                                // send to server
                                Icons = GetIconIDs();

                                var Array = Utilities.PacketParser.CreateMessage(P1, P2, IsDead, Memory.LevelID, Col1, Col2, IsGlow, GDM.Globals.Global_Data.Room, Icons);
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
            int clevelid = aMemory.ReadMemory<int>(Cached_Addresses.LevelAddr);
            return clevelid;
        }
        public static void InitializeJSON()
        {
            if (GDM.Globals.Global_Data.JSONCommunication == null)
                GDM.Globals.Global_Data.JSONCommunication = new JSONData(Globals.Paths.JsonInputFile);
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
            switch (g)
            {
                case Utilities.FormLongs.cube:
                    return (byte)(aMemory.ReadMemory<byte>(Cached_Addresses.CubeIconAddr1) - aMemory.ReadMemory<byte>(Cached_Addresses.CubeIconAddr1 + 0x4));
                case Utilities.FormLongs.ship:
                    return (byte)(aMemory.ReadMemory<byte>(Cached_Addresses.ShipIconAddr1) - aMemory.ReadMemory<byte>(Cached_Addresses.ShipIconAddr1 + 0x4));
                case Utilities.FormLongs.ball:
                    return (byte)(aMemory.ReadMemory<byte>(Cached_Addresses.BallIconAddr1) - aMemory.ReadMemory<byte>(Cached_Addresses.BallIconAddr1 + 0x4));
                case Utilities.FormLongs.ufo:
                    return (byte)(aMemory.ReadMemory<byte>(Cached_Addresses.UFOIconAddr1) - aMemory.ReadMemory<byte>(Cached_Addresses.UFOIconAddr1 + 0x4));
                case Utilities.FormLongs.wave:
                    return (byte)(aMemory.ReadMemory<byte>(Cached_Addresses.WaveIconAddr1) - aMemory.ReadMemory<byte>(Cached_Addresses.WaveIconAddr1 + 0x4));
                case Utilities.FormLongs.robot:
                    return (byte)(aMemory.ReadMemory<byte>(Cached_Addresses.RobotIconAddr1) - aMemory.ReadMemory<byte>(Cached_Addresses.RobotIconAddr1 + 0x4));
                case Utilities.FormLongs.spider:
                    return (byte)(aMemory.ReadMemory<byte>(Cached_Addresses.SpiderIconAddr1) - aMemory.ReadMemory<byte>(Cached_Addresses.SpiderIconAddr1 + 0x4));
            }
            return 0x0;
        }
        public static void TeleportTo(int x, int y)
        {
            aMemory.WriteMemory((IntPtr)Cached_Addresses.X1Position_StaticAddress, x);
            aMemory.WriteMemory((IntPtr)Cached_Addresses.Y1Position_StaticAddress, y);
        }
        public static void InitClient()
        {
            if (!GDM.Globals.Global_Data.PlayerIDLoaded)
            {
                var pLid = aMemory.ReadMemory<int>((IntPtr)Cached_Addresses.ModuleBaseAddr + GDM.Globals.Addresses.AddrPlayerID);
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
        public static void ClearPlayers() {

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
        }
        public static void RefreshLevelInformation() {
            int lvAddr = 0;
            lvAddr = Cached_Addresses.ModuleBaseAddr;
            lvAddr = aMemory.ReadMemory<int>((IntPtr)lvAddr + GDM.Globals.Addresses.LevelLengthOffets1[0]);
            Globals.Addresses.LevelLength = aMemory.ReadMemory<float>((IntPtr)lvAddr + GDM.Globals.Addresses.LevelLengthOffets1[1]);

            Cached_Addresses.LevelLengthAddr = (IntPtr)lvAddr + GDM.Globals.Addresses.LevelLengthOffets1[1];
            Cached_Addresses.LevelAddr = (IntPtr)Cached_Addresses.ModuleBaseAddr + GDM.Globals.Addresses.LevelIDOffset;
        }
        public static void RefreshPositionAddresses() {
            int u1 = Cached_Addresses.ModuleBaseAddr;
            foreach (var i in GDM.Globals.Addresses.Player1Layer) u1 = aMemory.ReadMemory<int>((IntPtr)u1 + i);
            int u2 = Cached_Addresses.ModuleBaseAddr;
            foreach (var i in GDM.Globals.Addresses.Player2Layer) u2 = aMemory.ReadMemory<int>((IntPtr)u2 + i);
            Cached_Addresses.X1Position_StaticAddress = u1 + GDM.Globals.Addresses.XPositionOffset;
            Cached_Addresses.Y1Position_StaticAddress = u1 + GDM.Globals.Addresses.YPositionOffset;
            Cached_Addresses.X1Rotation_StaticAddress = u1 + GDM.Globals.Addresses.XRotationOffset;
            Cached_Addresses.Y1Rotation_StaticAddress = u1 + GDM.Globals.Addresses.YRotationOffset;
            Cached_Addresses.Size1_StaticAddress = u1 + GDM.Globals.Addresses.PlayerSizeOffset;
            Cached_Addresses.Icon1FormAddr = u1 + GDM.Globals.Addresses.IconFormOffset;
            Cached_Addresses.Player1GravityAddress = (IntPtr)u1 + GDM.Globals.Addresses.GravityOffset;
            Cached_Addresses.X2Position_StaticAddress = u2 + GDM.Globals.Addresses.XPositionOffset;
            Cached_Addresses.Y2Position_StaticAddress = u2 + GDM.Globals.Addresses.YPositionOffset;
            Cached_Addresses.X2Rotation_StaticAddress = u2 + GDM.Globals.Addresses.XRotationOffset;
            Cached_Addresses.Y2Rotation_StaticAddress = u2 + GDM.Globals.Addresses.YRotationOffset;
            Cached_Addresses.Size2_StaticAddress = u2 + GDM.Globals.Addresses.PlayerSizeOffset;
            Cached_Addresses.Icon2FormAddr = u2 + GDM.Globals.Addresses.IconFormOffset;
            Cached_Addresses.Player1GravityAddress = (IntPtr)u2 + GDM.Globals.Addresses.GravityOffset;
        }
        // my favorite method
        public static void InitializeAddressList()
        {
            if (GDM.Globals.Global_Data.JSONCommunication != null) GDM.Globals.Global_Data.JSONCommunication.Clear();
            if (GDM.Globals.Global_Data.Initializer != null) GDM.Globals.Global_Data.Initializer.ClearPlayers();

            aMemory = aMemory ?? new Utilities.Memory.aMemory(gd);
            aMemory.InitProc(gd);
            aMemory.dllInject(GDM.Globals.Global_Data.DLLPath);

            Cached_Addresses.ModuleBaseAddr = aMemory.ReadMemory<int>((IntPtr)aMemory.GetModuleAddress(GDM.Globals.Global_Data.Main.UserPref.MainModule) + GDM.Globals.Addresses.BaseAddr);
            RefreshPositionAddresses();
            InitClient();

            RefreshLevelInformation();

            int nLevelID = GetLevelID();
            if (LevelID != nLevelID)
            {
                LevelID = nLevelID;
                // wtf
                new Thread(() =>
                {
                    GDM.Globals.Global_Data.Initializer.LoadLevelID(nLevelID);
                }).Start();
            }
            Cached_Addresses.CubeIconAddr1 = (IntPtr)Cached_Addresses.ModuleBaseAddr + 0x1E8;
            Cached_Addresses.ShipIconAddr1 = (IntPtr)Cached_Addresses.ModuleBaseAddr + 0x1F4;
            Cached_Addresses.BallIconAddr1 = (IntPtr)Cached_Addresses.ModuleBaseAddr + 0x200;
            Cached_Addresses.UFOIconAddr1 = (IntPtr)Cached_Addresses.ModuleBaseAddr + 0x20C;
            Cached_Addresses.WaveIconAddr1 = (IntPtr)Cached_Addresses.ModuleBaseAddr + 0x218;
            Cached_Addresses.RobotIconAddr1 = (IntPtr)Cached_Addresses.ModuleBaseAddr + 0x224;
            Cached_Addresses.SpiderIconAddr1 = (IntPtr)Cached_Addresses.ModuleBaseAddr + 0x230;
            Cached_Addresses.Color1Addr = (IntPtr)Cached_Addresses.ModuleBaseAddr + 0x23C;
            Col1 = (byte)(aMemory.ReadMemory<byte>(Cached_Addresses.Color1Addr) - aMemory.ReadMemory<byte>(Cached_Addresses.Color1Addr + 0x4));
            Cached_Addresses.Color2Addr = (IntPtr)Cached_Addresses.ModuleBaseAddr + 0x248;
            Col2 = (byte)(aMemory.ReadMemory<byte>(Cached_Addresses.Color2Addr) - aMemory.ReadMemory<byte>(Cached_Addresses.Color2Addr + 0x4));
            IsGlow = aMemory.ReadMemory<byte>((IntPtr)Cached_Addresses.ModuleBaseAddr + GDM.Globals.Addresses.IsGlow);
            Icons = GetIconIDs();
            var pLid2 = aMemory.ReadMemory<int>((IntPtr)Cached_Addresses.ModuleBaseAddr + GDM.Globals.Addresses.AddrPlayerID);
            if (!Globals.Global_Data.PlayerIDLoaded)
            {
                Globals.Global_Data.PlayerID = pLid2;
                Globals.Global_Data.PlayerIDLoaded = true;
                Globals.Global_Data.Initializer.SetAccountID(GDM.Globals.Global_Data.PlayerID);
            }
            Globals.Global_Data.IsInjected = true;
        }
        public static Process[] GetFromWindowTitle(string title)
        {
            return Process.GetProcesses().Where(p => p.MainWindowTitle.ToLower().Trim() == title.ToLower().Trim()).ToArray();
        }
    }
}
