using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DGScope.Library
{
    public class FlightPlan :IUpdatable
    {
        public string Callsign { get; private set; }
        public string? AircraftType { get; private set; }
        public string? WakeCategory { get; private set; }
        public string? FlightRules { get; private set; }
        public string? Origin { get; private set; }
        public string? Destination { get; private set; }
        public string? EntryFix { get; private set; }
        public string? ExitFix { get; private set; }
        public string? Route { get; private set; }
        public int RequestedAltitude { get; private set; }
        public string? Scratchpad1 { get; private set; }
        public string? Scratchpad2 { get; private set; }
        public string? Runway { get; private set; }
        public string? Owner { get; private set; }
        public string? PendingHandoff { get; private set; }
        public string? AssignedSquawk { get; private set; }
        public string? EquipmentSuffix { get; private set; }

        public Guid Guid { get; set; } = Guid.NewGuid();
        public DateTime LastMessageTime { get; private set; } = DateTime.MinValue;
        public LDRDirection? LDRDirection { get; private set; }
        public Track? AssociatedTrack { get; private set; }

        public FlightPlan(string Callsign) 
        {
            this.Callsign = Callsign;
            Created?.Invoke(this, new FlightPlanUpdatedEventArgs(GetCompleteFlightPlanUpdate()));
        }
        public FlightPlan(Guid guid)
        {
            this.Guid = guid;
            Created?.Invoke(this, new FlightPlanUpdatedEventArgs(GetCompleteFlightPlanUpdate()));
        }

        public void UpdateFlightPlan(FlightPlanUpdate update)
        {
            update.RemoveUnchanged();
            if (update.TimeStamp < LastMessageTime)
                return;
            LastMessageTime = update.TimeStamp;
            bool changed = false;
            if (update.Callsign != null)
            {
                changed = true;
                Callsign = update.Callsign;
            }
            if (update.AircraftType != null)
            {
                changed = true;
                AircraftType = update.AircraftType;
            }
            if (update.WakeCategory != null)
            {
                changed = true;
                WakeCategory = update.WakeCategory;
            }
            if (update.FlightRules != null)
            {
                changed = true;
                FlightRules = update.FlightRules;
            }
            if (update.Origin != null)
            {
                changed = true;
                Origin = update.Origin;
            }
            if (update.Destination != null)
            {
                changed = true;
                Destination = update.Destination;
            }
            if (update.EntryFix != null)
            {
                changed = true;
                EntryFix = update.EntryFix;
            }
            if (update.ExitFix != null)
            {
                changed = true;
                EntryFix = update.ExitFix;
            }
            if (update.Route != null)
            {
                changed = true;
                Route = update.Route;
            }
            if (update.RequestedAltitude != null)
            {
                changed = true;
                RequestedAltitude = (int)update.RequestedAltitude;
            }
            if (update.Scratchpad1 != null)
            {
                changed = true;
                Scratchpad1 = update.Scratchpad1;
            }
            if (update.Scratchpad2 != null)
            {
                changed = true;
                Scratchpad2 = update.Scratchpad2;
            }
            if (update.Runway != null)
            {
                changed = true;
                Runway = update.Runway;
            }
            if (update.Owner != null)
            {
                changed = true;
                Owner = update.Owner;
            }
            if (update.PendingHandoff != null)
            {
                changed = true;
                PendingHandoff = update.PendingHandoff;
            }
            if (update.AssignedSquawk != null)
            {
                changed = true;
                AssignedSquawk = update.AssignedSquawk;
            }
            if (update.LDRDirection != null)
            {
                changed = true;
                LDRDirection = update.LDRDirection;
            }
            if (update.EquipmentSuffix != null)
            {
                changed = true;
                EquipmentSuffix = update.EquipmentSuffix;
            }
            AssociatedTrack = update.AssociatedTrack;
            if (changed)
                Updated?.Invoke(this, new FlightPlanUpdatedEventArgs(update));
        }
        public FlightPlanUpdate GetCompleteFlightPlanUpdate()
        {
            return new FlightPlanUpdate(this)
            {
                AircraftType = this.AircraftType,
                WakeCategory = this.WakeCategory,
                FlightRules = this.FlightRules,
                Origin = this.Origin,
                Destination = this.Destination,
                EntryFix = this.EntryFix,
                ExitFix = this.ExitFix,
                Route = this.Route,
                RequestedAltitude = this.RequestedAltitude,
                Scratchpad1 = this.Scratchpad1,
                Scratchpad2 = this.Scratchpad2,
                Runway = this.Runway,
                Owner = this.Owner,
                PendingHandoff = this.PendingHandoff,
                AssignedSquawk = this.AssignedSquawk,
                LDRDirection = this.LDRDirection,
                AssociatedTrack = this.AssociatedTrack,
                Callsign = this.Callsign,
                EquipmentSuffix = this.EquipmentSuffix,
                TimeStamp = DateTime.Now,
            };
        }
        public override string ToString()
        {
            return Callsign;
        }
        public event EventHandler<UpdateEventArgs> Updated;
        public event EventHandler<UpdateEventArgs> Created;
    }

    public class FlightPlanUpdatedEventArgs : UpdateEventArgs
    {
        public FlightPlan FlightPlan { get; private set; }
        public FlightPlanUpdatedEventArgs(FlightPlan flightPlan)
        {
            FlightPlan = flightPlan;
            Update = flightPlan.GetCompleteFlightPlanUpdate();
        }
        public FlightPlanUpdatedEventArgs(FlightPlanUpdate update)
        {
            FlightPlan = update.FlightPlan;
            Update = update;
        }
    }
}
