using System;
using System.Collections.Generic;
using System.Text;

namespace FerPROJ.Web.Libraries.BaseDbHelper {
    public interface BaseIDbMigration<TContext> {
        Task RunMigrationAsync(TContext dbContext);
    }
}
