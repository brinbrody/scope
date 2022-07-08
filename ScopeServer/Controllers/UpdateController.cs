using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Threading.Tasks;
using DGScope.Library;
using System.Collections.ObjectModel;
using Microsoft.Net.Http.Headers;
using Microsoft.AspNetCore.Http;
using System.IO;
using DGScope.Receivers;
using DGScope.Receivers.FAA_STDDS;
using Newtonsoft.Json;
using System.Linq;
using BAMCIS.GeoJSON;
using System.Net.WebSockets;
using System.Text;
using System.Threading;

namespace ScopeServer.Controllers
{
    public class UpdateController : Controller
    {
        List<Update> PendingUpdates = new List<Update>();
        Facility selectedFacility;
        string selectedFacilityID;
        DateTime stateSent;
        

        [HttpGet]
        [Route("{facilityID}/updates")]
        public async Task GetUpdates(string facilityID)
        {
            SetFacilityID(facilityID);
            if (Response.HttpContext.WebSockets.IsWebSocketRequest)
            {
                using var webSocket = await HttpContext.WebSockets.AcceptWebSocketAsync();
                await StartUpdateSocket(webSocket);
            }
            else
            {
                this.Response.StatusCode = 200;
                this.Response.Headers.Add(HeaderNames.ContentType, "application/json");
                while (!HttpContext.RequestAborted.IsCancellationRequested)
                {
                    List<Update> sending;
                    lock (PendingUpdates)
                    {
                        sending = new List<Update>(PendingUpdates.ToList());
                        sending.ForEach(x => PendingUpdates.Remove(x));
                    }
                    sending.OrderBy(x => x.TimeStamp);
                    if (sending.Count == 0)
                        System.Threading.Thread.Sleep(100);
                    else
                    {
                        using (StreamWriter writer = new StreamWriter(this.Response.Body))
                        {
                            sending.ForEach(update => writer.WriteLineAsync(update.SerializeToJsonAsync().Result));
                        }
                    }
                }
            }
        }
        [HttpGet]
        [Route("{facilityID}/facilityState")]
        public void GetAdaptation(string facilityID)
        {
            this.Response.StatusCode = 200;
            this.Response.Headers.Add(HeaderNames.ContentType, "application/json");
            SetFacilityID(facilityID);
            using (StreamWriter writer = new StreamWriter(this.Response.Body))
            {
                writer.Write(JsonConvert.SerializeObject(selectedFacility));
            }
        }
        [HttpPost]
        [Route("{facilityID}/update")]
        public void PostUpdate(string facilityID)
        {
            Response.StatusCode = 403;
        }
        private void SetFacilityID(string facilityID)
        {
            selectedFacility = Settings.Facilities.Where(x => x.FacilityID == facilityID).FirstOrDefault();
            if (selectedFacility != null)
                AddFacilityWatchers(selectedFacility);
            else
            {
                selectedFacilityID = facilityID;
                Settings.Facilities.CollectionChanged += Facilities_CollectionChanged;
            }

        }

        private async Task StartUpdateSocket(WebSocket socket)
        {
            while (socket.State == WebSocketState.Connecting)
                System.Threading.Thread.Sleep(1000);
            while (socket.State == WebSocketState.Open)
            {
                List<Update> sending;
                lock (PendingUpdates)
                {
                    sending = new List<Update>(PendingUpdates.ToList());
                    sending.ForEach(x => PendingUpdates.Remove(x));
                }
                sending.OrderBy(x => x.TimeStamp);
                if (sending.Count == 0)
                    System.Threading.Thread.Sleep(100);
                else
                {
                    foreach (var item in sending)
                    {
                        var bytes = Encoding.UTF8.GetBytes(item.SerializeToJsonAsync().Result);
                        var segment = new ArraySegment<byte>(bytes);
                        await socket.SendAsync(segment, WebSocketMessageType.Text, true, CancellationToken.None);
                    }
                }
            }
            await socket.CloseAsync(WebSocketCloseStatus.Empty, "", CancellationToken.None);
        }

        
        private void AddFacilityWatchers(Facility facility)
        {
            lock (facility.Tracks)
            {
                lock (facility.FlightPlans)
                {
                    facility.Tracks.ToList().ForEach(x => PendingUpdates.Add(x.GetCompleteUpdate()));
                    facility.FlightPlans.ToList().ForEach(x => PendingUpdates.Add(x.GetCompleteUpdate()));
                    facility.Tracks.ToList().ForEach(x => x.Updated += UpdateReceived);
                    facility.FlightPlans.ToList().ForEach(x => x.Updated += UpdateReceived);
                    facility.Tracks.CollectionChanged += CollectionChanged;
                    facility.FlightPlans.CollectionChanged += CollectionChanged;
                }
            }
            stateSent = DateTime.UtcNow;
        }
        private void RemoveFacilityWatchers(Facility facility)
        {
            facility.Tracks.CollectionChanged -= CollectionChanged;
            facility.FlightPlans.CollectionChanged -= CollectionChanged;
        }
        private void Facilities_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    foreach (Facility item in e.NewItems)
                    {
                        if (item.FacilityID == selectedFacilityID)
                            AddFacilityWatchers(item);
                    }
                    break;
                case NotifyCollectionChangedAction.Remove:
                    foreach(Facility item in e.OldItems)
                    {
                        RemoveFacilityWatchers(item);
                    }
                    break;
            }
        }

        private void CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    foreach (IUpdatable item in e.NewItems)
                    {
                        PendingUpdates.Add(item.GetCompleteUpdate());
                        item.Updated += UpdateReceived;
                    }
                    break;
                case NotifyCollectionChangedAction.Remove:
                    foreach (IUpdatable item in e.OldItems)
                    {
                        item.Updated -= UpdateReceived;
                    }
                    break;
            }
        }

        private void UpdateReceived(object sender, UpdateEventArgs e)
        {
            if (e.Update.TimeStamp < stateSent)
                return;
            lock (PendingUpdates)
                PendingUpdates.Add(e.Update);
        }
    }

    
}
