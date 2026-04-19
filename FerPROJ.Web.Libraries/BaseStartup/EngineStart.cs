using FerPROJ.Web.Libraries.BaseDataHelper;
using FerPROJ.Web.Libraries.BaseDbHelper;
using FerPROJ.Web.Libraries.BaseModels;
using FerPROJ.Web.Libraries.BaseServices;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;

namespace FerPROJ.Web.Libraries.BaseStartup {
    public class EngineStart {
        public static void InitializeServices<TContext>(IServiceCollection services)
            where TContext : BaseDbContext {

            // Initialize from service
            BaseServiceManager.Set.Initialize(services);

            // Register DbContext using the generic type
            BaseServiceManager.Set.AsDbContext<TContext>(options =>
                options.UseMySql(
                    ConnectionString.ENTITY_CONNECTION_STRING,
                    ServerVersion.AutoDetect(ConnectionString.ENTITY_CONNECTION_STRING)
                ));

            // ✅ Map abstract → concrete
            services.AddScoped<BaseDbContext>(p =>
                p.GetRequiredService<TContext>());

            BaseServiceRegistration.RegisterRepository();
            BaseServiceRegistration.RegisterType<BaseModel>("Models");

            //
            BaseDbMigration.RunDatabaseMigrationAsync<TContext>().Wait();
        }
    }
}
