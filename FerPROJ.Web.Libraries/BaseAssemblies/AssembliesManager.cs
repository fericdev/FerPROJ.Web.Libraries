using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace FerPROJ.Web.Libraries.BaseAssemblies {
    public static class AssembliesManager {
        public static List<Assembly> GetAssemblies(params string[] assemblies) {

            var baseDir = AppDomain.CurrentDomain.BaseDirectory;

            var allAssemblies = assemblies.ToList();

            allAssemblies.Add("FerPROJ.Libraries");

            foreach (var file in allAssemblies) {

                // Support both file names (e.g., "LMS.Repository.dll") and short names ("LMS.Repository")
                var fileName = file.EndsWith(".dll", StringComparison.OrdinalIgnoreCase) ? file : file + ".dll";

                var path = Path.Combine(baseDir, fileName);

                if (File.Exists(path)) {

                    var asmName = Path.GetFileNameWithoutExtension(fileName);

                    if (!AppDomain.CurrentDomain.GetAssemblies().Any(a => a.GetName().Name.Equals(asmName, StringComparison.OrdinalIgnoreCase))) {
                        Assembly.LoadFrom(path);
                    }
                }
                else {
                    Console.WriteLine($"⚠️ Assembly not found: {path}");
                }
            }

            // List of assemblies to search, including current and referenced assemblies
            return AppDomain.CurrentDomain.GetAssemblies()
                .Where(a => !a.IsDynamic)  // Ignore dynamic assemblies
                .ToList();
        }
        public static List<Type> GetDerivedTypes(this Type baseType, params string[] assemblies) {

            var assemblyList = GetAssemblies(assemblies);

            return assemblyList.SelectMany(a => a.GetTypes())
                .Where(t => !t.IsAbstract && IsSubclassOfRawGeneric(baseType, t))
                .ToList();
        }
        public static List<Type> GetTypesOf(this Type type, params string[] assemblies) {

            var assemblyList = GetAssemblies(assemblies);

            return assemblyList.SelectMany(a => {
                try {
                    return a.GetTypes();
                }
                catch (ReflectionTypeLoadException ex) {
                    return ex.Types.Where(t => t != null);
                }
            })
                .Where(t => !t.IsAbstract && !t.IsInterface)
                .Where(t =>
                    t.GetInterfaces().Any(i =>
                        i.IsGenericType &&
                        i.GetGenericTypeDefinition() == type
                    )
                )
                .ToList();
        }
        public static bool IsSubclassOfRawGeneric(Type generic, Type toCheck) {
            while (toCheck != null && toCheck != typeof(object)) {
                var cur = toCheck.IsGenericType ? toCheck.GetGenericTypeDefinition() : toCheck;
                if (generic == cur) {
                    return true;
                }
                toCheck = toCheck.BaseType;
            }
            return false;
        }
    }
}
