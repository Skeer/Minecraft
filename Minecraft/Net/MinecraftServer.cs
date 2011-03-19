using System;
using System.Collections.Generic;
using System.IO;
using System.Timers;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using Minecraft.Command;
using Minecraft.Entities;
using Minecraft.Map;
using Minecraft.Packet;
using Minecraft.Utilities;
using NBTLibrary;

namespace Minecraft.Net
{
    public class MinecraftServer : MarshalByRefObject, IDisposable
    {
        private static Logger Log = new Logger(typeof(MinecraftServer));
        private static MinecraftServer _Instance = new MinecraftServer();
        private bool Disposed = false;
        private byte _Dimension = 0; //Maybe do this client wise?
        private int Port = 25565;
        private int _Version = 9;
        private string _Path = @"C:\world\";
        private uint _Entity = 0;
        private AutoResetEvent ResetEvent = new AutoResetEvent(true);
        private Dictionary<string, string> Configuration = new Dictionary<string, string>();
        private FileSystemWatcher Watcher;
        private List<string> Administrators = new List<string>();
        private MinecraftAuthentication _Authentication = MinecraftAuthentication.Online;
        private Socket Server = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        private System.Timers.Timer TimeTimer = new System.Timers.Timer(1000);
        private NBTFile LevelData;

        public static MinecraftServer Instance
        {
            get { return _Instance; }
            set { _Instance = value; }
        }
        public byte Dimension
        {
            get { return _Dimension; }
            set { _Dimension = value; }
        }
        /// <summary>
        /// In blocks (32 px).
        /// </summary>
        public int SpawnX
        {
            get
            {
                return (int)LevelData["Data"]["SpawnX"].Payload;
            }
            set
            {
                LevelData["Data"]["SpawnX"].Payload = value;
            }
        }
        /// <summary>
        /// In blocks (32 px).
        /// </summary>
        public short SpawnY
        {
            get
            {
                return (short)(int)LevelData["Data"]["SpawnY"].Payload;
            }
            set
            {
                LevelData["Data"]["SpawnY"].Payload = value;
            }
        }
        /// <summary>
        /// In blocks (32 px).
        /// </summary>
        public int SpawnZ
        {
            get
            {
                return (int)LevelData["Data"]["SpawnZ"].Payload;
            }
            set
            {
                LevelData["Data"]["SpawnZ"].Payload = value;
            }
        }
        public long Time
        {
            get
            {
                return (long)LevelData["Data"]["Time"].Payload;
            }
            set
            {
                LevelData["Data"]["Time"].Payload = value;
            }
        }
        public int Version
        {
            get { return _Version; }
            set { _Version = value; }
        }
        public long RandomSeed
        {
            get
            {
                return (long)LevelData["Data"]["RandomSeed"].Payload;
            }
            set
            {
                LevelData["Data"]["RandomSeed"].Payload = value;
            }
        }
        public string Path
        {
            get { return _Path; }
            set { _Path = value; }
        }
        public uint Entity
        {
            get { return _Entity; }
            set { _Entity = value; }
        }
        public ChunkManager ChunkManager { get; set; }
        public CommandManager CommandManager { get; set; }
        public Dictionary<string, Player> Players = new Dictionary<string, Player>();
        public MinecraftAuthentication Authentication
        {
            get { return _Authentication; }
            set { _Authentication = value; }
        }
        public MinecraftPacketRegistry PacketRegistry { get; set; }

        private MinecraftServer()
        { }

        public void Run()
        {
            ReloadConfiguration();
            ReloadAdministrators();

            SessionLock();

            Watcher = new FileSystemWatcher(_Path, "session.lock");
            Watcher.Changed += new FileSystemEventHandler(Watcher_Changed);

            LevelData = NBTFile.Open(_Path + "level.dat");

            PacketRegistry = new MinecraftPacketRegistry();
            CommandManager = new CommandManager();
            ChunkManager = new ChunkManager();

            TimeTimer.Elapsed += new ElapsedEventHandler(TimeTimer_Elapsed);
            TimeTimer.Start();

            //Socket
            try
            {
                IPEndPoint endPoint = new IPEndPoint(IPAddress.Any, Port);

                Server.Bind(endPoint);
                Server.Listen(10);
                Log.Info("Server bound and listing at {0}.", endPoint);

                while (ResetEvent.WaitOne())
                {
                    Server.BeginAccept(OnAccept, null);
                }
                // TODO: Research SendAsync and related functions
            }
            catch (Exception e)
            {
                Log.Error(e, "Unable to initialize socket server.");
            }
        }

