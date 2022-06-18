using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using DGScope.Library;

namespace DGScope.Receivers
{
   public abstract class Receiver
    {
        public string Name { get; set;  }
        public bool Enabled { get; set; }

        protected ObservableCollection<Track> Tracks;
        public GeoPoint Location { get; set; } = new GeoPoint(0, 0);
        public abstract void Start();
        public abstract void Stop();
        public void Restart(int sleep = 0)
        {
            Stop();
            System.Threading.Thread.Sleep(sleep);
            Start();
        }

        public void SetAircraftList(ObservableCollection<Track> tracks)
        {
            Tracks = tracks;
        }
        
        public Track GetPlane(int icaoID)
        {
            Track track;
            lock (Tracks)
            {
                track = (from x in Tracks where x.ModeSCode == icaoID select x).FirstOrDefault();
                if (track == null)
                {
                    track = new Track(icaoID);
                    Tracks.Add(track);
                    Debug.WriteLine("Added airplane {0} from {1}", icaoID.ToString("X"), Name);
                }
            }
            return track;
        }


        public override string ToString()
        {
            return base.ToString();
        }
    }

   
}