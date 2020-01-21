using ModMyFactory;
using ModMyFactory.BaseTypes;
using ModMyFactory.Export;
using ModMyFactory.Localization;
using ModMyFactory.ModSettings;
using ModMyFactory.WebApi;
using ModMyFactory.Win32;
using ModMyFactoryGUI.Helpers;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace ModMyFactoryGUI
{
    static class VersionStatistics
    {
        class AssemblyVersionDictionary : IReadOnlyDictionary<Assembly, string>
        {
            public string this[Assembly key]
            {
                get
                {
                    if (TryGetValue(key, out string value))
                        return value;
                    else
                        throw new KeyNotFoundException();
                }
            }

            public IEnumerable<Assembly> Keys { get; }

            public IEnumerable<string> Values => Keys.Select(a => a.ProductVersion());

            public int Count => Keys.Count();

            public AssemblyVersionDictionary(IEnumerable<Assembly> assemblies)
            {
                Keys = assemblies;
            }

            public AssemblyVersionDictionary(params Assembly[] assemblies)
                : this((IEnumerable<Assembly>)assemblies)
            { }

            public bool ContainsKey(Assembly key) => Keys.Contains(key);

            public bool TryGetValue(Assembly key, out string value)
            {
                if (key is null)
                    throw new ArgumentNullException(nameof(key));

                if (!ContainsKey(key))
                {
                    value = null;
                    return false;
                }

                value = key.ProductVersion();
                return true;
            }

            public IEnumerator<KeyValuePair<Assembly, string>> GetEnumerator()
            {
                foreach (var assembly in Keys)
                    yield return new KeyValuePair<Assembly, string>(assembly, assembly.ProductVersion());
            }

            IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
        }


        public static string AppVersion { get; }

        public static IReadOnlyDictionary<Assembly, string> LoadedAssemblyVersions { get; }

        static VersionStatistics()
        {
            AppVersion = Assembly.GetExecutingAssembly().ProductVersion();
            // Just using some arbitrary types to find the assemblies.
            LoadedAssemblyVersions = new AssemblyVersionDictionary(
                Assembly.GetAssembly(typeof(Manager)),
                Assembly.GetAssembly(typeof(AccurateVersion)),
                Assembly.GetAssembly(typeof(Package)),
                Assembly.GetAssembly(typeof(Locale)),
                Assembly.GetAssembly(typeof(ISetting)),
                Assembly.GetAssembly(typeof(Authentication)),
                Assembly.GetAssembly(typeof(Kernel32))
                );
        }
    }
}