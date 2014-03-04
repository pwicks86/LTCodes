using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace LTCodes
{
    /*
     * Test variables:
     * -Different degrees(?) -> no, proven optimal
     * -Different number of blocks?
     * -Different loss rates
     * -Different message sizes
     * 
     * Test parameters:
     * -Accuracy/Ability to complete
     * -Speed
     */

    class Program
    {
        static void Main(string[] args)
        {
            String fileName = String.Format("output {0:yyy-MM-dd HH-mm-ss}.csv", DateTime.Now);
            const String formatString = "{0}, {1}, {2}%, {10}%, {3}, {5}, {7}, {4}, {6}, {8}, {9}\n";
            String stats = String.Format(formatString,
                "Message size",
                "Block size",
                "Loss rate",
                "Total encode time",
                "Total decode time",
                "Packets encoded",
                "Packets decoded",
                "Amortized encode time",
                "Amortized decode time",
                "Number of errors",
                "Actual loss rate"
            );
            File.AppendAllText(fileName, stats);

            //Test different message sizes
            for(int messageSize = 16; messageSize < Int16.MaxValue; messageSize *= 2) {
                //Test different block sizes
                for (int numBlocks = 2; numBlocks < messageSize; numBlocks *= 2)
                {
                    //Test different loss rates
                    for (int lossRate = 0; lossRate < 9; ++lossRate)
                    {
                        //Metric variables
                        long start = 0;
                        long totalEncodeTime = 0;
                        long totalDecodeTime = 0;
                        int numPacketsEncoded = 0;
                        int numPacketsDecoded = 0;
                        int numWrongBytes = 0;  //Expected to remain 0

                        //General information
                        Random rand = new Random();

                        //Generate message
                        byte[] messageIn = new byte[messageSize];
                        rand.NextBytes(messageIn);

                        //Encode/decode
                        Encoder encoder = new Encoder(messageIn, messageSize / numBlocks, false);
                        Decoder decoder = new Decoder(encoder.GetNumberOfChunks());
                        while (!decoder.hasDecodedMessage())
                        {
                            //Encode a packet and measure the time it took
                            start = Stopwatch.GetTimestamp();
                            Packet p = encoder.Encode();
                            totalEncodeTime += Stopwatch.GetTimestamp() - start;
                            numPacketsEncoded++;
                            
                            //Drop the packet?
                            if (rand.Next(1,10) > lossRate)
                            {
                                //Decode a packet and measure the time it took
                                start = Stopwatch.GetTimestamp();
                                decoder.RecvPacket(p);
                                totalDecodeTime += Stopwatch.GetTimestamp() - start;
                                numPacketsDecoded++;
                            }
                        }
                        //Check the decoded message
                        byte[] messageOut = decoder.GetDecodedMessage();

                        //Initialize the number of incorrect bytes
                        int minLength = 0;
                        if(messageOut.Length < messageIn.Length) {
                            minLength = messageOut.Length;
                            numWrongBytes = messageIn.Length - messageOut.Length;
                        } else {
                            minLength = messageIn.Length;
                            numWrongBytes = messageOut.Length - messageIn.Length;
                        }

                        //Check the decoded message
                        for (int i = 0; i < minLength; ++i)
                        {
                            if (messageIn[i] != messageOut[i])
                            {
                                numWrongBytes++;
                            }
                        }

                        //Dump statistics
                        stats = String.Format(formatString,
                            messageSize,
                            messageSize / numBlocks, 
                            ((float)lossRate) * 10f,
                            totalEncodeTime,
                            totalDecodeTime,
                            numPacketsEncoded,
                            numPacketsDecoded,
                            (float)totalEncodeTime / (float)numPacketsEncoded,
                            (float)totalDecodeTime / (float)numPacketsDecoded,
                            numWrongBytes,
                            (float)(numPacketsEncoded - numPacketsDecoded) / (float)numPacketsEncoded
                        );
                        bool wroteData = false;
                        do
                        {
                            try
                            {
                                File.AppendAllText(fileName, stats);
                                wroteData = true;
                            }
                            catch (Exception e)
                            {
                                //Failed to write file
                            }

                        } while (!wroteData);
                        //statsOut.Write(System.Text.Encoding.ASCII.GetBytes (stats), 0, stats.Length);
                        //statsOut.Flush();
                        Console.Write(stats);
                    }
                }
            }

            //statsOut.Close();


/*
            string helloString = "Hello World! Foo Bar 1234567890ABCDEFGHIJKLMNOPQRSTUVWXYZ!?";
            string anotherString = "Hello World! Foo Bar 1234567890ABCDEFGHIJKLMNOPQRSTUVWXYZ!?";

            for (int i = 0; i < 1000; i++)
            {
                helloString += "a";
                anotherString += "a";
            }
            Console.WriteLine("Full Message:{0}", helloString);
            int chunkSize = 5;
            byte[] messageIn = System.Text.Encoding.ASCII.GetBytes (helloString);
            Encoder encoder = new Encoder(messageIn, chunkSize);
            Decoder decoder = new Decoder(encoder.GetNumberOfChunks());
            while (!decoder.hasDecodedMessage())
            {
                Packet p = encoder.Encode();
                // Drop the packet here?
                decoder.RecvPacket(p);
            }
            byte[] messageOut = decoder.GetDecodedMessage();
            string messageString = System.Text.Encoding.ASCII.GetString(messageOut);
            Console.WriteLine("Message was {0}", messageString);
            if (helloString.Equals( anotherString))
            {
                Console.WriteLine("They are equal!");
            }
            else
            {
                Console.WriteLine("They are not equal!");
            }
 */
            Console.ReadKey();
        }
    }
}
