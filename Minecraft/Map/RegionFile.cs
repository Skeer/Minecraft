using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading;
using Minecraft.Net;
using Minecraft.Utilities;

namespace Minecraft.Map
{
    public class RegionFile : IDisposable, IEquatable<RegionFile>
    {
        public const int XCoords = 32;
        public const int ZCoords = 32;
        public const int TotalChunks = XCoords * ZCoords;
        public readonly Point<int, int, int> Location;
        public readonly int MinX, MaxX;
        public readonly int MinZ, MaxZ;
        readonly FileStream Stream;
        readonly object LockObj = new object();
        int Disposed;
        bool AnyChanges;

        public readonly long[] ChunkLocations = new long[TotalChunks];
        public readonly int[] ChunkLengths = new int[TotalChunks];
        public readonly long[] ChunkTimeStamps = new long[TotalChunks];

        public RegionFile(int x, int z, string fileName)
        {
            Location = new Point<int, int, int>() { X = x, Z = z };
            MinX = x * XCoords;
            MinZ = z * ZCoords;
            MaxX = x * XCoords + XCoords - 1;
            MaxZ = z * ZCoords + ZCoords - 1;
            Stream = new FileStream(fileName, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.Read);
            ReadHeader();
            _DeferWriteHeader.DeferEnded = DeferWriteHeaderEnded;
        }
        public RegionFile(int x, int z)
            : this(x, z, Path.Combine(MinecraftServer.Instance.Path, "region", "r." + x.ToString() + "." + z.ToString() + ".mcr"))
        {
        }

        void ReadHeader()
        {
            byte[] buf = new byte[4 * TotalChunks];
            lock (LockObj)
            {
                if (Stream.Read(buf, 0, buf.Length) != buf.Length)
                    throw new FormatException();
                for (int x = 0; x < TotalChunks; ++x)
                {
                    ChunkLocations[x] = 4096 * (long)(((int)buf[x * 4 + 0] << 16) + ((int)buf[x * 4 + 1] << 8) + ((int)buf[x * 4 + 2]));
                    ChunkLengths[x] = 4096 * (int)buf[x * 4 + 3];
                }
                if (Stream.Read(buf, 0, buf.Length) != buf.Length)
                    throw new FormatException();
                for (int x = 0; x < TotalChunks; ++x)
                {
                    ChunkTimeStamps[x] = (long)(((int)buf[x * 4 + 0] << 24) + ((int)buf[x * 4 + 1] << 16) + ((int)buf[x * 4 + 2] << 8) + ((int)buf[x * 4 + 3]));
                }
            }
        }
        void WriteHeader()
        {
            if (_DeferWriteHeader.Deferred)
                return;
            byte[] buf = new byte[8 * TotalChunks];
            lock (LockObj)
            {
                for (int x = 0; x < TotalChunks; ++x)
                {
                    int loc = (int)(ChunkLocations[x] / 4096);
                    buf[x * 4 + 0] = (byte)(loc >> 16);
                    buf[x * 4 + 1] = (byte)(loc >> 8);
                    buf[x * 4 + 2] = (byte)loc;
                    buf[x * 4 + 3] = (byte)(ChunkLengths[x] / 4096);
                    buf[x * 4 + TotalChunks * 4 + 0] = (byte)(ChunkTimeStamps[x] >> 24);
                    buf[x * 4 + TotalChunks * 4 + 1] = (byte)(ChunkTimeStamps[x] >> 16);
                    buf[x * 4 + TotalChunks * 4 + 2] = (byte)(ChunkTimeStamps[x] >> 8);
                    buf[x * 4 + TotalChunks * 4 + 3] = (byte)ChunkTimeStamps[x];
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

        public bool ChunkExists(int cx, int cz)
        {
            ValidateChunkCoords(cx, cz);
            return ChunkLocations[(cx % XCoords) + (cz % ZCoords) * XCoords] >= TotalChunks * 8;
        }
        public byte[] GetChunkData(int cx, int cz)
        {
            ValidateChunkCoords(cx, cz);
            lock (LockObj)
            {
                if (Disposed != 0)
                    throw new ObjectDisposedException("RegionFile");
                long pos = ChunkLocations[(cx - MinX) + (cz - MinZ) * XCoords];
                if (pos < TotalChunks * 8)
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
        public void SetChunkData(int cx, int cz, byte[] data, long timeStamp)
        {
            ValidateChunkCoords(cx, cz);
            int c1 = cx % XCoords;
            int c2 = cz % ZCoords;
            int cindex = c1 + c2 * XCoords;
            lock (LockObj)
            {
                if (Disposed != 0)
                    throw new ObjectDisposedException("RegionFile");
                AnyChanges = true;
                ChunkTimeStamps[cindex] = timeStamp;
                if (data == null || data.Length == 0)
                {
                    ChunkLocations[cindex] = 0;
                    ChunkLengths[cindex] = 0;
                    return;
                }
                byte[] buf;
                using (MemoryStream compstream = new MemoryStream(data.Length))
                {
                    using (DeflateStream ds = new DeflateStream(new MemoryStream(data), CompressionMode.Compress, false))
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
                long chunkdatalength = Stream.Length - 8 * TotalChunks;
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

        void ValidateChunkCoords(int cx, int cz)
        {
            if (cx < MinX || cx > MaxX ||
                cz < MinZ || cz > MaxZ)
                throw new ArgumentException("The chunk coordinates are not in this region");
        }

        public bool Equals(RegionFile other)
        {
            return other != null && other.Location.X == Location.X && other.Location.Z == Location.Z;
        }
        public override int GetHashCode()
        {
            return Location.X * XCoords + Location.Z;
        }
    }
}
