using FerPROJ.Web.Libraries.BaseAssemblies;
using FerPROJ.Web.Libraries.BaseDbHelper;
using System;
using System.Collections.Generic;
using System.Text;

namespace FerPROJ.Web.Libraries.BaseServices {
    public static class BaseServiceRegistration {
        public static void RegisterRepository(params string[] repositoryAssemblies) {
            // Base generic
            var baseGenericType = typeof(BaseRepository<,,>);

            if (repositoryAssemblies.Length == 0) {
                var appName = AppDomain.CurrentDomain.FriendlyName;
                repositoryAssemblies = [$"{appName}.Repository", $"{appName}.Repositories"];
            }

            // Get the derived types (that implement BaseDBEntityAsync)
            var derivedTypes = baseGenericType.GetDerivedTypes(repositoryAssemblies);

            foreach (var derivedType in derivedTypes) {
                BaseServiceManager.Set.AsScoped(derivedType);
            }

        }
        public static void RegisterType<TType>(params string[] typeAssemblies) {

            if (typeAssemblies.Length > 0) {
                var appName = AppDomain.CurrentDomain.FriendlyName;
                typeAssemblies = typeAssemblies.Select(a => $"{appName}.{a}").ToArray();
            }

            var derivedTypes = typeof(TType).GetDerivedTypes(typeAssemblies);

            foreach (var derivedType in derivedTypes) {
                BaseServiceManager.Set.AsTransient(derivedType);
            }
        }
    }
}
