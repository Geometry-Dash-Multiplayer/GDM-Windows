using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Multiplayer
{
    public interface IService
    {
        /// <summary>
        /// Set to true if the service will be run on startup.
        /// </summary>
        bool AutoStart { get; }

        Task StartAsync();
    }
}
