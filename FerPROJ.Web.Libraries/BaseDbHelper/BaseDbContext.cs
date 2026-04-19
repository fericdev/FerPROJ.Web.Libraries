using FerPROJ.Web.Libraries.BaseDataHelper;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace FerPROJ.Web.Libraries.BaseDbHelper {
    public abstract class BaseDbContext : DbContext {
        public BaseDbContext(DbContextOptions<BaseDbContext> options)
            : base(options) {
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder) {
            optionsBuilder.UseMySql(ConnectionString.ENTITY_CONNECTION_STRING, ServerVersion.AutoDetect(ConnectionString.ENTITY_CONNECTION_STRING));
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder) {
            foreach (var entity in modelBuilder.Model.GetEntityTypes()) {
                entity.SetTableName(entity.DisplayName());
            }
        }
    }
}
