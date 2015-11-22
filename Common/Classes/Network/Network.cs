using System;
using System.Collections.Concurrent;

namespace Common.Classes.Network
{
    public struct Packet
    {
        public DateTime Timestamp;
        public long UID;
        public long Size;
        public byte[] Data;

        public Packet(DateTime Timestamp, long UID, long Size, byte[] Data)
        {
            this.Timestamp = Timestamp;
            this.UID = UID;
            this.Size = Size;
            this.Data = Data;
        }
    }

    public static class Network
    {
        // - Variables - //
        public static byte[] PACKET_HEARTBEAT = new byte[1]{ 0x00 };

        // - Concurrent Dictionaries - //
        public static ConcurrentDictionary<long, Packet> ClientPacketDictionary = new ConcurrentDictionary<long, Packet>();
        public static ConcurrentDictionary<long, ConcurrentQueue<Packet>> ServerPacketDictionary = new ConcurrentDictionary<long, ConcurrentQueue<Packet>>();

        public static long AssignUIC()
        {
            byte[] RandomArray = new byte[255];
            long CurrentValue = 0;

            var RNG = System.Security.Cryptography.RandomNumberGenerator.Create();

            RNG.GetBytes(RandomArray);

            for (int Index = 0; Index < RandomArray.Length / 2; Index++)
            {
                CurrentValue = CurrentValue ^ ((RandomArray[Index] ^ RandomArray[Index * 2]) << (RandomArray[Index] | RandomArray[Index * 2]));
            }

            RandomArray = null;
            RNG.Dispose();

            return CurrentValue;
        }
    }
}
