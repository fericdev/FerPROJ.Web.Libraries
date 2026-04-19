using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace FerPROJ.Web.Libraries.BaseEntities {
    public abstract class BaseEntity {
        [Key]
        public Guid Id { get; set; }
        public DateTime? DateCreated { get; set; } = null;
        public DateTime? DateModified { get; set; } = null;
        public DateTime? DateDeleted { get; set; } = null;
        public Guid CreatedById { get; set; }
        public Guid? ModifiedById { get; set; } = null;
        public string Status { get; set; }
    }
}
