//
// Created by Chris Harrell, 09/10/2010
//
// Base NexradDecoder class.

using ICSharpCode.SharpZipLib.BZip2;
using System;
using System.IO;

namespace NexradDecoder
{
    public class NexradDecoder
    {

        protected Stream fs;

        public HeaderBlock HeaderBlock { get; private set; }
        public int Range { get; private set; }
        const int msg_header_block_offset = 30;

        public DescriptionBlock DescriptionBlock { get; private set; }
        const int description_block_offset = 48;

        public SymbologyBlock SymbologyBlock { get; private set; }
        int symbology_block_offset;

        int graphic_block_offset;

        ///////////////////////////////////////////// 
        /* This constructor is executed when the   */
        /* object is first created                 */
        ///////////////////////////////////////////// 
        public NexradDecoder()
        {
                                      // Initialize method variables
        }

        public void Parse(string fileName)
        {
            Stream fs;
            if (File.Exists(fileName))
            {
                fs = new FileStream(fileName, FileMode.Open, FileAccess.Read);
                Parse(fs);
            }
            throw new FileNotFoundException();
        }

        public void Parse(Stream Stream)
        {
            if (Stream.CanSeek)
                fs = Stream;
            else
            {
                fs = new MemoryStream();
                Stream.CopyTo(fs);
            }
            HeaderBlock = ParseMHB(fs);
            DescriptionBlock = ParsePDB(fs);
            switch (HeaderBlock.Code)
            {
                case 94:
                    Range = 248;
                    SymbologyBlock = new RadialSymbologyBlock();
                    break;
                case 180:
                    Range = 48;
                    SymbologyBlock = new RadialSymbologyBlock();
                    break;
                default:
                    throw new NotImplementedException(string.Format("Product {0} is not supported", HeaderBlock.Code));
            }
            ParsePSB();
        }
               

        ///////////////////////////////////////////// 
        /* Read a byte (1 byte)              */
        ///////////////////////////////////////////// 
        public static int ReadByte(Stream fs, bool negativeRange = false)
        {
            byte[] bytes = new byte[1];
            fs.Read(bytes, 0, 1);
            return bytes[0];
        }

        ///////////////////////////////////////////// 
        /* Read a half word (2 bytes)              */
        ///////////////////////////////////////////// 
        public static int ReadHalfWord(Stream fs, bool negativeRange = false)
        {
            byte[] bytes = new byte[2];
            fs.Read(bytes, 0, 2);
            Array.Reverse(bytes);
            return BitConverter.ToInt16(bytes, 0);
        }


        ///////////////////////////////////////////// 
        /* Read a two halfwords (4 bytes)          */
        /////////////////////////////////////////////
        public static int ReadWord(Stream fs, bool negativeRange = false)
        {
            byte[] bytes = new byte[4];
            fs.Read(bytes, 0, 4);
            Array.Reverse(bytes);
            return BitConverter.ToInt32(bytes, 0);


            int result = bytes[0] << 32;
            result += bytes[1] << 24;
            result += bytes[2] << 16;
            result += bytes[3];
            return result;
        }

        ///////////////////////////////////////////// 
        /* Read 4 bit RLE data                     */
        /////////////////////////////////////////////
        public static int[] ParseRLE(Stream fs)
        {
            byte[] dataArray = new byte[1];
            fs.Read(dataArray, 0, 1);
            var data = dataArray[0].ToString("X").PadLeft(2, '0');
            var split_data = data.ToCharArray();

            int length = int.Parse(split_data[0].ToString(), System.Globalization.NumberStyles.HexNumber);
            int value = int.Parse(split_data[1].ToString(), System.Globalization.NumberStyles.HexNumber);
            int[] valueArray = new int[length];

            // Reduce the color values if the radar is in clean air mode and the current product is one of many Base Reflectivity products
            //if (description_block.Mode == 1 && (description_block.Code >= 16 && description_block.Code <= 21))
            //{
            //    if (value >= 8) value -= 8;
            //    else if (value < 8) value = 0;
            //}

            for (int i = 1; i < length; i++)
            {
                valueArray[i] = value;
            }

            return valueArray;

        }

