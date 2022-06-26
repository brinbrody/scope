using System;
using System.Collections.ObjectModel;
using System.Text.Json.Serialization;

namespace DGScope.Library
{
    public class Facility
    {
        public string FacilityID
        {
            get
            {
                return Adaptation.Name;
            }
            set
            {
                Adaptation.Name = value;
            }
        }
        public Adaptation Adaptation { get; set; } = new Adaptation();
        public Altimeter Altimeter { get; set; } = new Altimeter();
        [JsonIgnore]
        public ObservableCollection<Track> Tracks { get; set; } = new ObservableCollection<Track>();
        [JsonIgnore]
        public ObservableCollection<FlightPlan> FlightPlans { get; set; } = new ObservableCollection<FlightPlan>();
        public override string ToString()
        {
            return FacilityID;
        }
    }
}
