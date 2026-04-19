using FerPROJ.Web.Libraries.BaseDataHelper;
using FerPROJ.Web.Libraries.BaseDbHelper;
using FerPROJ.Web.Libraries.BaseServices;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace FerPROJ.Web.Libraries.BaseStartup {
    public class EngineStart {
        public static void InitializeServices<TContext>()
            where TContext : BaseDbContext {
            // Register base
            BaseServiceManager.Set.AsDbContext<BaseDbContext>(options =>
                options.UseMySql(
                    ConnectionString.ENTITY_CONNECTION_STRING,
                    ServerVersion.AutoDetect(ConnectionString.ENTITY_CONNECTION_STRING)
                ));

            BaseServiceManager.Set.AsDbContextFactory<BaseDbContext>(options =>
                options.UseMySql(
                    ConnectionString.ENTITY_CONNECTION_STRING,
                    ServerVersion.AutoDetect(ConnectionString.ENTITY_CONNECTION_STRING)
                ));

            // Register DbContext using the generic type
            BaseServiceManager.Set.AsDbContext<TContext>(options =>
                options.UseMySql(
                    ConnectionString.ENTITY_CONNECTION_STRING,
                    ServerVersion.AutoDetect(ConnectionString.ENTITY_CONNECTION_STRING)
                ));

            BaseServiceManager.Set.AsDbContextFactory<TContext>(options =>
                options.UseMySql(
                    ConnectionString.ENTITY_CONNECTION_STRING,
                    ServerVersion.AutoDetect(ConnectionString.ENTITY_CONNECTION_STRING)
                ));
        }
    }
}
