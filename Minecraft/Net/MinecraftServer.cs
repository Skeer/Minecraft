using System.IO;
using Minecraft.Utilities;
using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Net;
using System.Threading;
using System.IO.Compression;
using NBTLibrary;

namespace Minecraft.Net
{
    class MinecraftServer : IDisposable
    {
        private static Logger Log = new Logger(typeof(MinecraftServer));
        private static MinecraftServer _Instance = new MinecraftServer();
        private int Port = 25565;
        private List<string> Administrators = new List<string>();
        private Dictionary<string, string> Configuration = new Dictionary<string, string>();
        private Socket Server = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        private AutoResetEvent ResetEvent = new AutoResetEvent(true);
        private FileSystemWatcher Watcher;
        public long RandomSeed { get; set; }

        private byte _Dimension = 0; //Maybe do this client wise?
        
        public byte Dimension
        {
            get { return _Dimension; }
            set { _Dimension = value; }
        }

        private string _Path = @"C:\world\";

        public string Path
        {
            get { return _Path; }
            set { _Path = value; }
        }


        public static MinecraftServer Instance
        {
            get { return _Instance; }
            set { _Instance = value; }
        }

        private MinecraftServer()
        { }

        public void Run()
        {
            ReloadConfiguration();
            ReloadAdministrators();

            SessionLock();

            Watcher = new FileSystemWatcher(_Path, "session.lock");
            Watcher.Changed += new FileSystemEventHandler(Watcher_Changed);

            //level.dat
            // TODO: Path.Join?
            using (NBTFile levelFile = NBTFile.Open(_Path + "level.dat"))
            {
                RandomSeed = (long)levelFile.FindPayload("RandomSeed");
            }

            //object seed = levelFile.FindPayload("RandomSeed", levelFile.File);

            //Socket
            try
            {
                IPEndPoint endPoint = new IPEndPoint(IPAddress.Any, Port);

                Server.Bind(endPoint);
                Server.Listen(10);
                Log.Info("Socket server bound and listing at {0}.", endPoint);

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
                                _Authentication = (MinecraftAuthentication)Enum.Parse(typeof(MinecraftAuthentication), splitted[1]);
                            }
                            else if (splitted[0].ToLower() == "version")
                            {
                                _Version = int.Parse(splitted[1]);
                            }
                            else if (splitted[0].ToLower() == "path")
                            {
                                _Path = splitted[1];
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
            //chunks
            //players
            //  position
            //  items
        }

        public void Dispose()
        {
            Logger.Writer.Dispose();
        }

        private void OnAccept(IAsyncResult result)
        {
            Socket client = Server.EndAccept(result);

            // start client handler
            Log.Info("Client connected from {0}.", client.RemoteEndPoint);

            new MinecraftClient(client);

            ResetEvent.Set();
        }

        private MinecraftAuthentication _Authentication = MinecraftAuthentication.Online;

        public MinecraftAuthentication Authentication
        {
            get { return _Authentication; }
            set { _Authentication = value; }
        }

        private int _Version = 8;

        public int Version
        {
            get { return _Version; }
            set { _Version = value; }
        }

        private uint _Entity = 0;

        public uint Entity
        {
            get { return _Entity; }
            set { _Entity = value; }
        }
    }
}
