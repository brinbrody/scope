using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DGScope.Library
{
    public class FlightPlanUpdate : Update
    {
        private Guid flightPlanGuid = new Guid();
        [JsonIgnore]
        public FlightPlan FlightPlan { get; private set; }
        public string Callsign { get; set; }
        public string? AircraftType { get; set; }
        public string? WakeCategory { get; set; }
        public string? FlightRules { get; set; }
        public string? Origin { get; set; }
        public string? Destination { get; set; }
        public string? EntryFix { get; set; }
        public string? ExitFix { get; set; }
        public string? Route { get; set; }
        public int? RequestedAltitude { get; set; }
        public string? Scratchpad1 { get; set; }
        public string? Scratchpad2 { get; set; }
        public string? Runway { get; set; }
        public string? Owner { get; set; }
        public string? PendingHandoff { get; set; }
        public string? AssignedSquawk { get; set; }
        public string? EquipmentSuffix { get; set; }
        public LDRDirection? LDRDirection { get; set; }
        [JsonIgnore]
        public Track? AssociatedTrack { get; set; }
        public Guid? AssociatedTrackGuid
        {
            get
            {
                if (AssociatedTrack != null)
                    return AssociatedTrack.Guid;
                return null;
            }
        }
        public override Guid Guid 
        {
            get
            {
                if (FlightPlan != null)
                    return FlightPlan.Guid;
                return flightPlanGuid;
            }
            set
            {
                flightPlanGuid = value;
            }
        }

        public override UpdateType UpdateType => UpdateType.Flightplan;
        public FlightPlanUpdate(FlightPlan flightPlan, DateTime timestamp)
        {
            FlightPlan = flightPlan;
            TimeStamp = timestamp;
        }
        public FlightPlanUpdate(FlightPlan flightPlan)
        {
            FlightPlan = flightPlan;
        }
        public FlightPlanUpdate() { }
        public FlightPlanUpdate(FlightPlanUpdate update, FlightPlan flightPlan)
        {
            FlightPlan = flightPlan;
            TimeStamp = update.TimeStamp;
            Callsign = update.Callsign;
            AircraftType = update.AircraftType;
            WakeCategory = update.WakeCategory;
            FlightRules = update.FlightRules;
            Origin = update.Origin;
            Destination = update.Destination;
            EntryFix = update.EntryFix;
            ExitFix = update.ExitFix;
            Route = update.Route;
            RequestedAltitude = update.RequestedAltitude;
            Scratchpad1 = update.Scratchpad1;
            Scratchpad2 = update.Scratchpad2;
            Runway = update.Runway;
            Owner = update.Owner;
            PendingHandoff = update.PendingHandoff;
            AssignedSquawk = update.AssignedSquawk;
            LDRDirection = update.LDRDirection;
            EquipmentSuffix = update.EquipmentSuffix;
            AssociatedTrack = update.AssociatedTrack;
        }

        public override void RemoveUnchanged()
        {
            if (AircraftType == FlightPlan.AircraftType)
                AircraftType = null;
            if (WakeCategory == FlightPlan.WakeCategory)
                WakeCategory = null;
            if (FlightRules == FlightPlan.FlightRules)
                FlightRules = null;
            if (Origin == FlightPlan.Origin)
                Origin = null;
            if (Destination == FlightPlan.Destination)
                Destination = null;
            if (EntryFix == FlightPlan.EntryFix)
                EntryFix = null;
            if (ExitFix == FlightPlan.ExitFix)
                ExitFix = null;
            if (Route == FlightPlan.Route)
                Route = null;
            if (RequestedAltitude == FlightPlan.RequestedAltitude)
                RequestedAltitude = null;
            if (Scratchpad1 == FlightPlan.Scratchpad1)
                Scratchpad1 = null;
            if (Scratchpad2 == FlightPlan.Scratchpad2)
                Scratchpad2 = null;
            if (Runway == FlightPlan.Runway)
                Runway = null;
            if (Owner == FlightPlan.Owner)
                Owner = null;
            if (PendingHandoff == FlightPlan.PendingHandoff)
                PendingHandoff = null;
            if (AssignedSquawk == FlightPlan.AssignedSquawk)
                AssignedSquawk = null;
            if (LDRDirection == FlightPlan.LDRDirection)
                LDRDirection = null;
            if (EquipmentSuffix == FlightPlan.EquipmentSuffix)
                EquipmentSuffix = null;
        }
    }
}
