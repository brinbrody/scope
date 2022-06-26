using Newtonsoft.Json;
using System;

namespace DGScope.Library
{
    public class Altitude
    {
        public int Value { get; set; }
        public AltitudeType AltitudeType { get; set; }
        [JsonIgnore]
        public int TransitionAltitude { get; set; }
        [JsonIgnore]
        public int PressureAltitude
        {
            get
            {
                if (AltitudeType == AltitudeType.Pressure)
                    return Value;
                var correction = (int)((Altimeter.Value - 29.92) * 1000);
                var newvalue = Value;
                if (this.AltitudeType == AltitudeType.True)
                    newvalue -= correction;
                return newvalue;
            }
            set
            {
                UpdateAltitude(value, AltitudeType.Pressure);
            }
        }
        [JsonIgnore]
        public int TrueAltitude
        {
            get
            {
                if (AltitudeType == AltitudeType.True)
                    return Value;
                var correction = (int)((Altimeter.Value - 29.92) * 1000);
                var newvalue = Value;
                if (this.AltitudeType == AltitudeType.Pressure)
                    newvalue += correction;
                return newvalue;
            }
            set
            {
                UpdateAltitude(value, AltitudeType.True);
            }
        }

        private object convertLockObject = new object();


        public void SetAltitudeProperties(int TransitionAltitude, Altimeter Altimeter)
        {
            this.TransitionAltitude = TransitionAltitude;
            this.Altimeter = Altimeter;
        }

        public void UpdateAltitude(int Value, AltitudeType type)
        {
            lock (convertLockObject)
            {
                this.Value = Value;
                AltitudeType = type;
            }
        }

        private Altimeter Altimeter;
        public Altitude ConvertTo(AltitudeType type)
        {
            lock (convertLockObject)
            {
                if (type != AltitudeType)
                {
                    if (type == AltitudeType.Pressure)
                    {
                        if (this.AltitudeType == AltitudeType.True)
                        {
                            Value = PressureAltitude;
                            AltitudeType = AltitudeType.Pressure;
                        }
                    }
                    else if (type == AltitudeType.True)
                    {
                        if (this.AltitudeType == AltitudeType.Pressure)
                        {
                            Value = TrueAltitude;
                            AltitudeType = AltitudeType.True;
                        }
                    }
                    else
                    {
                        throw new NotImplementedException();
                    }
                }
                return this;
            }
        }
        public override string ToString()
        {
            if (ConvertTo(AltitudeType.True).Value > TransitionAltitude)
            {
                return string.Format("FL{0}", (Value / 100).ToString("D3"));
            }
            return string.Format("{0}ft.", Value);
        }
        public Altitude(int transitionAltitude, Altimeter altimeter) 
        { 
            Altimeter = altimeter;
            TransitionAltitude = transitionAltitude;
        }

        public Altitude()
        {
        }

        public Altitude Clone()
        {
            var newalt = new Altitude(this.TransitionAltitude, this.Altimeter);
            newalt.Value = Value;
            newalt.AltitudeType = AltitudeType;
            return newalt;
        }
        public override bool Equals(object obj)
        {
            if (obj == null || obj.GetType() != typeof(Altitude))
                return false;
            return this.TrueAltitude == (obj as Altitude).TrueAltitude;
        }
    }
    public enum AltitudeType
    {
        Pressure, True
    }
}
