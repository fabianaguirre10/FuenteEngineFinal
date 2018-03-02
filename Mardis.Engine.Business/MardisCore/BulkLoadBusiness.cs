using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using Mardis.Engine.DataAccess;
using Mardis.Engine.DataAccess.MardisCore;
using Mardis.Engine.DataObject.MardisCommon;
using Mardis.Engine.DataObject.MardisCore;
using Mardis.Engine.Framework;
using Mardis.Engine.Framework.Resources;
using Mardis.Engine.Web.ViewModel.BranchViewModels;
using Mardis.Engine.Web.ViewModel.TaskViewModels;

namespace Mardis.Engine.Business.MardisCore
{
    /// <summary>
    /// Clase de negocios de cargas masivas
    /// </summary>
    public class BulkLoadBusiness : ABusiness
    {
        private readonly BulkLoadDao _bulkLoadDao;
        private readonly BulkLoadCatalogDao _bulkLoadCatalogDao;
        private readonly BulkLoadStatusDao _bulkLoadStatusDao;
        private readonly string _connectionString;
        private readonly BranchDao _branchDao;
        private readonly TaskCampaignDao _taskCampaignDao;
        private readonly BranchMigrateDao _branchMigrateDao;
        private readonly IList<TaskMigrateResultViewModel> lstTaskResult = new List<TaskMigrateResultViewModel>();
        private readonly IList<BranchMigrate> lsBranch = new List<BranchMigrate>();
        private readonly PersonDao _personDao;
        public BulkLoadBusiness(MardisContext mardisContext, string connectionString)
               : base(mardisContext)
        {
            _personDao = new PersonDao(mardisContext);
            _branchMigrateDao = new BranchMigrateDao(mardisContext);
            _branchDao = new BranchDao(mardisContext);
            _taskCampaignDao = new TaskCampaignDao(mardisContext);
            _bulkLoadDao = new BulkLoadDao(mardisContext);
            _bulkLoadCatalogDao = new BulkLoadCatalogDao(mardisContext);
            _bulkLoadStatusDao = new BulkLoadStatusDao(mardisContext);
            _connectionString = connectionString;
        }

        /// <summary>
        /// Dame datos por cuenta
        /// </summary>
        /// <param name="idAccount"></param>
        /// <returns></returns>
        public List<BulkLoad> GetDataByAccount(Guid idAccount)
        {
            var itemsReturn = _bulkLoadDao.GetDataByAccount(idAccount);

            foreach (var itemTemp in itemsReturn)
            {
                itemTemp.BulkLoadCatalog = _bulkLoadCatalogDao.GetOne(itemTemp.IdBulkLoadCatalog);
                itemTemp.BulkLoadStatus = _bulkLoadStatusDao.GetOne(itemTemp.IdBulkLoadStatus);
            }

            return itemsReturn;
        }

        /// <summary>
        /// Crear nuevo proceso
        /// </summary>
        /// <param name="idAccount"></param>
        /// <param name="idBulkCatalog"></param>
        /// <param name="fileName"></param>
        /// <param name="totalLines"></param>
        /// <returns></returns>
        public BulkLoad CreateNewProcess(Guid idAccount, Guid idBulkCatalog, string fileName, int totalLines)
        {
            var oneBulkLoad = new BulkLoad
            {
                BulkLoadStatus = _bulkLoadStatusDao.GetOneByCode(CBulkLoad.StateBulk.Pendiente)
            };

            oneBulkLoad.IdBulkLoadStatus = oneBulkLoad.BulkLoadStatus.Id;
            oneBulkLoad.BulkLoadCatalog = _bulkLoadCatalogDao.GetOne(idBulkCatalog);
            oneBulkLoad.IdBulkLoadCatalog = oneBulkLoad.BulkLoadCatalog.Id;
            oneBulkLoad.FileName = fileName;
            oneBulkLoad.ContainerName = string.Empty;
            oneBulkLoad.CreatedDate = DateTime.Now;
            oneBulkLoad.IdAccount = idAccount;
            oneBulkLoad.StatusRegister = CStatusRegister.Active;
            oneBulkLoad.TotalAdded = 0;
            oneBulkLoad.TotalFailed = 0;
            oneBulkLoad.TotalUpdated = 0;
            oneBulkLoad.TotalRegister = totalLines;

            _bulkLoadDao.InsertOrUpdate(oneBulkLoad);

            return oneBulkLoad;
        }