        private void TimeTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            Time += 20;
        }

        private void SessionLock()
        {
            // vanilla does it with milliseconds
            // this is 100 nano seconds...
            using (StreamWriter writer = new StreamWriter(_Path + "session.lock"))
            {
                writer.Write(DateTime.Now.Ticks);
                writer.Flush();
            }
        }

        private void Watcher_Changed(object sender, FileSystemEventArgs e)
        {
            Shutdown();
        }

        public void ReloadConfiguration()
        {
            try
            {
                using (StreamReader reader = new StreamReader("server.config"))
                {
                    while (!reader.EndOfStream)
                    {
                        string[] splitted = reader.ReadLine().Trim().Split('=');
                        if (splitted.Length == 2)
                        {
                            //special options
                            if (splitted[0].ToLower() == "port")
                            {
                                Port = int.Parse(splitted[1]);
                            }
                            else if (splitted[0].ToLower() == "display")
                            {
                                Logger.Display = bool.Parse(splitted[1]);
                            }
                            else if (splitted[0].ToLower() == "auth" || splitted[0].ToLower() == "authentication")
                            {
                                _Authentication = (MinecraftAuthentication)Enum.Parse(typeof(MinecraftAuthentication), splitted[1], true);
                            }
                            else if (splitted[0].ToLower() == "version")
                            {
                                _Version = int.Parse(splitted[1]);
                            }
                            else if (splitted[0].ToLower() == "path")
                            {
                                _Path = splitted[1];
                            }
                            else if (splitted[0].ToLower() == "commanddirectory")
                            {
                                CommandManager.CommandDirectory = splitted[1];
                            }
                            else
                            {
                                //store data for future references....
                                Configuration.Add(splitted[0].ToLower(), splitted[1]);
                            }
                        }
                    }
                }
            }
            catch (FileNotFoundException)
            {
                Log.Warning("Unable to locate server.config.");
            }
            catch (Exception e)
            {
                Log.Error(e, "Unable to reload configuration.");
            }
        }

        public void ReloadAdministrators()
        {
            try
            {
                using (StreamReader reader = new StreamReader("administrators.txt"))
                {
                    while (!reader.EndOfStream)
                    {
                        string line = reader.ReadLine().Trim().ToLower();
                        if (line.Length > 0)
                        {
                            Administrators.Add(line);
                        }
                    }
                }
            }
            catch (FileNotFoundException)
            {
                Log.Warning("Unable to locate administrators.txt.");
            }
            catch (Exception e)
            {
                Log.Error(e, "Unable to reload administrators.");
            }
        }

        public void Shutdown()
        {
            Save();
            Dispose();
        }

        public void Save()
        {
            LevelData.Save();
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!Disposed)
            {
                if (disposing)
                {
                    if (Logger.Writer != null)
                    {
                        Logger.Writer.Dispose();
                    }

                    if (Server != null)
                    {
                        Server.Dispose();
                    }

                    if (ResetEvent != null)
                    {
                        ResetEvent.Dispose();
                    }

                    if (Watcher != null)
                    {
                        Watcher.Dispose();
                    }

                    if (CommandManager != null)
                    {
                        CommandManager.Dispose();
                    }
                }

                CommandManager = null;
                Watcher = null;
                ResetEvent = null;
                Server = null;
                Logger.Writer = null;
                Disposed = true;
            }
        }

        ~MinecraftServer()
        {
            Dispose(false);
        }

        private void OnAccept(IAsyncResult result)
        {
            Socket client = Server.EndAccept(result);

            // start client handler
            new MinecraftClient(client);

            Log.Info("Client connected from {0}.", client.RemoteEndPoint);

            ResetEvent.Set();
        }

        public MinecraftRank GetRank(string Username)
        {
            if (Administrators.Contains(Username.ToLower()))
            {
                return MinecraftRank.Admin;
            }
            return MinecraftRank.Player;
        }
    }
}
