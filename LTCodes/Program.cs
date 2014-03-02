﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LTCodes
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Full Message: Hello World!");
            string helloString = "Hello World!";
            int chunkSize = 5;
            byte[] inputBytes = System.Text.Encoding.ASCII.GetBytes (helloString);
            Encoder encoder = new Encoder(inputBytes, chunkSize);
            Decoder decoder = new Decoder(encoder.GetNumberOfChunks());
            while (!decoder.hasDecodedMessage())
            {
                Packet p = encoder.Encode();
                // Drop the packet here?
                decoder.RecvPacket(p);
            }
            byte[] message = decoder.GetDecodedMessage();
            string messageString = System.Text.Encoding.ASCII.GetString(message);
            Console.WriteLine("Message was {0}", messageString);
            Console.ReadKey();
        }
    }
}