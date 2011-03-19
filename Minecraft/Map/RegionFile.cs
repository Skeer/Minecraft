using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading;
using Minecraft.Net;

namespace Minecraft.Map
{
    public class RegionFile : IDisposable, IEquatable<RegionFile>
    {
        public const int Coord1sPerRegion = 32;
        public const int Coord2sPerRegion = 32;
        public const int ChunksPerRegion = Coord1sPerRegion * Coord2sPerRegion;
        public readonly int RegionCoord1, RegionCoord2;
        public readonly int MinChunkCoord1, MaxChunkCoord1;
        public readonly int MinChunkCoord2, MaxChunkCoord2;
        readonly FileStream Stream;
        readonly object LockObj = new object();
        int Disposed;
        bool AnyChanges;

        public readonly long[] ChunkLocations = new long[ChunksPerRegion];
        public readonly int[] ChunkLengths = new int[ChunksPerRegion];
        public readonly long[] ChunkTimeStamps = new long[ChunksPerRegion];

        public RegionFile(int RegionCoord1, int RegionCoord2, string FileName)
        {
            this.RegionCoord1 = RegionCoord1;
            this.RegionCoord2 = RegionCoord2;
            MinChunkCoord1 = RegionCoord1 * Coord1sPerRegion;
            MinChunkCoord2 = RegionCoord2 * Coord2sPerRegion;
            MaxChunkCoord1 = RegionCoord1 * Coord1sPerRegion + Coord1sPerRegion - 1;
            MaxChunkCoord2 = RegionCoord2 * Coord2sPerRegion + Coord2sPerRegion - 1;
            Stream = new FileStream(FileName, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.Read);
            ReadHeader();
            _DeferWriteHeader.DeferEnded = DeferWriteHeaderEnded;
        }
        public RegionFile(int RegionCoord1, int RegionCoord2)
            : this(RegionCoord1, RegionCoord2, Path.Combine(MinecraftServer.Instance.Path, "region", "r." + RegionCoord1.ToString() + "." + RegionCoord2.ToString() + ".mcr"))
        {
        }

        public static void ChunkCoordToRegionCoord(int ChunkCoord1, int ChunkCoord2, out int RegionCoord1, out int RegionCoord2)
        {
            RegionCoord1 = (int)Math.Floor((double)ChunkCoord1 / Coord1sPerRegion);
            RegionCoord2 = (int)Math.Floor((double)ChunkCoord2 / Coord2sPerRegion);
        }

        void ReadHeader()
        {
            byte[] buf = new byte[4 * ChunksPerRegion];
            lock (LockObj)
            {
                if (Stream.Read(buf, 0, buf.Length) != buf.Length)
                    throw new FormatException();
                for (int x = 0; x < ChunksPerRegion; ++x)
                {
                    ChunkLocations[x] = 4096 * (long)(((int)buf[x * 4 + 0] << 16) + ((int)buf[x * 4 + 1] << 8) + ((int)buf[x * 4 + 2]));
                    ChunkLengths[x] = 4096 * (int)buf[x * 4 + 3];
                }
                if (Stream.Read(buf, 0, buf.Length) != buf.Length)
                    throw new FormatException();
                for (int x = 0; x < ChunksPerRegion; ++x)
                {
                    ChunkTimeStamps[x] = (long)(((int)buf[x * 4 + 0] << 24) + ((int)buf[x * 4 + 1] << 16) + ((int)buf[x * 4 + 2] << 8) + ((int)buf[x * 4 + 3]));
                }
            }
        }
        void WriteHeader()
        {
            if (_DeferWriteHeader.Deferred)
                return;
            byte[] buf = new byte[8 * ChunksPerRegion];
            lock (LockObj)
            {
                for (int x = 0; x < ChunksPerRegion; ++x)
                {
                    int loc = (int)(ChunkLocations[x] / 4096);
                    buf[x * 4 + 0] = (byte)(loc >> 16);
                    buf[x * 4 + 1] = (byte)(loc >> 8);
                    buf[x * 4 + 2] = (byte)loc;
                    buf[x * 4 + 3] = (byte)(ChunkLengths[x] / 4096);
                    buf[x * 4 + ChunksPerRegion * 4 + 0] = (byte)(ChunkTimeStamps[x] >> 24);
                    buf[x * 4 + ChunksPerRegion * 4 + 1] = (byte)(ChunkTimeStamps[x] >> 16);
                    buf[x * 4 + ChunksPerRegion * 4 + 2] = (byte)(ChunkTimeStamps[x] >> 8);
                    buf[x * 4 + ChunksPerRegion * 4 + 3] = (byte)ChunkTimeStamps[x];
                }
                Stream.Position = 0;
                Stream.Write(buf, 0, buf.Length);
            }
        }
        class DeferClass : IDisposable
        {
            int Count;
            public bool Deferred { get { return Count != 0; } }
            public Action DeferEnded;
            public void Dispose()
            {
                if (Interlocked.Decrement(ref Count) != 0)
                    return;
                Action a = DeferEnded;
                if (a != null)
                    a();
            }
            public IDisposable Lock()
            {
                Interlocked.Increment(ref Count);
                return this;
            }
        }
        readonly DeferClass _DeferWriteHeader = new DeferClass();
        public IDisposable DeferWriteHeader() { return _DeferWriteHeader.Lock(); }
        void DeferWriteHeaderEnded()
        {
            lock (LockObj)
                if (AnyChanges && Disposed == 0)
                {
                    WriteHeader();
                    AnyChanges = false;
                }
        }

