using FerPROJ.Web.Libraries.BaseAssemblies;
using FerPROJ.Web.Libraries.BaseDbExtensions;
using FerPROJ.Web.Libraries.BaseServices;
using System;
using System.Collections.Generic;
using System.Text;

namespace FerPROJ.Web.Libraries.BaseDbHelper {
    public static class BaseDbMigration {
        public static async Task RunDatabaseMigrationAsync<TContext>() where TContext : BaseDbContext {

            // Db Context
            var dbContext = BaseServiceManager.Get<TContext>.Service();

            // Create
            await dbContext.Database.EnsureCreatedAsync();

            // 1. Check if the database exists
            if (!await dbContext.Database.CanConnectAsync()) {
                return;
            }

            // 2. Run custom seeders (classes implementing IDbContextMigration<T>)
            var migrationTypes = typeof(BaseIDbMigration<>).GetTypesOf();

            // 3. Loop through each migration type and execute its RunMigrationAsync method
            foreach (var migrationType in migrationTypes) {
                var migrationInstance = Activator.CreateInstance(migrationType);

                // Find the RunMigrationAsync method and invoke it
                var method = migrationType.GetMethod("RunMigrationAsync");
                if (method != null) {
                    var task = (Task)method.Invoke(migrationInstance, new object[] { dbContext });
                    await task; // wait for async method
                }
            }
        }
    }
}
