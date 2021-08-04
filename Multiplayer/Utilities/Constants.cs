using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Multiplayer.Utilities
{
    public enum Prefixes
    {
        Hello = 0x3,
        Ping = 0x0,
        Message = 0x1,
        Disconnext = 0x2,
        AckHello = 0x4,
        ServerData = 0x5,
        PlayerDisconnect = 0x7,
        PlayerIcons = 0x8,
        ReceivedPlayerIcons = 0x9,
        OutsideLevel = 0x10,
        VipActions = 0x11,
        BadKey = 0x12
    }
    public enum VipActions
    {
        NewLobby = 0x1
    }
    public enum FormLongs
    {
        cube = 0,
        ship = 1,
        ufo = 2,
        ball = 3,
        wave = 4,
        robot = 5,
        spider = 6,
    }
    public enum UserAgents
    {
        windows = 0,
        android = 1,
        mac = 2,
        linux = 3
    }
}
