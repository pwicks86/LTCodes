using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LTCodes
{
    class Encoder
    {
        private int chunkSize = 1;
        private int messageSize = 0;
        private int degree = 2;
        private Random randomNumberGenerator;
        private List<byte[]> blocks;
        private byte[] message;
        private bool verbose;

        public int GetChunkSize(){return chunkSize;}
        public int GetMessageSize() { return messageSize; }
        public int GetNumberOfChunks() { return blocks.Count(); }

        public Encoder(byte[] msg, int chunksize, bool verbose)
        {
            this.verbose = verbose;
            randomNumberGenerator = new Random();
            messageSize = msg.Length;
            message = msg;
            chunkSize = chunksize;

            blocks = CreateBlocks();

            //assume degree to be max(1, blocks.Count() / 2);
            if (blocks.Count() > 1)
                degree = blocks.Count() / 2;

            //unit testing
            if (verbose)
            {
                System.Console.WriteLine("Chunk Size: {0}, Number of Chunks: {1}, Degree: {2}", chunkSize, blocks.Count(), degree);
            
                foreach(byte[] block in blocks)
                {
                    string blockString = System.Text.Encoding.ASCII.GetString(block);
                    System.Console.WriteLine("Chunked Message: {0}", blockString);
                }
            }
            //unit testing
        }

        public Packet Encode()
        {
            //steps - randomly select a set of indices <= blocks.count
            //xor the data in the selected indices
            //packetize the above data
            //return it

            int[] selectedBlocks = SelectedBlocks();

            byte[] packetDataForTransport = CreatePacketData(selectedBlocks);

            return new Packet { listSelectedIndexes = selectedBlocks, packetData = packetDataForTransport };
        }

        private List<byte[]> CreateBlocks()
        {
            List<byte[]> blocks = new List<byte[]>();

            //divide given message into chunks of size chunkSize each
            int nBlocks = message.Length / chunkSize;
            if ((message.Length % chunkSize) > 0)
                ++nBlocks;

            int sizeToCopy = chunkSize;
            for (int i = 0; i < nBlocks; i++ )
            {
                byte[] block = new byte[chunkSize];
                if(i == (nBlocks -1))
                {
                    //last block
                    sizeToCopy = (message.Length) - (i * chunkSize);
                }

                Array.Copy(message, i * chunkSize, block, 0, sizeToCopy);
                blocks.Add(block);
            }
            
            return blocks;
        }

        private int[] SelectedBlocks()
        {
            HashSet<int> listOfSelectedBlocks = new HashSet<int>();

            // randomly select degree
            int numberOfNeighbors = randomNumberGenerator.Next(1, degree);
            for (int i = 0; i < numberOfNeighbors; i++ )
            {
                //always get and add an element
                while(true)
                {
                    //which blocks, select randomly
                    int randomlySelectedBlock = randomNumberGenerator.Next(blocks.Count());
                    if (listOfSelectedBlocks.Contains(randomlySelectedBlock) == false)
                    {
                        listOfSelectedBlocks.Add(randomlySelectedBlock);
                        break;
                    }
                }
            }
            return listOfSelectedBlocks.ToArray();
        }

        private byte[] CreatePacketData(int[] selectedBlocks)
        {
            byte[] packetData = new byte[chunkSize];

            for (int i = 0; i < chunkSize; i++ )
            {
                byte xoredByte = blocks[selectedBlocks[0]][i];
                for(int selectedBlockIndex = 1; selectedBlockIndex < selectedBlocks.Length; selectedBlockIndex++)
                {
                    byte byteFromSelectedBlock = (blocks[selectedBlocks[selectedBlockIndex]][i]);
                    xoredByte ^= byteFromSelectedBlock;
                }

                packetData[i] = xoredByte;
            }

            return packetData;
        }
    }
}
