using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace NautaManager.Interfaces
{
    interface IAsyncInitialization
    {
        Task Initialized { get; }
    }
}
