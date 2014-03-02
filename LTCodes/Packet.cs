using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LTCodes
{
    class Packet
    {
        public int[] listSelectedIndexes {get; set;} // contains indexes which are xored in this packet,metadata#2
        public byte[] packetData {get; set;} //actual data

        //ToStream packs the packet data into a byte array
        //format = number of indexes + indexes[] + data bytes
        public byte[] Serialize()
        {
            List<byte> packetArrayBytes = new List<byte>();

            packetArrayBytes.Add((byte)(listSelectedIndexes.Length));

            foreach (int index in listSelectedIndexes)
                packetArrayBytes.Add((byte)index);

            foreach (byte dataByte in packetData)
                packetArrayBytes.Add(dataByte);

            return packetArrayBytes.ToArray();
        }
        
    }
}
;