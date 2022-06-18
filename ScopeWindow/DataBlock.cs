using DGScope.Library;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScopeWindow
{
    public abstract class DataBlock
    {
        public abstract Bitmap GetBitmap();
    }
    public class FullDataBlock : DataBlock
    {
        // Full data blocks are shown for associated tracks (tracks with a flight plan) under
        // any of the following conditions:
        //
        // +  track is controlled at this TCW/TDW
        // +  track (unowned) has been "Quick Looked" at this TCW/TDW
        // +  track meets adapted criteria and is in an enabled Quicklook region
        // +  track is involved in handoff with this TCW/TDW
        // +  track is squawking a Special Condition beacon code (Lost Link, General
        //    Emergency, Hijack, Radio Communications Failure, or Military Intercept)
        // +  track has Minimum Save Altitude Warning (MSAW) or Conflict Alert (CA)
        //    indicators
        // +  Track is displaying an Automated Terminal Proximity Alert (ATPA) Caution
        //    or Alert Cone
        // +  track is in an ATPA Warning or Alert condition, the display of ATPA In-trail
        //    Distance is enabled, and the TCP is adapted to display ATPA In-trail
        //    Warning and Alerts for the ATPA Approach Volume
        public override Bitmap GetBitmap()
        {
            throw new NotImplementedException();
        }
    }
    public class PartialDataBlock : DataBlock
    {
        // Partial data blocks are shown for associated tracks that are not owned by this
        // TCW/TDW and are within this display's altitude filter limits for associated
        // tracks, or within an adapted an enabled Altitude filter override volume for the
        // TCP. The Partial data block typically includes the Mode-C altitude and aircraft
        // category identifier.
        public override Bitmap GetBitmap()
        {
            throw new NotImplementedException();
        }
    }
    public class LimitedDataBlock : DataBlock
    {
        // Limited data block are shown for unassociated tracks (tracks without a flight
        // plan) that are within this display's altitude filter limits for unassociated tracks or
        // are within an adapted and enabled Altitude Filter Override volume for the TCP,
        // and the track is established. Limited data blocks may include the following data:
        //
        // +  SPC mnemonic if the tracks is squawking a Special Condition beacon code
        //    (General Emergency, Hijack, Radio Communications Failure, or Lost Link
        // +  Conflict alert indicator ("CA") if the track is involved in Mode-C Intruder
        //    (MCI) conflict alert with an owned associated track
        // +  Ident indicator ("ID") if Special Position Indicator (SPI) is received for the
        //    track.
        // +  An ACID when in EFSL mode, as long as the track's ACID has been
        //    provided by FSL.
        public override Bitmap GetBitmap()
        {
            throw new NotImplementedException();
        }
    }
    
}
