using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DGScope.Receivers.ReadsbJSON
{
    public class QueueStream : MemoryStream
    {
        long readPosition, writePosition;
        object lockobj = new object();
        public QueueStream() : base() { }
        public override int Read(byte[] buffer, int offset, int count)
        {
            int temp;
            lock (lockobj)
            {
                Position = readPosition;
                temp = base.Read(buffer, offset, count);
                readPosition = Position;
            }
            return temp;
        }
        public override void Write(byte[] buffer, int offset, int count)
        {
            lock (lockobj)
            {
                Position = writePosition;
                base.Write(buffer, offset, count);
                writePosition = Position;
            }
        }

        
    }
}