        /// <summary>
        /// Procesar archivo
        /// </summary>
        /// <param name="idAccount"></param>
        /// <param name="idBulkCatalog"></param>
        /// <param name="characteristicBulk"></param>
        /// <param name="fileName"></param>
        /// <param name="bufferArray"></param>
        public Guid ProcessFile(Guid idAccount, Guid idBulkCatalog,
                                string characteristicBulk,
                                string fileName,
                                byte[] bufferArray)
        {
            var oneBulkCatalog = _bulkLoadCatalogDao.GetOne(idBulkCatalog);
            var oneBulkLoad = CreateNewProcess(idAccount, idBulkCatalog, fileName, FileUtil.NumberFiles(bufferArray));
            var returnValue = oneBulkLoad.Id;

            try
            {

                switch (oneBulkCatalog.Code)
                {
                    case CBulkLoad.TypeBulk.BulkBranch:

                        var oneBulkBranch = new BulkLoadBranchBusiness(_connectionString, characteristicBulk, idAccount, oneBulkLoad.Id, bufferArray);
                        Thread oneThread = new Thread(oneBulkBranch.InitProcess);

                        oneThread.Start();
                        break;
                }
            }
            catch
            {
                // ignored
            }

            return returnValue;
        }

        public BulkLoad GetOne(Guid id)
        {
            var itemReturn = _bulkLoadDao.GetOne(id);

            itemReturn.BulkLoadCatalog = _bulkLoadCatalogDao.GetOne(itemReturn.IdBulkLoadCatalog);
            itemReturn.BulkLoadStatus = _bulkLoadStatusDao.GetOne(itemReturn.IdBulkLoadStatus);

            return itemReturn;
        }
        public IList<TaskMigrateResultViewModel> taskMigrate(string fileBrachMassive, Guid idAccount, Guid idcampaing)
        {
            int j = 0;

      
            using (SpreadsheetDocument doc = SpreadsheetDocument.Open(fileBrachMassive, false))
            {
              
                //Read the first Sheets 
                Sheet sheet = doc.WorkbookPart.Workbook.Sheets.GetFirstChild<Sheet>();
                Worksheet worksheet = (doc.WorkbookPart.GetPartById(sheet.Id.Value) as WorksheetPart).Worksheet;
                IEnumerable<Row> rows = worksheet.GetFirstChild<SheetData>().Descendants<Row>();

                foreach (Row row in rows)
                {
                    j++;
                    //Read the first row as header
                    if (row.RowIndex.Value != 1)
                    {

                        BranchMigrate BranchModel = new BranchMigrate();
                        int i = 0;
                        foreach (Cell cell in row.Descendants<Cell>())
                        {

                            try
                            {
                                i++;
                                switch (i)
                                {
                                    case 1:
                                        BranchModel.Code = GetCellValue(doc, cell);
                                        break;
                                    case 2:
                                        BranchModel.BranchType = GetCellValue(doc, cell);
                                        break;
                                    case 3:
                                        BranchModel.BranchName = GetCellValue(doc, cell);
                                        break;
                                    case 4:
                                        BranchModel.BranchStreet = GetCellValue(doc, cell);
                                        break;
                                    case 5:
                                        BranchModel.BranchReference = GetCellValue(doc, cell);
                                        break;
                                    case 6:
                                        BranchModel.PersonName = GetCellValue(doc, cell);
                                        break;
                                    case 7:
                                        BranchModel.Document = GetCellValue(doc, cell);
                                        break;
                                    case 8:
                                        BranchModel.phone = GetCellValue(doc, cell);
                                        break;
                                    case 9:
                                        BranchModel.Mobil = GetCellValue(doc, cell);
                                        break;
                                    case 10:
                                        string lat = GetCellValue(doc, cell);
                                        BranchModel.LatitudeBranch = lat.Length <= 10 ? lat : lat.Substring(0, 10);

                                        break;
                                    case 11:
                                        string len = GetCellValue(doc, cell);
                                        BranchModel.LenghtBranch = len.Length <= 10 ? len : len.Substring(0, 10);

                                        break;
                                    case 12:
                                        BranchModel.IdProvice = _branchMigrateDao.GetProviceByName(GetCellValue(doc, cell));
                                        Isval(BranchModel.IdProvice.ToString(), 4, j);
                                        break;
                                    case 13:
                                        BranchModel.IdDistrict = _branchMigrateDao.GetDistrictByName(GetCellValue(doc, cell), BranchModel.IdProvice);
                                        Isval(BranchModel.IdProvice.ToString(), 5, j);
                                        break;
                                    case 14:
                                        BranchModel.IdParish = _branchMigrateDao.GetParishByName(GetCellValue(doc, cell), BranchModel.IdDistrict);
                                        Isval(BranchModel.IdProvice.ToString(), 6, j);
                                        break;
                                    case 15:
                                        BranchModel.IdSector = _branchMigrateDao.GetSectorByName(GetCellValue(doc, cell), BranchModel.IdDistrict);
                                        Isval(BranchModel.IdProvice.ToString(), 7, j);
                                        break;
                                    case 16:
                                        BranchModel.Rute = GetCellValue(doc, cell);
                                        break;
                                    case 17:
                                        BranchModel.IMEI = GetCellValue(doc, cell);
                                        Isval(BranchModel.IMEI.ToString(), 8, j);
                                        break;

                                }
                            }
                            catch (Exception e)
                            {

                                var ex = e.Message.ToString();
#pragma warning disable CS0219 // La variable 'ne' está asignada pero su valor nunca se usa
                                int ne = -1;
#pragma warning restore CS0219 // La variable 'ne' está asignada pero su valor nunca se usa
                                switch (ex)
                                {
                                    case "Error al consultar Cuidad":
                                        ne = 2;
                                        break;
                                    case "Error al consultar Parroquias":
                                        ne = 3;
                                        break;
                                    case "Error al consultar Sectores":
                                        ne = 4;
                                        break;

                                }

                            }

                        }
                        if (row.RowIndex.Value != 1)
                        {
                            lsBranch.Add(BranchModel);
                        }
                    }

                }


            }
            // 
            IList<TaskMigrateResultViewModel> result = new List<TaskMigrateResultViewModel>();
            int numberError = lstTaskResult.Where(x => x.type == "E").Count();
            if (numberError < 1)
            {
                _branchMigrateDao.SaveBranchMigrate(lsBranch, idAccount, idcampaing);
            }
            else
            {

                result.Add(new TaskMigrateResultViewModel { description = "Errores", Element = numberError.ToString() });
            }
            result.Add(new TaskMigrateResultViewModel { description = "Registros", Element = (j - 1).ToString() });
            return result;
        }
        private string GetCellValue(SpreadsheetDocument doc, Cell cell)
        {
            string value = "";
            if (cell.CellValue != null)
            {

                value = cell.CellValue.InnerText;
                if (cell.DataType != null && cell.DataType.Value == CellValues.SharedString)
                {
                    return doc.WorkbookPart.SharedStringTablePart.SharedStringTable.ChildElements.GetItem(int.Parse(value)).InnerText;
                }
            }
            else
            {
                value = "NA";

            }
            return value;

        }
        public string Isval(string data, int col, int fil)
        {

#pragma warning disable CS0219 // La variable 'ex' está asignada pero su valor nunca se usa
            var ex = "";
#pragma warning restore CS0219 // La variable 'ex' está asignada pero su valor nunca se usa
#pragma warning disable CS0219 // La variable 'ne' está asignada pero su valor nunca se usa
            int ne = -1;
#pragma warning restore CS0219 // La variable 'ne' está asignada pero su valor nunca se usa
            switch (col)
            {
                case 1:
                    if (data == null || data == " " || data == "NA")
                        lstTaskResult.Add(new TaskMigrateResultViewModel { description = "El codigó se encuentra en vacio", line = fil, type = "E" });
                    break;
                case 2:
                    if (data == null || data == " " || data == "NA")
                        lstTaskResult.Add(new TaskMigrateResultViewModel { description = "La latitud se encuentra en vacia", line = fil, type = "E" });
                    break;
                case 3:
                    if (data == null || data == " " || data == "NA")
                        lstTaskResult.Add(new TaskMigrateResultViewModel { description = "La longitud se encuentra vacia ", line = fil, type = "E" });
                    break;
                case 4:
                    if (data == null || data == "00000000-0000-0000-0000-000000000000")
                        lstTaskResult.Add(new TaskMigrateResultViewModel { description = "La Provicia se encuentra vacia o no existe en la base de datos", line = fil, type = "E" });
                    break;
                case 5:
                    if (data == null || data == "00000000-0000-0000-0000-000000000000") lstTaskResult.Add(new TaskMigrateResultViewModel
                    { description = "La Cuidad se encuentra vacia o no existe en la base de datos", line = fil, type = "E" });
                    break;
                case 6:
                    if (data == null || data == "00000000-0000-0000-0000-000000000000") lstTaskResult.Add(new TaskMigrateResultViewModel
                    { description = "La Parroquia se encuentra vacia o no existe en la base de datos", line = fil, type = "E" });
                    break;
                case 7:
                    if (data == null || data == "00000000-0000-0000-0000-000000000000") lstTaskResult.Add(new TaskMigrateResultViewModel
                    { description = "El sector se encuentra vacio o no existe en la base de datos", line = fil, type = "E" });
                    break;
                case 8:
                    if (_personDao.GetPersonByCode(data) == null) lstTaskResult.Add(new TaskMigrateResultViewModel
                    { description = "El IMEI no se encuentra asignado a ningún encuestador", line = fil, type = "E" });
                    break;
            }
            return "";
        }
    }
}
