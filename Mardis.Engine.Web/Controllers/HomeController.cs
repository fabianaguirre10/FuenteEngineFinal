using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.Serialization;
using Mardis.Engine.Business;
using Mardis.Engine.Business.MardisCore;
using Mardis.Engine.Business.MardisSecurity;
using Mardis.Engine.DataAccess;
using Mardis.Engine.Web.Libraries.Security;
using Mardis.Engine.Web.Model;
using Mardis.Engine.Web.Services;
using Mardis.Engine.Web.ViewModel.DashBoardViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Logging;

namespace Mardis.Engine.Web.Controllers
{

    [Authorize]
    [EnableCors("CorsPolicy")]

    public class HomeController : AController<HomeController>
    {
        private readonly MenuBusiness _menuBusiness;
        private readonly CampaignBusiness _campaignBusiness;
        private readonly HomeBusiness _homeBusiness;
        private readonly IDataProtector _protector;
        private readonly IDataProtector _protectorCampaign;
        private string setting;

        public HomeController(UserManager<ApplicationUser> userManager,
                              IHttpContextAccessor httpContextAccessor,
                              MardisContext mardisContext,
                              ILogger<HomeController> logger,
                              IMenuService menuService,
                                    IDataProtectionProvider protectorProvider
                              )
            : base(userManager, httpContextAccessor, mardisContext, logger)
        {
            _protector = protectorProvider.CreateProtector(GetType().FullName);
            _protectorCampaign = protectorProvider.CreateProtector("Mardis.Engine.Web.Controllers.CampaignController");
            _menuBusiness = new MenuBusiness(mardisContext);
            _campaignBusiness = new CampaignBusiness(mardisContext);
            _homeBusiness = new HomeBusiness(mardisContext);
        }
        
        //[HttpPost(Name = "GetMenuByProfile")]
        //public IEnumerable GetMenuByProfile()
        //{
        //    var idProfile = ApplicationUserCurrent.ProfileId;
        //    var itemMenu =
        //    _menuBusiness.GetOnlyParentsByProfile(idProfile);

        //    foreach (var itemMenuTemp in itemMenu)
        //    {
        //        itemMenuTemp.MenuChildrens = _menuBusiness.GetChildrens(idProfile, itemMenuTemp.Id);
        //    }

        //    return itemMenu;login
        //}
        
        public IActionResult Index()
        {
            HttpContext.Session.SetString("das", "The Doctor");
            HttpContext.Session.SetInt32("dsa", 773);
            return View();
        }
        
        [HttpGet]


        public IActionResult DashBoard(DashBoardViewModel model, string filterValues, bool deleteFilter, int pageIndex = 1, int pageSize = 50)
        {

            if (!string.IsNullOrEmpty(model.IdCampaign))
            {
                SetSessionVariable("idCampaign", model.IdCampaign);
            }
            else if (!string.IsNullOrEmpty(filterValues) || pageIndex != 1)
            {
                model.IdCampaign = GetSessionVariable("idCampaign");
            }
         
            ViewBag.CampaignList =
                _campaignBusiness.GetActiveCampaignsListDasboard(ApplicationUserCurrent.AccountId, Guid.Parse(ApplicationUserCurrent.UserId)).OrderBy(x => x.Name)
                    .Select(c => new SelectListItem() { Value = _protectorCampaign.Protect(c.idcampaign.ToString()), Text = c.Name });
                  
            var filters = GetFilters(filterValues, deleteFilter);

            model = _homeBusiness.GetDashBoard(model, filters, pageIndex, pageSize, ApplicationUserCurrent.AccountId, _protectorCampaign);

            if (!string.IsNullOrEmpty(model.IdCampaign))
            {
                model.IdCampaign = _protectorCampaign.Protect(model.IdCampaign);
            }
            ViewBag.Account = ApplicationUserCurrent.AccountId;
            ViewBag.user = ApplicationUserCurrent.UserId;
            return View(model);
        }

        [HttpPost]
        public JsonResult GetDashBord(string idCampaign) {

          var id= _protectorCampaign.Unprotect(idCampaign);
            var url=_campaignBusiness.GetDashOne(Guid.Parse(id)).url;
            return Json(url);
        }
        public IActionResult About()
        {
            ViewData["Message"] = "Your application description page.";

            return View();
        }

        public IActionResult Contact()
        {
            ViewData["Message"] = "Your contact page.";

            return View();
        }

        public IActionResult Error()
        {
            return View();
        }
    }
}
