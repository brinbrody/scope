using Amqp;
using Amqp.Sasl;
using System;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Security.Cryptography.X509Certificates;
using System.Net.Security;
using System.Xml.Serialization;
using System.IO;
using DGScope.Library;
using System.Collections.Generic;
using System.Linq;

namespace DGScope.Receivers.FAA_STDDS
{
    public class STDDSReceiver : Receiver
    {
        private Dictionary<string, Guid> trackLookup = new Dictionary<string, Guid>();
        private Dictionary<string, Guid> fpLookup = new Dictionary<string, Guid>();
        public string Host { get; set; }
        public string Username { get; set; }
        [PasswordPropertyText(true)]
        public string Password { get; set; }
        public string Queue { get; set; }
        public bool Forever { get; set; } = true;
        public int ClientTimeout { get; set; } = 5000;
        public int InitialCredit { get; set; } = 5;
        public string CertificatePath { get; set; } = @"scds.cert";

        TimeSpan timeout = TimeSpan.MaxValue;

        Connection connection = null;
        Session session;
        ReceiverLink receiver;
        XmlSerializer xmlSerializer = new XmlSerializer(typeof(TATrackAndFlightPlan));
        public override void Start()
        {
            Address address = new Address(Host, 5668, Username, Password, "/", "amqps");

            ConnectionFactory factory = new ConnectionFactory();
            factory.SSL.ClientCertificates.Add(new X509Certificate(CertificatePath));
            factory.SASL.Profile = SaslProfile.External;
            factory.SSL.RemoteCertificateValidationCallback = ValidateServerCertificate;
            connection = factory.CreateAsync(address).Result;

            Task.Run(ReceiveMessage);
        }
        public static Stream GenerateStreamFromString(string s)
        {
            var stream = new MemoryStream();
            var writer = new StreamWriter(stream);
            writer.Write(s);
            writer.Flush();
            stream.Position = 0;
            return stream;
        }

