using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mardis.Engine.Web.ViewModel.Migration
{
  public  class MigrationTaskViewModel
    {
        public Guid Idtask { get; set; }
 
        public Guid Idbranch { get; set; }

        public Guid IdAcccount { get; set; }
    }
}
