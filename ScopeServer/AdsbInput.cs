using DGScope.Library;
using DGScope.Receivers;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using VirtualRadar.Interface.Adsb;
using VirtualRadar.Interface.ModeS;
using VirtualRadar.Library.Adsb;

namespace ScopeServer
{
    public class AdsbInput
    {
        private List<AdsbReceiver> adsbReceivers;
        private ObservableCollection<Facility> facilities;
        private Dictionary<int, CompactPositionReportingCoordinate> earlierPositionMessages = new Dictionary<int, CompactPositionReportingCoordinate>();
        private CompactPositionReporting cpr = new CompactPositionReporting();
        public AdsbInput(List<AdsbReceiver> receivers, ObservableCollection<Facility> facilities)
        {
            this.adsbReceivers = receivers;
            this.facilities = facilities;
        }

        public void Start()
        {
            foreach (AdsbReceiver receiver in adsbReceivers)
            {
                if (receiver.Enabled)
                {
                    receiver.Start();
                    receiver.AdsbUpdateReceived += Receiver_AdsbUpdateReceived;
                }
            }
        }

        private void Receiver_AdsbUpdateReceived(object sender, UpdateEventArgs e)
        {
            ParseAdsbUpdate(e.Update as TrackUpdate);
        }
        public async void ParseAdsbUpdate(TrackUpdate update)
        {
            bool updated = false;
            List<Track> tracks = new List<Track>();
            facilities.ToList().ForEach(facility =>
            {
                lock (facility.Tracks)
                {
                    tracks.AddRange(facility.Tracks.Where(track => track.ModeSCode == update.ModeSCode));
                }
            });
            if (tracks.Count == 0)
                return;
            foreach (Track track in tracks)
            {
                var newUpdate = new TrackUpdate(update,track);
                track.UpdateTrack(newUpdate);
            }
            
        }
    }
}
