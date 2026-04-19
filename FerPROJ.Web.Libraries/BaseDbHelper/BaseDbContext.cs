using FerPROJ.Web.Libraries.BaseDataHelper;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace FerPROJ.Web.Libraries.BaseDbHelper {
    public partial class BaseDbContext : DbContext {
        public BaseDbContext(DbContextOptions options)
            : base(options) {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder) {
            foreach (var entity in modelBuilder.Model.GetEntityTypes()) {
                entity.SetTableName(entity.DisplayName());
            }
        }
    }
}
