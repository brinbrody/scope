using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DGScope.Library
{
    public class FlightPlan
    {
        private string callsign;
        private string? aircraftType;
        private string? wakeCategory;
        private string? flightRules;
        private string? origin;
        private string? destination;
        private string? entryFix;
        private string? exitFix;
        private string? route;
        private int requestedAltitude;
        private string? scratchpad1;
        private string? scratchpad2;
        private string? runway;
        private string? owner;
        private string? pendingHandoff;
        private string? squawk;
        private LDRDirection? ldrDirection;
        private Track? associatedTrack;
        public string Callsign 
        {
            get => callsign;
            set
            {
                if (callsign == value)
                    return;
                callsign = value;
                FlightPlanUpdated?.Invoke(this, new EventArgs());
            }
        }
        public string? AircraftType
        {
            get => aircraftType;
            set
            {
                if (aircraftType == value)
                    return;
                aircraftType = value;
                FlightPlanUpdated?.Invoke(this, new EventArgs());
            }
        }
        public string? WakeCategory
        {
            get => wakeCategory;
            set
            {
                if (wakeCategory == value)
                    return;
                wakeCategory = value;
                FlightPlanUpdated?.Invoke(this, new EventArgs());
            }
        }
        public string? FlightRules
        {
            get => flightRules;
            set
            {
                if (flightRules == value)
                    return;
                flightRules = value;
                FlightPlanUpdated?.Invoke(this, new EventArgs());
            }
        }
        public string? Origin
        {
            get => origin;
            set
            {
                if (origin == value)
                    return;
                origin = value;
                FlightPlanUpdated?.Invoke(this, new EventArgs());
            }
        }
        public string? Destination
        {
            get => destination;
            set
            {
                if (destination == value)
                    return;
                destination = value;
                FlightPlanUpdated?.Invoke(this, new EventArgs());
            }
        }
        public string? EntryFix
        {
            get => entryFix;
            set
            {
                if (entryFix == value)
                    return;
                entryFix = value;
                FlightPlanUpdated?.Invoke(this, new EventArgs());
            }
        }
        public string? ExitFix
        {
            get => exitFix;
            set
            {
                if (exitFix == value)
                    return;
                exitFix = value;
                FlightPlanUpdated?.Invoke(this, new EventArgs());
            }
        }
        public string? Route
        {
            get => route;
            set
            {
                if (route == value)
                    return;
                route = value;
                FlightPlanUpdated?.Invoke(this, new EventArgs());
            }
        }
        public int RequestedAltitude
        {
            get => requestedAltitude;
            set
            {
                if (requestedAltitude == value)
                    return;
                requestedAltitude = value;
                FlightPlanUpdated?.Invoke(this, new EventArgs());
            }
        }
        public string? Scratchpad1
        {
            get => scratchpad1;
            set
            {
                if (scratchpad1 == value)
                    return;
                scratchpad1 = value;
                FlightPlanUpdated?.Invoke(this, new EventArgs());
            }
        }
        public string? Scratchpad2
        {
            get => scratchpad2;
            set
            {
                if (scratchpad2 == value)
                    return;
                scratchpad2 = value;
                FlightPlanUpdated?.Invoke(this, new EventArgs());
            }
        }
        public string? Runway
        {
            get => runway;
            set
            {
                if (runway == value)
                    return;
                runway = value;
                FlightPlanUpdated?.Invoke(this, new EventArgs());
            }
        }
        public string? Owner
        {
            get => owner;
            set
            {
                if (owner == value)
                    return;
                owner = value;
                FlightPlanUpdated?.Invoke(this, new EventArgs());
            }
        }
        public string? PendingHandoff
        {
            get => pendingHandoff;
            set
            {
                if (pendingHandoff == value)
                    return;
                pendingHandoff = value;
                FlightPlanUpdated?.Invoke(this, new EventArgs());
            }
        }
        public string? AssignedSquawk
        {
            get => squawk;
            set
            {
                if (squawk == value)
                    return;
                squawk = value;
                FlightPlanUpdated?.Invoke(this, new EventArgs());
            }
        }

        public LDRDirection? LDRDirection
        {
            get => ldrDirection;
            set
            {
                if (ldrDirection == value)
                    return;
                ldrDirection = value;
                FlightPlanUpdated?.Invoke(this, new EventArgs());
            }
        }
        public Track? AssociatedTrack
        {
            get => associatedTrack;
            set
            {
                if (associatedTrack == value)
                    return;
                associatedTrack = value;
                FlightPlanUpdated?.Invoke(this, new EventArgs());
            }
        }

        public EventHandler FlightPlanUpdated;
    }
}
