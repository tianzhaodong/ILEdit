﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Media;
using ILEdit.Injection;
using ILEdit.Injection.Injectors;
using Mono.Cecil;
using ICSharpCode.ILSpy;
using System.Xml.Linq;

namespace ILEdit
{
    public static class GlobalContainer
    {
        #region Loading (.cctor)

        /// <summary>
        /// Static constructor which initializes the class
        /// </summary>
        static GlobalContainer()
        {
            //Sets the brushes
            //(performs this action on the UI thread)
            Action createBrushes = () => 
            {
                NormalNodesBrush = ICSharpCode.ILSpy.NodeBrushes.Normal;
                NewNodesBrush = Brushes.Red;
                ModifiedNodesBrush = new SolidColorBrush(Color.FromArgb(255, 230, 96, 6));
            };
            if (Application.Current.Dispatcher.CheckAccess())
                createBrushes();
            else
                Application.Current.Dispatcher.Invoke(createBrushes, null);

            //Adds the injectors
            Injectors = new List<IInjector>() { 
                
                new AssemblyReferenceInjector(),
                
                new ClassInjector(),
                new StructInjector(),
                new DelegateInjector(),
                new InterfaceInjector(),
                new EnumInjector(),

                new FieldInjector(),
                new MethodInjector(),
                new PropertyInjector(),
                new EventInjector()

            };
        }

        #endregion

        #region Settings

        /// <summary>
        /// ILEdit settings manager
        /// </summary>
        public class SettingsManager
        {
            ILSpySettings settings;
            
            #region Singleton implementation
            
            private SettingsManager()
            {
                settings = ILSpySettings.Load();
                Root = settings[XName.Get("ILEdit")];
                if (Root.Elements().Count() == 0)
                    RestoreDefault();
            }

            private static SettingsManager _instance;
            /// <summary>
            /// Returns the only instance of the settings manager
            /// </summary>
            public static SettingsManager Instance {
                get { return _instance ?? (_instance = new SettingsManager()); } 
            }

            #endregion

            /// <summary>
            /// Returns the root settings node &lt;ILEdit /&gt;
            /// </summary>
            public XElement Root { get; private set; }

            /// <summary>
            /// Returns the section with the given name
            /// </summary>
            /// <param name="name"></param>
            /// <returns></returns>
            public XElement this[string name] { 
                get 
                {
                    var xel = Root.Element(XName.Get(name));
                    if (xel == null)
                    {
                        xel = new XElement(XName.Get(name));
                        Root.Add(xel);
                    }
                    return xel;
                } 
            }

            /// <summary>
            /// Saves the settings
            /// </summary>
            public void Save() 
            {
                ILSpySettings.SaveSettings(Root);
            }

            /// <summary>
            /// Restores the settings to their default value
            /// </summary>
            public void RestoreDefault()
            {
                //Clears the root
                Root.RemoveAll();

                //Creates <Injection> node
                Root.Add(new XElement("Injection",
                    new XAttribute("MaxRecentMembersCount", 5),
                    new XElement("RecentMembers"),
                    new XElement("InjectExistingSettings",
                        new XAttribute("Preview", true),
                        new XAttribute("ImportAsNestedTypes", false)
                    )
                ));
            }

        }

        /// <summary>
        /// Returns the settings of the injection part
        /// </summary>
        public static XElement InjectionSettings {
            get { return SettingsManager.Instance["Injection"]; } 
        }

        #endregion

        #region Injectors

        /// <summary>
        /// Complete list of the available injectors
        /// </summary>
        public static List<IInjector> Injectors { get; set; }

        #endregion

        #region Brushes

        /// <summary>
        /// Returns the color used to identify the normal nodes
        /// </summary>
        public static Brush NormalNodesBrush { get; private set; }

        /// <summary>
        /// Returns the color used to identify the new nodes
        /// </summary>
        public static Brush NewNodesBrush { get; private set; }

        /// <summary>
        /// Returns the color used to identify the modified nodes
        /// </summary>
        public static Brush ModifiedNodesBrush { get; private set; }

        #endregion

        #region GetFreeRID

        private static Dictionary<ModuleDefinition, int> _ridCache = new Dictionary<ModuleDefinition, int>();

        /// <summary>
        /// Returns the first free RID for the given module
        /// </summary>
        /// <param name="module"></param>
        /// <returns></returns>
        public static int GetFreeRID(ModuleDefinition module)
        {
            int ret;
            if (_ridCache.TryGetValue(module, out ret))
            {
                return (_ridCache[module] = (ret - 1));
            }
            else
            {
                _ridCache.Add(module, ret = (0x00ffffff - 1));
                return ret;
            }
        }

        #endregion
    }
}
