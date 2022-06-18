using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DGScope.Library
{
    public class Facility
    {
        public Adaptation Adaptation { get; set; } = new Adaptation();
        public List<Track> Tracks { get; set; } = new List<Track>();
        public List<FlightPlan> FlightPlans { get; set; } = new List<FlightPlan>();

    }
}
