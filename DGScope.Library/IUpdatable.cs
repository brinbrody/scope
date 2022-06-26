using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DGScope.Library
{
    public interface IUpdatable
    {
        DateTime LastMessageTime { get;  }
        event EventHandler<UpdateEventArgs> Updated;
        event EventHandler<UpdateEventArgs> Created;
    }
}
