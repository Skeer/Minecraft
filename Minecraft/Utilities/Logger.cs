﻿using System;
using System.IO;

namespace Minecraft.Utilities
{
    class Logger
    {
        private static bool _Display = false;

        public static bool Display
        {
            get { return _Display; }
            set { _Display = value; }
        }

        public static StreamWriter Writer = new StreamWriter("server.log");

        public string ClassName { get; set; }

        public Logger(Type className)
        {
            ClassName = className.ToString();
        }

        public void Info(string message, params object[] args)
        {
            if (_Display)
            {
                Console.Write("{0} [INFO] ", DateTime.Now);
                if (args.Length > 0)
                {
                    Console.WriteLine(message, args);
                }
                else
                {
                    Console.WriteLine(message);
                }
            }

            Writer.Write("{0} [INFO] ", DateTime.Now);
            if (args.Length > 0)
            {
                Writer.WriteLine(message, args);
            }
            else
            {
                Writer.WriteLine(message);
            }
            Writer.Flush();
        }

        public void Warning(string message, params object[] args)
        {
            if (_Display)
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.Write("{0} [WARNING] ", DateTime.Now);
                if (args.Length > 0)
                {
                    Console.WriteLine(message, args);
                }
                else
                {
                    Console.WriteLine(message);
                }
                Console.ResetColor();
            }

            Writer.Write("{0} [WARNING] ", DateTime.Now);
            if (args.Length > 0)
            {
                Writer.WriteLine(message, args);
            }
            else
            {
                Writer.WriteLine(message);
            }
            Writer.Flush();
        }

      
        public void Error(Exception e, string message, params object[] args)
        {
            if (Display)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.Error.Write("{0} [ERROR] ", DateTime.Now);
                if (args.Length > 0)
                {
                    Console.Error.WriteLine(message, args);
                }
                else
                {
                    Console.Error.WriteLine(message);
                }
                Console.ResetColor();
                Console.Error.WriteLine(e);
            }

            Writer.Write("{0} [ERROR] ", DateTime.Now);
            if (args.Length > 0)
            {
                Writer.WriteLine(message, args);
            }
            else
            {
                Writer.WriteLine(message);
            }
            Writer.WriteLine(e);
        }
    }
}
