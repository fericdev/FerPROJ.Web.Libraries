using FerPROJ.Web.Libraries.BaseDataHelper;
using System;
using System.Collections.Generic;
using System.Text;

namespace FerPROJ.Web.Libraries.BaseModels {
    public abstract class BaseModel : ModelValidator {
        public Guid Id { get; set; } = Guid.NewGuid();
        public DateTime DateCreated { get; set; } = DateTime.Now;
        public DateTime? DateModified { get; set; } = null;
        public DateTime? DateDeleted { get; set; } = null;
        public Guid CreatedById { get; set; }
        public Guid? ModifiedById { get; set; } = null;
        public string Status { get; set; } = SystemConstants.ACTIVE_STATUS;
    }
}