        public bool ChunkExists(int ChunkCoord1, int ChunkCoord2)
        {
            ValidateChunkCoords(ChunkCoord1, ChunkCoord2);
            return ChunkLocations[(ChunkCoord1 % Coord1sPerRegion) + (ChunkCoord2 % Coord2sPerRegion) * Coord1sPerRegion] >= ChunksPerRegion * 8;
        }
        public byte[] GetChunkData(int ChunkCoord1, int ChunkCoord2)
        {
            ValidateChunkCoords(ChunkCoord1, ChunkCoord2);
            lock (LockObj)
            {
                if (Disposed != 0)
                    throw new ObjectDisposedException("RegionFile");
                long pos = ChunkLocations[(ChunkCoord1 / Coord1sPerRegion) + (ChunkCoord2 / Coord2sPerRegion) * Coord1sPerRegion];
                if (pos < ChunksPerRegion * 8)
                    return null;
                byte[] buf = new byte[4];
                Stream.Position = pos;
                if (Stream.Read(buf, 0, buf.Length) != buf.Length)
                    throw new FormatException();
                int len = (((int)buf[0] << 24) + ((int)buf[1] << 16) + ((int)buf[2] << 8) + ((int)buf[3]));
                if (len <= 1)
                    return null;
                if (Stream.ReadByte() != 2)         // 2 = zlib
                    throw new FormatException();        // anything except zlib is not supported
                buf = new byte[len - 7];
                Stream.Position += 2;
                if (Stream.Read(buf, 0, buf.Length) != buf.Length)
                    throw new FormatException();
                using (DeflateStream ds = new DeflateStream(new MemoryStream(buf), CompressionMode.Decompress, false))
                using (MemoryStream ms = new MemoryStream(buf.Length * 2))
                {
                    ds.CopyTo(ms);
                    return ms.ToArray();
                }
            }
        }
        public void SetChunkData(int ChunkCoord1, int ChunkCoord2, byte[] ChunkData, long CurrentTimeStamp)
        {
            ValidateChunkCoords(ChunkCoord1, ChunkCoord2);
            int c1 = ChunkCoord1 % Coord1sPerRegion;
            int c2 = ChunkCoord2 % Coord2sPerRegion;
            int cindex = c1 + c2 * Coord1sPerRegion;
            lock (LockObj)
            {
                if (Disposed != 0)
                    throw new ObjectDisposedException("RegionFile");
                AnyChanges = true;
                ChunkTimeStamps[cindex] = CurrentTimeStamp;
                if (ChunkData == null || ChunkData.Length == 0)
                {
                    ChunkLocations[cindex] = 0;
                    ChunkLengths[cindex] = 0;
                    return;
                }
                byte[] buf;
                using (MemoryStream compstream = new MemoryStream(ChunkData.Length))
                {
                    using (DeflateStream ds = new DeflateStream(new MemoryStream(ChunkData), CompressionMode.Compress, false))
                        ds.CopyTo(compstream);
                    buf = compstream.ToArray();
                }

                // calculate adler checksum - if you are certain that nobody's going to be reading your files, you can skip this - its for zlib
                uint s1 = 1, s2 = 0;
                int len = buf.Length;
                int at = 0;
                while (len > 0)
                {
                    int n = 3800 > len ? len : 3800;
                    len -= n;
                    while (--n > 0)
                    {
                        s1 += (uint)buf[at++];
                        s2 += s1;
                    }
                    s1 %= 65521;
                    s2 %= 65521;
                }
                uint chksum = (s2 << 16) | s1;

                long newloc = 0;
                if (ChunkLocations[cindex] != 0)
                    if (ChunkLengths[cindex] <= buf.Length + 6 + 5)       // 6 bytes for zlib header/footer, 5 bytes for chunk data format
                        newloc = ChunkLocations[cindex];
                if (newloc == 0)
                    newloc = (Stream.Length + 4095) & (0x7FFFFFFFFFFFFFFF & 4095);   // round up to nearest 4k "sector"
                byte[] tmp = new byte[7];
                tmp[0] = (byte)((buf.Length + 7) >> 24);
                tmp[1] = (byte)((buf.Length + 7) >> 16);
                tmp[2] = (byte)((buf.Length + 7) >> 8);
                tmp[3] = (byte)(buf.Length + 7);
                tmp[4] = 2;                         // zlib
                tmp[5] = 0x87;                      // deflate, 32k window      zlib header[0]
                tmp[6] = 5;                         // 0x8705 mod 31 = 0        zlib header[1]
                Stream.Position = newloc;
                Stream.Write(tmp, 0, tmp.Length);
                Stream.Write(buf, 0, buf.Length);
                tmp[0] = (byte)(chksum >> 24);
                tmp[1] = (byte)(chksum >> 16);
                tmp[2] = (byte)(chksum >> 8);
                tmp[3] = (byte)chksum;
                Stream.Write(buf, 0, 4);
                ChunkLocations[cindex] = newloc;
                ChunkLengths[cindex] = (buf.Length + 6 + 5 + 4095) & (0x7FFFFFFF & 4095);       // 6 = zlib data, 5 = chunk data, ... round up to nearest 4k sector

                WriteHeader();
            }
        }
        /// <summary>
        /// 0 = no space wasted...  1 = 100% space wasted
        /// </summary>
        /// <returns>0 = no space wasted...  1 = 100% space wasted</returns>
        public double CalculateWastedSpace()
        {
            lock (LockObj)
            {
                long chunkdatalength = Stream.Length - 8 * ChunksPerRegion;
                if (chunkdatalength == 0)
                    return 0;
                long chunkdataused = ChunkLengths.Sum(x => (long)x);
                double d = (double)(chunkdatalength - chunkdataused) / (double)chunkdatalength;
                if (d < 0)
                    return 0;
                if (d > 1)
                    return 1;
                return d;
            }
        }

        public void Compact()
        {
            throw new NotImplementedException();        // Defrag the file here (all depends on if wasted space becomes a problem)
            // or implement a free block manager... probably this is the better option
            // how often is a chunk written?  and how often is it overwritten?
        }
        public void Dispose()
        {
            if (Interlocked.Exchange(ref Disposed, 1) == 0)
            {
                lock (LockObj)
                {
                    Stream.Dispose();
                }
            }
        }
        void ValidateChunkCoords(int ChunkCoord1, int ChunkCoord2)
        {
            if (ChunkCoord1 < MinChunkCoord1 || ChunkCoord1 > MaxChunkCoord1 ||
                ChunkCoord2 < MinChunkCoord2 || ChunkCoord2 > MaxChunkCoord2)
                throw new ArgumentException("The chunk coordinates are not in this region");
        }

        public bool Equals(RegionFile other)
        {
            return other != null && other.RegionCoord1 == RegionCoord1 && other.RegionCoord2 == RegionCoord2;
        }
        public override int GetHashCode()
        {
            return RegionCoord1 * Coord1sPerRegion + RegionCoord2;
        }
    }
}
