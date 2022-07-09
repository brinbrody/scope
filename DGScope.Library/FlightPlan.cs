using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace DGScope.Library
{
    public class FlightPlan :IUpdatable
    {
        private DateTime created = DateTime.UtcNow;
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

        public string sfpn { get; set; }
        public Guid Guid { get; set; } = Guid.NewGuid();
        public DateTime LastMessageTime { get; private set; } = DateTime.UtcNow;
        public LDRDirection? LDRDirection { get; private set; }
        public Track? AssociatedTrack { get; private set; }
        public Dictionary<string, DateTime> PropertyUpdatedTimes { get; } = new Dictionary<string, DateTime>();
        public FlightPlan(string Callsign) 
        {
            this.Callsign = Callsign;
            Created?.Invoke(this, new FlightPlanUpdatedEventArgs(GetCompleteUpdate()));
        }
        public FlightPlan(Guid guid)
        {
            this.Guid = guid;
            Created?.Invoke(this, new FlightPlanUpdatedEventArgs(GetCompleteUpdate()));
        }

        public void UpdateFlightPlan(FlightPlanUpdate update)
        {
            update.RemoveUnchanged();
            if (update.TimeStamp > LastMessageTime)
                LastMessageTime = update.TimeStamp;

            bool changed = false;
            foreach (var updateProperty in update.GetType().GetProperties())
            {
                PropertyInfo thisProperty = GetType().GetProperty(updateProperty.Name);
                object updateValue = updateProperty.GetValue(update);
                if (updateValue == null || thisProperty == null)
                    continue;
                object thisValue = thisProperty.GetValue(this);
                if (!PropertyUpdatedTimes.TryGetValue(thisProperty.Name, out DateTime lastUpdatedTime))
                    PropertyUpdatedTimes.TryAdd(thisProperty.Name, update.TimeStamp);
                if (update.TimeStamp > lastUpdatedTime && thisProperty.CanWrite && !Equals(thisValue, updateValue))
                {
                    thisProperty.SetValue(this, updateValue);
                    PropertyUpdatedTimes[thisProperty.Name] = update.TimeStamp;
                    changed = true;
                }
            }
            if (changed)
            {
                Updated?.Invoke(this, new FlightPlanUpdatedEventArgs(update));
            }
            return;
        }
        public Update GetCompleteUpdate()
        {
            var newUpdate = new FlightPlanUpdate(this);
            newUpdate.SetAllProperties();
            return newUpdate;
        }
        public override string ToString()
        {
            return Callsign;
        }

        public void InvokeDeleted()
        {
            AssociatedTrack = null;
            Deleted?.Invoke(this, null);
        }

        public event EventHandler<UpdateEventArgs> Updated;
        public event EventHandler<UpdateEventArgs> Created;
        public event EventHandler<EventArgs> Deleted;
    }

    public class FlightPlanUpdatedEventArgs : UpdateEventArgs
    {
        public FlightPlan FlightPlan { get; private set; }
        public FlightPlanUpdatedEventArgs(FlightPlan flightPlan)
        {
            FlightPlan = flightPlan;
            Update = flightPlan.GetCompleteUpdate();
        }
        public FlightPlanUpdatedEventArgs(Update update)
        {
            FlightPlan = update.Base as FlightPlan;
            Update = update;
        }
    }
}
