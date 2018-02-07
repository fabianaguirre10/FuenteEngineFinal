using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Mardis.Engine.Web.Controllers
{
    public class EquipmentController : Controller
    {
        // GET: Equipment
        public ActionResult Index()
        {
            return View();
        }

      
    }
}