        public static string sec2hms(int sec, bool padHours = false)
        {

            string hms = "";                                               // start with a blank string
            int hours = (int)(sec / 3600);

            hms += padHours ? hours.ToString().PadLeft(2, '0') : hours.ToString();

            int minutes = (int)((sec / 60) % 60);
            hms += ":" + minutes.ToString().PadLeft(2, '0');
            int seconds = (int)(sec % 60);
            hms += ":" + seconds.ToString().PadLeft(2, '0');

            return hms;

        }

        /////////////////////////////////////////////
        /* Parse the Graphic Alphanumeric Block    */
        /* pages into an array and return it.  To  */
        /* be called by the parseGAB() method.     */
        /////////////////////////////////////////////
        public static Page ParsePage(Stream fs)
        {
            Page page = new Page();
            page.Number = ReadHalfWord(fs);
            page.Length = ReadHalfWord(fs);
            int totalBytesToRead = page.Length;
            int vectorID = 0;
            int messageID = 0;

            while (totalBytesToRead > 0)
            {
                int packetCode = ReadHalfWord(fs);
                int packetLength = ReadHalfWord(fs);

                // If the packet code is 8 then decode it as a Text & Special Symbol Packet
                if (packetCode == 8)
                {
                    messageID++;
                    Message message = new Message();
                    message.TextColor = ReadHalfWord(fs);
                    message.PosI = ReadHalfWord(fs, true);
                    message.PosJ = ReadHalfWord(fs, true);
                    message.MessageText = null;

                    // We have already 6 bytes of this packet.  Subtract it from the amount of 
                    // bytes thare still need to be read.
                    int packetBytesToRead = packetLength - 6;

                    // Read the remaining bytes (packetBytesToRead) to obtain the actual message
                    // that is encoded in the packet
                    for (int j = 0; j < packetBytesToRead; j++)
                    {
                        message.MessageText += (char)ReadByte(fs);
                    }

                    // Subtract the total length of the packet (packetLength) from the total bytes
                    // in the page (totalBytesToRead).  We must account for the 4 bytes that were
                    // read while reading the packet code and packet length, because they are not included
                    // in the Packet Length.
                    totalBytesToRead -= (packetLength + 4);
                    page.Data.Messages.Add(message);
                }


                // If the packet code is 10 then decode it as a Unlinked Vector Packet
                else if (packetCode == 10)
                {
                    Vector vector = new Vector();
                    vector.Color = ReadHalfWord(fs);

                    // We have already 2 bytes of this packet.  Subtract it from the amount of 
                    // bytes thare still need to be read.
                    int packetBytesToRead = packetLength - 2;

                    vectorID = 0;
                    while (packetBytesToRead > 0)
                    {
                        vectorID++;

                        vector.PosIBegin = ReadHalfWord(fs, true);
                        vector.PosIBegin = ReadHalfWord(fs, true);
                        vector.PosIEnd = ReadHalfWord(fs, true);
                        vector.PosJEnd = ReadHalfWord(fs, true);
                        page.Data.Vectors.Add(vector);
                        // Subtract the 8 bytes that we just read from the amount of packet bytes remaining 
                        // to be read (packetBytesToRead).
                        packetBytesToRead -= 8;
                    }

                    // Subtract the total length of the packet (packetLength) from the total bytes
                    // in the page (totalBytesToRead).  We must account for the 4 bytes that were
                    // read while reading the packet code and packet length, because they are not included
                    // in the Packet Length.
                    totalBytesToRead -= (packetLength + 4);
                }

            }
            return page;
        }

