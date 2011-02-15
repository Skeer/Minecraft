using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Microsoft.CSharp;
using Minecraft.Net;
using Minecraft.Utilities;

namespace Minecraft.Command
{
    public class CommandManager : IDisposable
    {
        private static Logger Log = new Logger(typeof(CommandManager));
        private bool Disposed = false;
        private string _CommandDirectory = "Commands/";
        private AppDomainSetup Setup = new AppDomainSetup();
        private CompilerParameters Parameters = new CompilerParameters();
        private CSharpCodeProvider Provider = new CSharpCodeProvider();
        private Dictionary<string, string> Commands = new Dictionary<string, string>(); // Document?

        public string CommandDirectory
        {
            get { return _CommandDirectory; }
            set { _CommandDirectory = value; }
        }

        public bool CommandExists(string name)
        {
            return Commands.ContainsKey(name);
        }

        public Dictionary<string, string>.KeyCollection GetCommands()
        {
            return Commands.Keys;
        }

        public CommandManager()
        {
            Parameters.GenerateExecutable = false;
            Parameters.GenerateInMemory = false;
            Parameters.ReferencedAssemblies.Add(Assembly.GetExecutingAssembly().Location);

            Setup.ApplicationBase = AppDomain.CurrentDomain.BaseDirectory;

            ReloadCommands();
        }

        public void ReloadCommands()
        {
            Commands.Clear();
            string[] files = Directory.GetFiles(CommandDirectory, "*.cs", SearchOption.AllDirectories);
            foreach (string path in files)
            {
                string name = Path.GetFileNameWithoutExtension(path);
                if (Compile(name))
                {
                    Commands.Add(name.ToLower(), name);
                }
            }
        }

        private bool Compile(string name)
        {
            Parameters.OutputAssembly = Path.Combine(CommandDirectory, name + ".dll");
            string data = File.ReadAllText(Path.Combine(CommandDirectory, name + ".cs"));
            CompilerResults results = Provider.CompileAssemblyFromSource(Parameters, data);

            if (results.Errors.HasErrors)
            {
                foreach (CompilerError error in results.Errors)
                {
                    Log.Error(new Exception(error.ToString()), "Error encountered while compiling {0}.", name + ".cs");
                }
                return false;
            }

            if (results.Errors.HasWarnings)
            {
                foreach (CompilerError error in results.Errors)
                {
                    Log.Warning("Warning encountered while compiling {0}.\n{1}", name + ".cs", error);
                }
            }
            return true;
        }

        public bool RunCommand(MinecraftClient client, string name, string[] args)
        {
            try
            {
                Run(client, name, args);
                return true;
            }
            catch
            {
                if (Compile(name))
                {
                    try
                    {
                        Run(client, name, args);
                        return true;
                    }
                    catch (Exception e)
                    {
                        Log.Error(e, "Error running script {0}.", name);
                        return false;
                    }
                }
                return false;
            }
        }

        private void Run(MinecraftClient client, string name, string[] args)
        {
            AppDomain domain = AppDomain.CreateDomain("domain", null, Setup);
            try
            {
                ((ICommand)domain.CreateInstanceFromAndUnwrap(Path.Combine(CommandDirectory, name + ".dll"), "Minecraft.Commands." + Commands[name])).Run(MinecraftServer.Instance, client, args);
            }
            finally
            {
                AppDomain.Unload(domain);
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        ~CommandManager()
        {
            Dispose(false);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!Disposed)
            {
                if (disposing)
                {
                    if (Provider != null)
                    {
                        Provider.Dispose();
                    }
                }

                Provider = null;
                Disposed = true;
            }
        }
    }
}
