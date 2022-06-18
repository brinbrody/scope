using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DGScope.Nexrad
{
    public abstract class SymbologyBlock
    {
        public int LayerDivider { get; set; }
        public int LayerLength { get; set; }
        public int LayerPacketCode { get; set; }
        public int Divider { get; set; }
        public int BlockID { get; set; }
        public int BlockLength { get; set; }
        public int NumOfLayers { get; set; }
        public abstract void ParseLayers(Stream fs, DescriptionBlock description_block);
    }

    public class RasterSymbologyBlock : SymbologyBlock
    {
        
        public int LayerPacketCode2 { get; set; }
        public int LayerPacketCode3 { get; set; }
        public int I_Coord_Start { get; set; }
        public int J_Coord_Start { get; set; }
        public int X_Scale_Int { get; set; }
        public int X_Scale_Fraction { get; set; }
        public int Y_Scale_Int { get; set; }
        public int Y_Scale_Fraction { get; set; }
        public int NumRows { get; set; }
        public int PackingDescriptor { get; set; }
        public Row[] Rows { get; set; }
        public override void ParseLayers(Stream fs, DescriptionBlock description_block)
        {
            RasterSymbologyBlock symbology_block = this;
            symbology_block.LayerDivider = NexradDecoder.ReadHalfWord(fs);
            symbology_block.LayerLength = NexradDecoder.ReadWord(fs);
            symbology_block.LayerPacketCode = NexradDecoder.ReadHalfWord(fs);
            symbology_block.LayerPacketCode2 = NexradDecoder.ReadHalfWord(fs);
            symbology_block.LayerPacketCode3 = NexradDecoder.ReadHalfWord(fs);
            symbology_block.I_Coord_Start = NexradDecoder.ReadHalfWord(fs);
            symbology_block.J_Coord_Start = NexradDecoder.ReadHalfWord(fs);
            symbology_block.X_Scale_Int = NexradDecoder.ReadHalfWord(fs);
            symbology_block.X_Scale_Fraction = NexradDecoder.ReadHalfWord(fs);
            symbology_block.Y_Scale_Int = NexradDecoder.ReadHalfWord(fs);
            symbology_block.Y_Scale_Fraction = NexradDecoder.ReadHalfWord(fs);
            symbology_block.NumRows = NexradDecoder.ReadHalfWord(fs);
            symbology_block.PackingDescriptor = NexradDecoder.ReadHalfWord(fs);
            symbology_block.Rows = new Row[symbology_block.NumRows];
            for (int i = 0; i < symbology_block.NumRows; i++)
            {
                var rowBytes = NexradDecoder.ReadHalfWord(fs);
                symbology_block.Rows[i] = new Row();
                symbology_block.Rows[i].Bytes = rowBytes;
                symbology_block.Rows[i].Data = new int[0];
                for (int j = 0; j < rowBytes; j++)
                {
                    var tempColorValues = NexradDecoder.ParseRLE(fs);
                    symbology_block.Rows[i].Data = ArrayMerge.Merge(symbology_block.Rows[i].Data, tempColorValues);
                }
            }
        }
    }

    public class RadialSymbologyBlock : SymbologyBlock
    {
        public int LayerIndexOfFirstRangeBin { get; set; }
        public int LayerNumberOfRangeBins { get; set; }
        public int I_CenterOfSweep { get; set; }
        public int J_CenterOFSweep { get; set; }
        public double ScaleFactor { get; set; }
        public int NumberOfRadials { get; set; }

        public Radial[] Radials { get; set; }

        public override void ParseLayers(Stream fs, DescriptionBlock description_block)
        {
            RadialSymbologyBlock symbology_block = this;
            symbology_block.LayerDivider = NexradDecoder.ReadHalfWord(fs);
            symbology_block.LayerLength = NexradDecoder.ReadWord(fs);
            symbology_block.LayerPacketCode = NexradDecoder.ReadHalfWord(fs);
            symbology_block.LayerIndexOfFirstRangeBin = NexradDecoder.ReadHalfWord(fs);
            symbology_block.LayerNumberOfRangeBins = NexradDecoder.ReadHalfWord(fs);
            symbology_block.I_CenterOfSweep = NexradDecoder.ReadHalfWord(fs);
            symbology_block.J_CenterOFSweep = NexradDecoder.ReadHalfWord(fs);
            symbology_block.ScaleFactor = NexradDecoder.ReadHalfWord(fs) / 1000;
            symbology_block.NumberOfRadials = NexradDecoder.ReadHalfWord(fs);
            symbology_block.Radials = new Radial[symbology_block.NumberOfRadials];
            for (int i = 0; i < symbology_block.NumberOfRadials; i++)
            {
                int bytes = NexradDecoder.ReadHalfWord(fs);
                double startangle = NexradDecoder.ReadHalfWord(fs) / 10;
                double angledelta = NexradDecoder.ReadHalfWord(fs) / 10;
                symbology_block.Radials[i] = new Radial();
                symbology_block.Radials[i].StartAngle = startangle;
                symbology_block.Radials[i].RadialBytes = bytes;
                symbology_block.Radials[i].AngleDelta = angledelta;
                if (symbology_block.LayerPacketCode == -20705)
                {
                    symbology_block.Radials[i].ColorValues = new int[0];
                    symbology_block.Radials[i].Values = new double[bytes];
                    for (int j = 0; j < bytes * 2; j++)
                    {
                        var tempcolorvalues = NexradDecoder.ParseRLE(fs);
                        symbology_block.Radials[i].ColorValues = ArrayMerge.Merge(symbology_block.Radials[i].ColorValues, tempcolorvalues);
                    }
                    symbology_block.Radials[i].Values = new double[symbology_block.Radials[i].ColorValues.Length];
                    for (int j = 0; j < symbology_block.Radials[i].ColorValues.Length; j++)
                    {
                        int value = description_block.Threshold[symbology_block.Radials[i].ColorValues[j]];
                        if (description_block.Mode == 1 && (description_block.Code >= 16 && description_block.Code <= 21))
                            value = ((int)(((double)value / 256.0) * 16));

                        symbology_block.Radials[i].Values[j] = value;
                    }
                }
                else if (symbology_block.LayerPacketCode == 16)
                {
                    double minval = (double)description_block.Threshold[0] / 10;
                    double increment = (double)description_block.Threshold[1] / 10;
                    symbology_block.Radials[i].ColorValues = new int[bytes];
                    symbology_block.Radials[i].Values = new double[bytes];
                    for (int j = 0; j < bytes; j++)
                    {
                        int value = NexradDecoder.ReadByte(fs);
                        symbology_block.Radials[i].ColorValues[j] = value / 16;
                        if (value > 2)
                            symbology_block.Radials[i].Values[j] = (increment * (value - 2)) + minval;
                    }
                }
            }
        }
    }

    public class Radial
    {
        public int[] ColorValues { get; set; }
        public double[] Values { get; set; }
        public int RadialBytes { get; set; }
        public double StartAngle { get; set; }
        public double AngleDelta { get; set; }

    }

    public class Row
    {
        public int[] Data { get; set; }
        public int Bytes { get; set; }
    }
}