        /////////////////////////////////////////////
        /* Parse the Message Header Block into an  */
        /* array and return it.                    */
        /////////////////////////////////////////////
        public static HeaderBlock ParseMHB(Stream fs)
        {
            HeaderBlock msg_header_block = new HeaderBlock();
            fs.Seek(msg_header_block_offset, SeekOrigin.Begin);
            msg_header_block.Code = ReadHalfWord(fs);                            // HW 1
            msg_header_block.DateTime = ReadTimeStamp(fs); // convertFromJulian((int)(ReadHalfWord(fs) + 2440586.5)); // HW 2
            msg_header_block.Length = ReadWord(fs);                              // HW 5 & HW 6
            msg_header_block.sourceID = ReadHalfWord(fs);                        // HW 7
            msg_header_block.destinationID = ReadHalfWord(fs);                   // HW 8
            msg_header_block.numberofBlocks = ReadHalfWord(fs);                  // HW 9
            return msg_header_block;
        }
        static DateTime UnixTimeStampToDateTime(double unixTimeStamp)
        {
            // Unix timestamp is seconds past epoch
            System.DateTime dtDateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, System.DateTimeKind.Utc);
            dtDateTime = dtDateTime.AddSeconds(unixTimeStamp).ToLocalTime();
            return dtDateTime.AddDays(-1);
        }

        public static DateTime ReadTimeStamp(Stream fs)
        {
            DateTime dateTime = UnixTimeStampToDateTime(ReadHalfWord(fs) * 86400);
            var seconds = ReadWord(fs);
            dateTime = dateTime.AddSeconds(seconds);
            return dateTime;
        }
        private DateTime convertFromJulian(int jDate)
        {
            int day = jDate % 1000;
            int year = (jDate - day) / 1000;
            var date1 = new DateTime(year, 1, 1);
            return date1.AddDays(day - 1);
        }

        /////////////////////////////////////////////
        /* Parse the Product Description Block     */
        /* into an array and return it.            */
        /////////////////////////////////////////////
        public static DescriptionBlock ParsePDB(Stream fs)
        {
            DescriptionBlock description_block = new DescriptionBlock();
            fs.Seek(description_block_offset, SeekOrigin.Begin);

            description_block.Divider = ReadHalfWord(fs, true);                                //HW 10	
            description_block.Latitude = (double)ReadWord(fs) / 1000;                        //HW 11 - 12
            description_block.Longitude = (double)ReadWord(fs) / 1000;                       //HW 13 - 14
            description_block.Height = ReadHalfWord(fs, true);                                 //HW 15
            description_block.Code = ReadHalfWord(fs, true);                                   //HW 16
            description_block.Mode = ReadHalfWord(fs);                                       //HW 17
            description_block.VolumeCoveragePattern = ReadHalfWord(fs);                      //HW 18
            description_block.SequenceNumber = ReadHalfWord(fs);                             //HW 19

            description_block.ScanNumber = ReadHalfWord(fs);                                 //HW 20
            description_block.ScanTime = ReadTimeStamp(fs);                                  //HW 22 - 23
            description_block.GenerationTime = ReadTimeStamp(fs);                            //HW 25 - 26
            description_block.ProductSpecific_1 = ReadHalfWord(fs);                          //HW 27
            description_block.ProductSpecific_2 = ReadHalfWord(fs);                          //HW 28
            description_block.ElevationNumber = ReadHalfWord(fs);                            //HW 29

            description_block.ProductSpecific_3 = ReadHalfWord(fs) / 10;                      //HW 30
            description_block.Threshold[0] = ReadHalfWord(fs);                                //HW 31
            description_block.Threshold[1] = ReadHalfWord(fs);                                //HW 32
            description_block.Threshold[2] = ReadHalfWord(fs);                                //HW 33
            description_block.Threshold[3] = ReadHalfWord(fs);                                //HW 34
            description_block.Threshold[4] = ReadHalfWord(fs);                                //HW 35
            description_block.Threshold[5] = ReadHalfWord(fs);                                //HW 36
            description_block.Threshold[6] = ReadHalfWord(fs);                                //HW 37
            description_block.Threshold[7] = ReadHalfWord(fs);                                //HW 38
            description_block.Threshold[8] = ReadHalfWord(fs);                                //HW 39
                                       
            description_block.Threshold[9] = ReadHalfWord(fs);                               //HW 40
            description_block.Threshold[10] = ReadHalfWord(fs);                               //HW 41
            description_block.Threshold[11] = ReadHalfWord(fs);                               //HW 42
            description_block.Threshold[12] = ReadHalfWord(fs);                               //HW 43
            description_block.Threshold[13] = ReadHalfWord(fs);                               //HW 44
            description_block.Threshold[14] = ReadHalfWord(fs);                               //HW 45
            description_block.Threshold[15] = ReadHalfWord(fs);                               //HW 46
            description_block.ProductSpecific_4 = ReadHalfWord(fs);                          //HW 47
            description_block.ProductSpecific_5 = ReadHalfWord(fs);                          //HW 48
            description_block.ProductSpecific_6 = ReadHalfWord(fs);                          //HW 49

            description_block.ProductSpecific_7 = ReadHalfWord(fs);                          //HW 50
            description_block.ProductSpecific_8 = ReadHalfWord(fs);                          //HW 51
            description_block.ProductSpecific_9 = ReadHalfWord(fs);                          //HW 52
            description_block.ProductSpecific_10 = ReadHalfWord(fs);                         //HW 53
            description_block.Version = ReadByte(fs);                                        //HW 54
            ReadByte(fs);
            description_block.SymbologyOffset = ReadWord(fs);                                //HW 55 - 56
            description_block.GraphicOffset = ReadWord(fs);                                  //HW 57 - 58
            description_block.TabularOffset = ReadWord(fs);                                  //HW 59 - 60
            return description_block;
        }


