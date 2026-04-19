using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;

namespace FerPROJ.Web.Libraries.BaseServices {
    public static class BaseServiceManager {
        private static IServiceCollection _services = new ServiceCollection();
        private static IServiceProvider Services { get; set; }
        // ---------------------------
        // Retrieval API
        // ---------------------------
        public static class Get<T> where T : class {
            public static T Service() {
                return Services.GetRequiredService<T>();
            }
            public static T Service(Type type) {
                return (T)Services.GetRequiredService(type);
            }
        }

        // ---------------------------
        // Registration API
        // ---------------------------
        public static class Set {
            public static IServiceCollection AsDbContext<T>(Action<DbContextOptionsBuilder> option = null) where T : DbContext {
                var regService = _services.AddDbContext<T>(option);
                Build();
                return regService;
            }
            public static IServiceCollection AsDbContextFactory<T>(Action<DbContextOptionsBuilder> option = null) where T : DbContext {
                var regService = _services.AddDbContextFactory<T>(option);
                Build();
                return regService;
            }
            public static IServiceCollection AsTransient<T>() where T : class {
                var regService = _services.AddTransient<T>();
                Build();
                return regService;
            }
            public static IServiceCollection AsTransient(Type type) {
                var regService = _services.AddTransient(type);
                Build();
                return regService;
            }

            public static IServiceCollection AsScoped<T>() where T : class {
                var regService = _services.AddScoped<T>();
                Build();
                return regService;
            }
            public static IServiceCollection AsScoped(Type type) {
                var regService = _services.AddScoped(type);
                Build();
                return regService;
            }
            public static IServiceCollection AsScoped<T>(Func<IServiceProvider, T> factory) where T : class {
                var regService = _services.AddScoped(factory);
                Build();
                return regService;
            }

            public static IServiceCollection AsSingleton<T>() where T : class {
                var regService = _services.AddSingleton<T>();
                Build();
                return regService;
            }
            public static IServiceCollection AsSingleton(Type type) {
                var regService = _services.AddSingleton(type);
                Build();
                return regService;
            }

            public static IServiceCollection AsTransient<TService, TInterface>()
                where TService : class
                where TInterface : class, TService {
                var regService = _services.AddTransient<TService, TInterface>();
                Build();
                return regService;
            }
        }

        // ---------------------------
        // Build
        // ---------------------------
        private static void Build() {
            Services = _services.BuildServiceProvider();
        }
    }
}
