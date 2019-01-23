
using Mardis.Engine.DataAccess.MardisCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mardis.Engine.DataAccess
{

    [Table("logError", Schema = "MardisCore")]
    public  class logApi : IEntityId
    {
        [Key]
        public int Id { get; set; }
        public string logs { get; set; }
        public string controller { get; set; }
        public string views { get; set; }
        public DateTime creationDate { get; set; }

    }
}
