using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LTCodes
{
    class Decoder
    {
        // Total bytes in message
        private int numBytes;
        // number of chunks in whole message
        private int numChunks;
        // Size of the chunks in bytes
        private int chunkSize = -1;
        // Holds decoded blocks
        private byte[] messageArray;
        // says whether a block has been decoded
        private bool[] decodedBlocks;

        public Decoder(int numChunks)
        {
            this.numChunks = numChunks;
        }

        internal bool hasDecodedMessage()
        {
            // Return true if all blocks are true
            return decodedBlocks != null && decodedBlocks.All(x => x);
        }

        // Holds any undecoded packets
        private List<Packet> holdList = new List<Packet>();
        
        internal void RecvPacket(Packet p)
        {

            int packetDegree = p.listSelectedIndexes.Length;
            byte[] packetData = p.packetData;
            String packetAsString = System.Text.Encoding.ASCII.GetString(packetData);
            Console.WriteLine("Got Packet: Degree: {0} StrData: {1}", packetDegree, packetAsString);
            // Figure out chunkSize from the packet data
            if (chunkSize == -1)
            {
                chunkSize = packetData.Length;
                // Default for boolean is false (i.e. we haven't decoed any blocks yet)
                this.decodedBlocks = new bool[numChunks];
                this.messageArray = new byte[chunkSize * numChunks]; 
            }
            dealWithPacket(p);
            
        }

        private void dealWithPacket(Packet p)
        {
            HashSet<int> indexesToRemove = new HashSet<int>();
            // For each block that has been xor'd into the packet
            List<int> sourceBlockIndexList = new List<int>(p.listSelectedIndexes);
            foreach (int sourceBlockIndex in sourceBlockIndexList)
            {
                // Undo the Xor if we can
                if (decodedBlocks[sourceBlockIndex])
                {
                    // Then xor the decoded block with the data
                    p.packetData = doXOR(p.packetData, getDecodedBlockByIndex(sourceBlockIndex));
                    // and schedule this index for removal
                    indexesToRemove.Add(sourceBlockIndex);
                }

            }
            // remove xor'd blocks from the metadata
            sourceBlockIndexList.RemoveAll(x => indexesToRemove.Contains(x));
            // If there is more than one source block in the data for this packet still, then we can't decode it
            // so we should just hold onto it for now
            if (sourceBlockIndexList.Count >= 2)
            {
                // Set the correct value for this and add the packet to the holdList
                p.listSelectedIndexes = sourceBlockIndexList.ToArray();
                // Add it to the hold list if it isn't there already
                if (!holdList.Contains(p))
                {
                    holdList.Add(p);
                }
            }
            // then we decoded a block!
            else if (sourceBlockIndexList.Count == 1)
            {
                // get the index of the decoded block
                int decodedBlockIndex = sourceBlockIndexList.First();
                // We decoded another block, copy it to the output
                Array.Copy(p.packetData, 0, this.messageArray, decodedBlockIndex * chunkSize, p.packetData.Length);
                // Mark the block as decoded
                this.decodedBlocks[decodedBlockIndex] = true;
                // Check if we can decode another packet because we decode this one
                Packet possiblePacketToDecode;
                try
                {
                     possiblePacketToDecode = holdList.First(thePacket => thePacket.listSelectedIndexes.Contains(decodedBlockIndex));
                }
                catch (InvalidOperationException e)
                {
                    possiblePacketToDecode = null;
                }
                if (possiblePacketToDecode != null)
                {
                    dealWithPacket(possiblePacketToDecode);
                }
            }
        }


        // REturn a byte array representing a decoded chunk
        private byte[] getDecodedBlockByIndex(int sourceBlockIndex)
        {
            byte[] decodedChunk = new byte[chunkSize];
            Array.Copy(this.messageArray, sourceBlockIndex * chunkSize, decodedChunk, 0, chunkSize);
            return decodedChunk;
        }

        internal byte[] GetDecodedMessage()
        {
            return messageArray;
        }

        // xor two byte arrays together
        public byte[] doXOR(byte[] a, byte[] b)
        {
            if (a.Length != b.Length) throw new Exception("Oh no!");
            byte[] xorArray = new byte[a.Length];
            for(int i = 0; i < a.Length; i++)
            {
                xorArray[i] = (byte) (a[i] ^ b[i]);
            }
            return xorArray;
        }
    }
}
