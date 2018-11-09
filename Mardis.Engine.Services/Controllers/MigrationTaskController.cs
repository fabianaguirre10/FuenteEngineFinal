using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Mardis.Engine.Business.MardisCore;
using Mardis.Engine.Business.MardisCommon;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Mardis.Engine.DataAccess;
using Mardis.Engine.Framework;
using Mardis.Engine.Web.ViewModel.TaskViewModels;
using Mardis.Engine.DataAccess.MardisCore;
using clases;
using Microsoft.Extensions.Logging;

namespace Mardis.Engine.Services.Controllers
{
   
    [Route("api/MigrationTask")]
    public class MigrationTaskController : Controller
    {
        private readonly TaskCampaignBusiness _taskCampaignBusiness;
        private readonly BranchBusiness _BranchBusiness;
        private readonly CodigoReservadosBusiness _CodigoReservadosBusiness;

        private ILogger _logger;
        public MigrationTaskController(MardisContext mardisContext,
       RedisCache distributedCache,
       ILoggerFactory _loggerFactory)
        {
            _taskCampaignBusiness = new TaskCampaignBusiness(mardisContext, distributedCache);
            _BranchBusiness = new BranchBusiness(mardisContext);
            _CodigoReservadosBusiness = new CodigoReservadosBusiness(mardisContext);
            _logger = _loggerFactory.CreateLogger("Mardis.Engine.Services");
        }

 
        // GET: api/MigrationTask/5
        [HttpGet()]
        public object Get(string Icampaign, string aggregateUri)
        {
            return _taskCampaignBusiness.MigrationTask(Guid.Parse(Icampaign), aggregateUri);
        }
        
        // POST: api/MigrationTask
        [HttpPost]
        public void Post([FromBody]string value)
        {
        }
        
        // PUT: api/MigrationTask/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody]string value)
        {
        }
        
        // DELETE: api/ApiWithActions/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }
    }
}
