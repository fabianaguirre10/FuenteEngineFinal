using System;
using System.IO;
using System.Linq;
using System.Net;
using Mardis.Engine.Business.MardisCore;
using Mardis.Engine.DataAccess;
using Mardis.Engine.Framework;
using Mardis.Engine.Framework.Resources;
using Mardis.Engine.Web.Libraries.Security;
using Mardis.Engine.Web.Libraries.Util;
using Mardis.Engine.Web.Model;
using Mardis.Engine.Web.Util;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace Mardis.Engine.Web.Controllers
{
    /// <summary>
    /// Controlador Bulk Load
    /// </summary>
    [Authorize]
    public class BulkLoadController : AController<BulkLoadController>
    {
        private readonly BulkLoadBusiness _bulkLoadBusiness;
        private readonly BulkLoadCatalogBusiness _bulkLoadCatalogBusiness;
        private IHostingEnvironment _Env;
        private readonly TaskCampaignBusiness _taskCampaignBusiness;
        public BulkLoadController(UserManager<ApplicationUser> userManager,
                                  IHttpContextAccessor httpContextAccessor,
                                  MardisContext mardisContext,
                                  ILogger<BulkLoadController> logger,
                                    RedisCache distributedCache,
                                  IHostingEnvironment envrnmt)
            : base(userManager, httpContextAccessor, mardisContext, logger)
        {
            _taskCampaignBusiness = new TaskCampaignBusiness(mardisContext, distributedCache);
            _Env = envrnmt;
            _bulkLoadBusiness = new BulkLoadBusiness(mardisContext, Startup.Configuration.GetConnectionString("DefaultConnection"));
            _bulkLoadCatalogBusiness = new BulkLoadCatalogBusiness(mardisContext);

        }

        /// <summary>
        /// View de índice
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public IActionResult Index()
        {

            ViewData[CBulkLoad.Catalog] = _bulkLoadCatalogBusiness.GetLoadCatalog();

            return View();
        }

        /// <summary>
        /// Dame Resultados
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public string GetResults()
        {
            Guid idAccount = ApplicationUserCurrent.AccountId;

            var itemsResults = _bulkLoadBusiness.GetDataByAccount(idAccount);

            return JSonConvertUtil.Convert(itemsResults);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public string GetBulkLoadById(string inputProcess)
        {
            Guid idProcess = new Guid(inputProcess);

            return JSonConvertUtil.Convert(_bulkLoadBusiness.GetOne(idProcess));
        }

        /// <summary>
        /// View Cargar Archivo
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public IActionResult LoadFile(string idBulkCatalog)
        {
            SharedMemory.Remove(CBulkLoad.CSessionFile);

            ViewData[CBulkLoad.SelectCatalog] = _bulkLoadCatalogBusiness.GetOne(new Guid(idBulkCatalog));

            return View();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="idBulkCatalog"></param>
        /// <param name="nameFile"></param>
        /// <returns></returns>
        [HttpGet]
        public IActionResult ResumeBulkLoad(string idBulkCatalog, string nameFile)
        {
            ViewData[CBulkLoad.SelectCatalog] = idBulkCatalog;
            ViewData[CBulkLoad.CFileName] = nameFile;

            return View();
        }

        /// <summary>
        /// Iniciar proceso
        /// </summary>
        /// <param name="inputBulkCatalog"></param>
        /// <param name="characteristBulk"></param>
        /// <param name="fileName"></param>
        [HttpPost]
        public Guid InitProcess(string inputBulkCatalog, string characteristBulk, string fileName)
        {
            var idAccount = ApplicationUserCurrent.AccountId;
            var idBulkCatalogo = new Guid(inputBulkCatalog);
            var bufferFile = (byte[])SharedMemory.Get(CBulkLoad.CSessionFile);

            var isValidProcess = _bulkLoadBusiness.ProcessFile(idAccount, idBulkCatalogo,
                characteristBulk, fileName,
                bufferFile);

            return isValidProcess;
        }


        /// <summary>
        /// Descargar Archivos
        /// </summary>
        [HttpPost]
        public ContentResult DownloadFile(string idBulkCatalog)
        {
            var oneBulkCatalog = _bulkLoadCatalogBusiness.GetOne(new Guid(idBulkCatalog));
            string returnValue = string.Empty;
            int? statusCode = (int)HttpStatusCode.OK;

            foreach (var fileTemp in Request.Form.Files)
            {
                var fileName = fileTemp.FileName;
                int indexSeparator = fileName.LastIndexOf(".", StringComparison.Ordinal);

                //validar extensión de archivo
                if (0 < indexSeparator)
                {
                    if (!CBulkLoad.Extention.Equals(fileName.Substring(indexSeparator + 1).ToUpper()))
                    {
                        returnValue = "Archivo no válido, extensión no válida";
                        statusCode = (int)HttpStatusCode.NotAcceptable;
                        break;
                    }
                }
                else
                {
                    returnValue = "Archivo no válido, no hay extensión";
                    statusCode = (int)HttpStatusCode.NotAcceptable;
                    break;
                }

                //validacion de contenido
                if (1 < fileTemp.Length)
                {
                    byte[] bufferDocument;
                    returnValue = BulkLoadUtil.ValidateContentBulkFile(fileTemp, oneBulkCatalog.Separator
                                                                       , oneBulkCatalog.ColumnNumber, out bufferDocument);

                    if (string.IsNullOrEmpty(returnValue))
                    {

                        SharedMemory.Set(CBulkLoad.CSessionFile, bufferDocument);

                        returnValue = "Archivo válido";
                        statusCode = (int)HttpStatusCode.OK;
                        break;
                    }

                    statusCode = (int)HttpStatusCode.NotAcceptable;
                    break;
                }

                returnValue = "Error, Archivo Vacio";
                statusCode = (int)HttpStatusCode.NotAcceptable;
            }

            var response = new ContentResult
            {
                Content = returnValue,
                StatusCode = statusCode
            };

            return response;
        }
        #region cargaMasiva
        [HttpGet]
        public IActionResult Massive(Guid idTask, Guid? idCampaign = null, string returnUrl = null)
        {

            try
            {
                ViewData["ReturnUrl"] = returnUrl;

                TempData["Idcampaing"] = JsonConvert.SerializeObject(idCampaign);
                return View();
            }
            catch (Exception e)
            {
                _logger.LogError(new EventId(0, "Error Index"), e.Message);
                return RedirectToAction("Index", "StatusCode", new { statusCode = 1 });
            }
        }
        [HttpPost]
        public IActionResult Massive(IFormFile fileBranch)
        {
            DateTime localDate = DateTime.Now;
            if (fileBranch == null)
            {

                ViewBag.error = "Verfique si el archivo fue cargado";
                return Json("-1");
            }
            Guid idcampaing = JsonConvert.DeserializeObject<Guid>(TempData["Idcampaing"].ToString());
            string LogFile = localDate.ToString("yyyyMMddHHmmss");
            var Filepath = _Env.WebRootPath + "\\Form\\ " + LogFile + "_" + fileBranch.FileName.ToString();
         
            using (var fileStream = new FileStream(Filepath, FileMode.Create))
            {
                fileBranch.CopyTo(fileStream);
               
            }


            //byte[] by2tes = System.IO.File.ReadAllBytes(Filepath);
            //if (Directory.Exists(Path.GetDirectoryName(Filepath)))
            //{
            //    System.IO.File.Delete(Filepath);
            //}
            ////Filepath = _taskCampaignBusiness.UploadFile(by2tes, LogFile + "_" + fileBranch.FileName.ToString());

            return RedirectToAction("LoadTask", "BulkLoad", new { @idCampaign = idcampaing, @path = Filepath, @nameFile = fileBranch.FileName.ToString() });
        }
        [HttpGet]
        public IActionResult LoadTask(Guid idCampaign, string path = null, string nameFile = null)
        {

            try
            {
                ViewBag.file = nameFile;
                ViewBag.path = path;
                ViewBag.idcampingn = idCampaign;
                //Guid idAccount = ApplicationUserCurrent.AccountId;
                //var msg =_taskCampaignBusiness.taskMigrate(path, idAccount, idCampaign);

                return View();
            }
            catch (Exception e)
            {
                _logger.LogError(new EventId(0, "Error Index"), e.Message);
                return RedirectToAction("Index", "StatusCode", new { statusCode = 1 });
            }
        }
        [HttpPost]
        public JsonResult LoadTask(string idcampaign, string idpath)
        {

            try
            {
                Guid idCampaignGuid = Guid.Parse(idcampaign);
                Guid idAccount = ApplicationUserCurrent.AccountId;
                var data = _bulkLoadBusiness.taskMigrate(idpath, idAccount, idCampaignGuid);
                var rows = from x in data
                           select new
                           {
                               description = x.description,
                               data = x.Element

                           };
                if (Directory.Exists(Path.GetDirectoryName(idpath)))
                {
                    System.IO.File.Delete(idpath);
                }
     
                var jsondata = rows.ToArray();
                return Json(jsondata);

            }
            catch (Exception e)
            {
                if (Directory.Exists(Path.GetDirectoryName(idpath)))
                {
                    System.IO.File.Delete(idpath);
                }

                _logger.LogError(new EventId(0, "Error Index"), e.Message);
                return Json("error");
            }
        }
        #endregion
    }

}