        private async Task<bool> ParseMessage(Message message)
        {
            try
            {
                TATrackAndFlightPlan data;
                using (var stream = GenerateStreamFromString(message.Body.ToString()))
                {
                    data = xmlSerializer.Deserialize(stream) as TATrackAndFlightPlan;
                }
                if (data.record == null)
                    return false;
                foreach (var record in data.record)
                {
                    if (stop)
                        return false;
                    List<Track> track = null;
                    FlightPlan flightPlan = null;
                    if (record.flightPlan != null)
                    {
                        lock (fpLookup)
                        {
                            if (fpLookup.TryGetValue($"{data.src}{record.flightPlan.sfpn}", out Guid guid))
                                flightPlan = GetFlightPlan(guid, data.src);
                            else
                            {
                                var facility = GetFacility(data.src);
                                var newfp = new FlightPlan(record.flightPlan.acid.Trim());
                                lock (facility.FlightPlans)
                                {
                                    facility.FlightPlans.Add(newfp);
                                    fpLookup.Add($"{data.src}{record.flightPlan.sfpn}", newfp.Guid);
                                }
                                flightPlan = newfp;
                            }
                        }
                    }
                    if (record.track != null)
                    {
                        lock (trackLookup)
                        {
                            if (trackLookup.TryGetValue($"{data.src}{record.track.trackNum}", out Guid guid))
                                track = GetTracks(guid, data.src);
                            else
                            {
                                var facility = GetFacility(data.src);
                                var newtrack = new Track(facility);
                                lock (facility.Tracks)
                                {
                                    facility.Tracks.Add(newtrack);
                                    trackLookup.Add($"{data.src}{record.track.trackNum}", newtrack.Guid);
                                }
                                track = new List<Track>();
                                track.Add(newtrack);
                            }
                        }
                            
                    }
                    if (track != null && record.track != null)
                    {
                        TrackUpdate update = new TrackUpdate();
                        update.TimeStamp = record.track.mrtTime;
                        if (record.track.reportedBeaconCode > 0)
                            update.Squawk = record.track.reportedBeaconCode.ToString("0000");
                        update.Location = new GeoPoint((double)record.track.lat, (double)record.track.lon);
                        update.GroundTrack = GroundTrack(record.track.vx, record.track.vy);
                        update.GroundSpeed = GroundSpeed(record.track.vx, record.track.vy);
                        if (update.Altitude == null)
                            update.Altitude = new Altitude();
                        update.Altitude.TrueAltitude = record.track.reportedAltitude; 
                        var addressString = record.track.acAddress;
                        int address = 0;
                        if (addressString != "")
                            address = Convert.ToInt32(record.track.acAddress, 16);
                        if (address != 0)
                            update.ModeSCode = address;
                        SendTrackUpdates(track, update);
                    }
                    if (flightPlan != null && record.flightPlan != null)
                    {
                        FlightPlanUpdate update = new FlightPlanUpdate(flightPlan);
                        update.TimeStamp = DateTime.UtcNow;
                        var scddsfp = record.flightPlan;
                        if (scddsfp.assignedBeaconCode == 0)
                            update.AssignedSquawk = scddsfp.assignedBeaconCode.ToString();
                        update.AircraftType = scddsfp.acType;
                        update.LDRDirection = ParseLDR(scddsfp.lld);
                        if (scddsfp.scratchPad1 != null)
                            update.Scratchpad1 = scddsfp.scratchPad1;
                        else
                            update.Scratchpad1 = string.Empty;
                        if (scddsfp.scratchPad2 != null)
                            update.Scratchpad2 = scddsfp.scratchPad2;
                        else
                            update.Scratchpad2 = string.Empty;
                        update.RequestedAltitude = (int)scddsfp.requestedAltitude;
                        update.WakeCategory = scddsfp.category;
                        update.Destination = scddsfp.exitFix;
                        update.Origin = scddsfp.entryFix;
                        update.ExitFix = scddsfp.exitFix;
                        update.EntryFix = scddsfp.entryFix;
                        update.FlightRules = scddsfp.flightRules;
                        update.Callsign = scddsfp.acid.Trim();
                        update.EquipmentSuffix = scddsfp.eqptSuffix;
                        switch (scddsfp.ocr)
                        {
                            case "intrafacility handoff":
                            case "normal handoff":
                            case "manual":
                            case "no change":
                            case "consolidation":
                            case "directed handoff":
                                update.Owner = scddsfp.cps;
                                update.PendingHandoff = string.Empty;
                                break;
                            case "pending":
                                update.PendingHandoff = scddsfp.cps;
                                break;
                        }
                        if (track != null)
                            update.AssociatedTrack = track.FirstOrDefault();
                        if (track != null && track.Count > 1)
                            ;
                        flightPlan.UpdateFlightPlan(update);
                    }
                    else
                        continue;

                }

            }

            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                Console.WriteLine(message);
                return false;
            }
            return true;
        }
        private async Task<bool> ReceiveMessage()
        {
            bool stop = false;
            session = new Session(connection);
            receiver = new ReceiverLink(session, "amqpConsumer", Queue);

            if (!Forever)
                timeout = TimeSpan.FromSeconds(ClientTimeout);
            while (!stop)
            {
                var message = receiver.Receive(timeout);
                if (message == null)
                    continue;
                receiver.Accept(message);
                Task.Run(() => ParseMessage(message));
            }
            return true;
        }

        private LDRDirection? ParseLDR(string lld)
        {
            if (lld == null || lld == "")
                return null;
            switch (lld.ToUpper())
            {
                case "N":
                    return LDRDirection.N;
                case "NE":
                    return LDRDirection.NE;
                case "E":
                    return LDRDirection.E;
                case "SE":
                    return LDRDirection.SE;
                case "S":
                    return LDRDirection.S;
                case "SW":
                    return LDRDirection.SW;
                case "W":
                    return LDRDirection.W;
                case "NW":
                    return LDRDirection.NW;
                default:
                    return null;
            }
        }

        private int GroundTrack(double vx, double vy)
        {
            double angle = Math.Atan2(vy, vx) * (180 / Math.PI);
            double track = 0;
            if (!double.IsNaN(angle))
                track = (450 - angle) % 360;
            return (int)track;
        }
        private int GroundSpeed(double vx, double vy)
        {
            return (int)Math.Sqrt((vx * vx) + (vy * vy));
        }
        static bool ValidateServerCertificate(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
        {
            if (sslPolicyErrors == System.Net.Security.SslPolicyErrors.None)
                return true;
            else
                return false;
        }

        bool stop = false;

        public override void Stop()
        {
            stop = true;
            if (receiver != null)
                receiver.Close();
            if (session != null)
                session.Close();
            if (connection != null)
                connection.Close();
        }
        public STDDSReceiver() { }
    }
}