        /////////////////////////////////////////////
        /* Parse the Product Symbology Block into  */
        /* an array and return it.                 */
        /////////////////////////////////////////////
        public void ParsePSB()
        {
            symbology_block_offset = (DescriptionBlock.SymbologyOffset * 2) + msg_header_block_offset;
            fs.Seek(symbology_block_offset, SeekOrigin.Begin);
            if (DescriptionBlock.ProductSpecific_8 == 1) // This means the Symbology is BZip2 compressed
            {
                using (MemoryStream copy = new MemoryStream()) 
                {
                    fs.CopyTo(copy);
                    copy.Seek(0, SeekOrigin.Begin);
                    using (BZip2InputStream inputStream = new BZip2InputStream(copy))
                    {
                        var fslength = fs.Length;
                        fs.Seek(0, SeekOrigin.End);
                        inputStream.CopyTo(fs);
                        fs.Seek(fslength, SeekOrigin.Begin);

                    }
                }
            }
            SymbologyBlock.Divider = ReadHalfWord(fs);
            SymbologyBlock.BlockID = ReadHalfWord(fs);
            SymbologyBlock.BlockLength = ReadWord(fs);
            SymbologyBlock.NumOfLayers = ReadHalfWord(fs);

            for (int i = 1; i <= SymbologyBlock.NumOfLayers; i++)
            {
                SymbologyBlock.ParseLayers(fs, DescriptionBlock);
            }
        }

        /////////////////////////////////////////////
        /* Parse the Graphic Alphanumeric Block    */
        /* into an array and return it.            */
        /////////////////////////////////////////////
        public GraphicBlock ParseGAB()
        {
            GraphicBlock graphic_block = new GraphicBlock();
            graphic_block_offset = (DescriptionBlock.GraphicOffset * 2) + msg_header_block_offset;

            fs.Seek(graphic_block_offset, SeekOrigin.Begin);
            graphic_block.Divider = ReadHalfWord(fs, true);
            graphic_block.BlockID = ReadHalfWord(fs);
            graphic_block.BlockLength = ReadWord(fs);
            graphic_block.Pages = new Page[ReadHalfWord(fs)];

            for (int i = 0; i < graphic_block.Pages.Length; i++)
            {
                graphic_block.Pages[i] = ParsePage(fs);
            }

            return graphic_block;
        }

        public enum MessageCodes
        {
            BaseReflectivity
        }

    }

}
