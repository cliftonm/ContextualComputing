using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;

using Clifton.Core.Assertions;
using Clifton.Core.ExtensionMethods;
using Clifton.Core.Semantics;
using Clifton.Core.ModuleManagement;
using Clifton.Core.ServiceInterfaces;
using Clifton.Core.ServiceManagement;

namespace MeaningExplorer
{
    public class AppConfigDecryption : ServiceBase, IAppConfigDecryptionService
    {
        public string Password { get; set; }
        public string Salt { get; set; }

        public string Decrypt(string text)
        {
            return text.Decrypt(Password, Salt);
        }
    }

    static partial class Program
    {
        public static ServiceManager serviceManager;

        static ServiceManager Bootstrap()
        {
            serviceManager = new ServiceManager();
            serviceManager.RegisterSingleton<IServiceModuleManager, ServiceModuleManager>();
            serviceManager.RegisterSingleton<IAppConfigDecryptionService, AppConfigDecryption>(d =>
            {
                d.Password = "abc!@1234";
                d.Salt = "91827465";
            });

            try
            {
                IModuleManager moduleMgr = (IModuleManager)serviceManager.Get<IServiceModuleManager>();
                List<AssemblyFileName> modules = GetModuleList(XmlFileName.Create("modules.xml"));
                moduleMgr.RegisterModules(modules); //, OptionalPath.Create("dll"));
                List<Exception> exceptions = serviceManager.FinishSingletonInitialization();
                ShowAnyExceptions(exceptions);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Initialization Error: " + ex.Message);
            }

            return serviceManager;
        }

        /// <summary>
        /// Return the list of assembly names specified in the XML file so that
        /// we know what assemblies are considered modules as part of the application.
        /// </summary>
        static private List<AssemblyFileName> GetModuleList(XmlFileName filename)
        {
            Assert.That(File.Exists(filename.Value), "Module definition file " + filename.Value + " does not exist.");
            XDocument xdoc = XDocument.Load(filename.Value);

            return GetModuleList(xdoc);
        }

        /// <summary>
        /// Returns the list of modules specified in the XML document so we know what
        /// modules to instantiate.
        /// </summary>
        static private List<AssemblyFileName> GetModuleList(XDocument xdoc)
        {
            List<AssemblyFileName> assemblies = new List<AssemblyFileName>();
            (from module in xdoc.Element("Modules").Elements("Module")
             select module.Attribute("AssemblyName").Value).ForEach(s => assemblies.Add(AssemblyFileName.Create(s)));

            return assemblies;
        }
    }
}
