using DGScope.Library;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DGScope.Receivers
{
    public abstract class AdsbReceiver
    {
        public string Name { get; set; }
        public bool Enabled { get; set; } = false;
        public abstract void Start();
        public abstract void Stop();
        public void Restart(int sleep = 0)
        {
            Stop();
            System.Threading.Thread.Sleep(sleep);
            Start();
        }

        public void SendUpdate(AdsbUpdate update)
        {
            AdsbUpdateReceived?.Invoke(this, new AdsbUpdateEventArgs(update));
        }
        public event EventHandler<AdsbUpdateEventArgs> AdsbUpdateReceived;
    }
    public class AdsbUpdateEventArgs : EventArgs
    {
        public AdsbUpdate AdsbUpdate { get; }
        public AdsbUpdateEventArgs(AdsbUpdate update)
        {
            AdsbUpdate = update;
        }
    }
}
