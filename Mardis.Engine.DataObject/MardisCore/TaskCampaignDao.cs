﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Dynamic.Core;
using Mardis.Engine.DataAccess;
using Mardis.Engine.DataAccess.MardisCore;
using Mardis.Engine.DataObject.Dto;
using Mardis.Engine.DataObject.MardisCommon;
using Mardis.Engine.Framework.Resources;
using Mardis.Engine.Web.ViewModel.Filter;
using Mardis.Engine.Web.ViewModel.Migration;
using Mardis.Engine.Web.ViewModel.TaskViewModels;
using Microsoft.EntityFrameworkCore;

namespace Mardis.Engine.DataObject.MardisCore
{
    public class TaskCampaignDao : ADao
    {

        public TaskCampaignDao(MardisContext mardisContext) : base(mardisContext)
        {
            CoreFilterDetailDao = new CoreFilterDetailDao(mardisContext);
        }

        public TaskCampaign GetTaskCampaignById(Guid idTask, Guid idAccount)
        {
            var task = Context.TaskCampaigns
                .Include(t => t.Campaign)
                .ThenInclude(c => c.CampaignServices)
                .ThenInclude(cs => cs.Service)
                .FirstOrDefault(t => t.StatusRegister == CStatusRegister.Active &&
                                     t.Id == idTask &&
                                     t.IdAccount == idAccount);
            return task;
        }

        public TaskCampaign Get(Guid idTask, Guid idAccount)
        {
            return Context.TaskCampaigns
                .FirstOrDefault(
                    t => t.StatusRegister == CStatusRegister.Active &&
                    t.Id == idTask &&
                    t.IdAccount == idAccount);
        }

        public MyTaskViewModel GetForProfile(Guid idTask, Guid idAccount)
        {

#if DEBUG
            var myWatch = new Stopwatch();
            myWatch.Start();
#endif

            var task=Context.Query<MyTaskViewModel>($@"select a.*, b.Name as MerchantName, b.Surname as MerchantSurname,a.CodigoGemini as CodeGemini
            from vw_Campaign_Information a
                inner join mardiscommon.person b on a.idmerchantperson = b.id
            where a.IdTask='{idTask}'").FirstOrDefault();

#if DEBUG
            myWatch.Stop();
            Debugger.Log(0, "Consulta", $"ms: {myWatch.ElapsedMilliseconds}");
#endif

            return task;
        }
        public List<MyTaskViewModel> GetForProfileLista(Guid idAccount)
        {

#if DEBUG
            var myWatch = new Stopwatch();
            myWatch.Start();
#endif

            List<MyTaskViewModel> task = Context.Query<MyTaskViewModel>($@"select a.*, b.Name as MerchantName, b.Surname as MerchantSurname
            from vw_Campaign_Information a
                inner join mardiscommon.person b on a.idmerchantperson = b.id
            where a.idaccount='{idAccount}'").ToList();

#if DEBUG
            myWatch.Stop();
            Debugger.Log(0, "Consulta", $"ms: {myWatch.ElapsedMilliseconds}");
#endif

            return task;
        }

        public TaskCampaign GetTaskByIdForProfilePage(Guid idTask, Guid idAccount)
        {
            var task = Context.TaskCampaigns
                .Include(t => t.Campaign)
                    .ThenInclude(c => c.CampaignServices)
                        .ThenInclude(cs => cs.Service)
                            .ThenInclude(sd => sd.ServiceDetails)
                        .ThenInclude(sc => sc.Sections)
                            .ThenInclude(sq => sq.Questions)
                                .ThenInclude(sqd => sqd.QuestionDetails)
                .Include(t => t.Campaign)
                    .ThenInclude(c => c.CampaignServices)
                        .ThenInclude(cs => cs.Service)
                            .ThenInclude(sd => sd.ServiceDetails)
                                .ThenInclude(sc => sc.Sections)
                                    .ThenInclude(sq => sq.Questions)
                                        .ThenInclude(t => t.TypePoll)
                .Include(t => t.Campaign)
                    .ThenInclude(c => c.CampaignServices)
                        .ThenInclude(cs => cs.Service)
                            .ThenInclude(sd => sd.ServiceDetails)
                                .ThenInclude(q => q.Questions)
                                    .ThenInclude(qd => qd.TypePoll)
                .Include(t => t.Campaign)
                    .ThenInclude(c => c.Customer)
                .Include(t => t.Merchant)
                    .ThenInclude(m => m.Profile)
                .Include(t => t.Merchant)
                    .ThenInclude(m => m.Person)
                .Include(t => t.Branch)
                    .ThenInclude(b => b.PersonOwner)
                .Include(t => t.Branch)
                    .ThenInclude(b => b.Parish)
                .Include(t => t.Branch)
                    .ThenInclude(b => b.Sector)
                 .Include(t => t.Branch)
                    .ThenInclude(b => b.Province)
                .Include(t => t.Branch)
                    .ThenInclude(b => b.District)
                .Include(t => t.StatusTask)
                .FirstOrDefault(t => t.StatusRegister == CStatusRegister.Active &&
                                     t.Id == idTask &&
                                     t.IdAccount == idAccount);
            return task;
        }

        public TaskCampaign GetTaskByIdForRegisterPage(Guid idTask, Guid idAccount)
        {
            var itemResult = Context.TaskCampaigns
                .Include(t => t.Branch)
                .Include(t => t.Campaign)
                .Include(t => t.StatusTask)
                .Include(t => t.Merchant)
                    .ThenInclude(m => m.Profile)
                .AsNoTracking()
                .FirstOrDefault(t => t.StatusRegister == CStatusRegister.Active &&
                            t.Id == idTask &&
                            t.IdAccount == idAccount);
            return itemResult;
        }

        public List<TaskCampaign> GetAlltasksByCampaignId(Guid idCampaign, Guid idAccount)
        {
            var resultList = Context.TaskCampaigns
                .Where(t => t.IdCampaign == idCampaign &&
                            t.StatusRegister == CStatusRegister.Active &&
                            t.IdAccount == idAccount)
                .ToList();

            return resultList;
        }

        public List<TaskCampaign> GetAlltasksByCampaignNewId(Guid? idCampaign)
        {
            var resultList = Context.TaskCampaigns
                .Include(t => t.Branch)
                .Include(t => t.Campaign)
                .Include(t => t.StatusTask)
                .Include(t => t.Branch.PersonAdministration)
                .Include(t => t.Branch.Province)
                .Include(t => t.Branch.District)
                .Include(t => t.Branch.Sector)
                .Where(t => t.IdCampaign == idCampaign &&
                            t.StatusRegister == CStatusRegister.Active)
                .ToList();

            return resultList;
        }

       

        public List<TaskCampaign> GetAlltasksByCampaignId(Guid idCampaign, Guid idAccount, List<FilterValue> filters)
        {
            var strPredicate = $" StatusRegister == \"{CStatusRegister.Active}\" " +
                               $"&& IdAccount == \"{idAccount.ToString()}\" " +
                               $"&& IdCampaign == \"{idCampaign.ToString()}\" ";

            strPredicate += GetFilterPredicate(filters);

            return Context.TaskCampaigns
                .Where(strPredicate)
                .ToList();
        }

        public List<TaskCampaign> GetTasksByStatusAndUser(string nameStatus, Guid userId, Guid idAccount)
        {
            var resultList = Context.TaskCampaigns
                .Include(t => t.Branch)
                .Include(t => t.Campaign)
                .Where(t => t.StatusTask.Name == nameStatus &&
                            t.StatusRegister == CStatusRegister.Active &&
                            t.IdMerchant == userId &&
                            t.IdAccount == idAccount)
                .OrderByDescending(t => t.DateModification)
                .ToList();

            return resultList;
        }

        public List<TaskCampaign> GetTasksByCampaignAndStatus(Guid idCampaign, string nameStatus, Guid idAccount)
        {
            return Context.TaskCampaigns
                .Include(t => t.Branch)
                .Include(t => t.Campaign)
                .Where(t => t.StatusTask.Name == nameStatus &&
                            t.StatusRegister == CStatusRegister.Active &&
                            t.IdCampaign == idCampaign &&
                            t.IdAccount == idAccount)
                .OrderByDescending(t => t.DateModification)
                .ToList();
        }

        public List<TaskCampaign> GetPaginatedTasksByCampaign(int pageIndex, int pageSize, List<FilterValue> filterValues,
            Guid idAccount)
        {
            var strPredicate = $" StatusRegister == \"{CStatusRegister.Active}\" " +
                               $"&& IdAccount == \"{idAccount.ToString()}\" ";

            strPredicate += GetFilterPredicate(filterValues);

            return Context.TaskCampaigns
                .Include(t => t.Branch)
                .Include(t => t.StatusTask)
                .Where(strPredicate)
                .OrderByDescending(t => t.DateModification)
                .Skip((pageIndex - 1) * pageSize)
                .Take(pageSize)
                .ToList();
        }

        public List<TaskCampaign> GetPaginatedTasksByCampaignAndStatus(string nameStatus, int pageIndex, int pageSize, List<FilterValue> filterValues, Guid idAccount)
        {

            var strPredicate = $"StatusTask.Name ==  \"{nameStatus}\" " +
                               $"&& StatusRegister == \"{CStatusRegister.Active}\" " +
                               $"&& IdAccount == \"{idAccount.ToString()}\" ";

            var dataElement = Context.TaskCampaigns
                .Include(t => t.Branch)
                .Include(t => t.Campaign);

            strPredicate += GetFilterPredicate(filterValues);

            var resultsList = dataElement
                .Where(strPredicate)
                .OrderByDescending(t => t.DateModification)
                .Skip((pageIndex - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            return resultsList;
        }

        public int GetTaskCountByCampaignAndStatus(string nameStatus, List<FilterValue> filterValues, Guid idAccount)
        {
            var strPredicate = $"StatusTask.Name ==  \"{nameStatus}\" " +
                         $"&& StatusRegister == \"{CStatusRegister.Active}\" " +
                         $"&& IdAccount == \"{idAccount.ToString()}\" ";

            strPredicate += GetFilterPredicate(filterValues);

            return Context.TaskCampaigns
                .Count(strPredicate);
        }
        public int GetTaskCountByCampaignAndStatusStadi(string nameStatus, Guid idcamp, Guid idAccount)
        {
            var strPredicate = $"StatusTask.Name ==  \"{nameStatus}\" " +
                         $"&& StatusRegister == \"{CStatusRegister.Active}\" " +
                         $"&& IdAccount == \"{idAccount.ToString()}\" "+
                         $"&& IdCampaign == \"{idcamp.ToString()}\" ";

      

            return Context.TaskCampaigns
                .Count(strPredicate);
        }

        public List<TaskCampaign> GetTaskListByBranch(Guid idBranch, Guid idAccount)
        {
            return Context.TaskCampaigns
                .Include(t => t.StatusTask)
                .Include(t => t.Campaign.CampaignServices)
                    .ThenInclude(c => c.Service)
                .Include(t => t.Merchant.Profile)
                .Include(t => t.Merchant.Person)
                .Where(t => t.IdBranch == idBranch &&
                            t.StatusRegister == CStatusRegister.Active &&
                            t.IdAccount == idAccount)
                .ToList();
        }

        public List<TaskCampaign> GetTaskListByCampaignIdsAndBranch(Guid idBranch, Guid[] idCampaigns, Guid idAccount)
        {
            return Context.TaskCampaigns
                .Include(t => t.Campaign)
                    .ThenInclude(c => c.Supervisor)
                        .ThenInclude(s => s.Profile)
                .Include(t => t.Merchant)
                    .ThenInclude(s => s.Profile)
                .Where(t => t.IdBranch == idBranch &&
                            idCampaigns.Contains(t.IdCampaign) &&
                            t.StatusRegister == CStatusRegister.Active &&
                            t.IdAccount == idAccount)
                .ToList();
        }

        public int GetTaskCountByStatus(Guid idCampaign, Guid idStatusTask, Guid idAccount)
        {
            return Context.TaskCampaigns
                .Count(t => t.IdCampaign == idCampaign &&
                            t.IdStatusTask == idStatusTask &&
                            t.IdAccount == idAccount);
        }

        public int GetTaskCountByStatusAndMerchant(Guid idCampaign, Guid idStatusTask, Guid idMerchant, Guid idAccount)
        {
            return Context.TaskCampaigns
                .Count(t => t.IdCampaign == idCampaign &&
                            t.IdStatusTask == idStatusTask &&
                            t.IdMerchant == idMerchant &&
                            t.IdAccount == idAccount);
        }

        public List<TaskCampaign> GetTaskListByCampaign(Guid idCampaign, Guid idAccount)
        {
            return Context.TaskCampaigns
                .Include(t => t.Campaign)
                    .ThenInclude(c => c.Supervisor)
                        .ThenInclude(s => s.Profile)
                .Include(t => t.Merchant)
                    .ThenInclude(s => s.Profile)
                .Where(t => t.IdCampaign == idCampaign &&
                            t.StatusRegister == CStatusRegister.Active && t.IdAccount == idAccount)
                .ToList();
        }

        public int GetTaskCountByStatus(Guid idCampaign, string statusName, Guid idAccount)
        {
            return Context.TaskCampaigns
                .Count(t => t.IdCampaign == idCampaign &&
                            t.StatusTask.Name == statusName &&
                            t.StatusRegister == CStatusRegister.Active &&
                            t.IdAccount == idAccount);
        }

        public List<TaskPerViewDto> GetTaskCountPerStatus(Guid idCampaign, Guid idAccount)
        {

            var query = $@"SELECT COUNT(*) AS [Count],  [t].[IdStatusTask], [t.StatusTask].[Name] AS StatusName
                        FROM [MardisCore].[Task] AS [t]
                        INNER JOIN [MardisCore].[StatusTask] AS [t.StatusTask] ON [t].[IdStatusTask] = [t.StatusTask].[Id]
                        WHERE (([t].[IdCampaign] = '{idCampaign}') AND ([t].[StatusRegister] = 'A')) AND ([t].[IdAccount] = '{idAccount}')
                        group BY [t].[IdCampaign], [t].[StatusRegister], [t].[IdAccount], [t].[IdStatusTask], [t.StatusTask].[Name]
                        ORDER BY [t].[IdCampaign], [t].[StatusRegister], [t].[IdAccount], [t].[IdStatusTask], [t.StatusTask].[Name]";

            return Context.Query<TaskPerViewDto>(query).ToList();

            //return Context.TaskCampaigns
            //    .Where(t => t.IdCampaign == idCampaign &&
            //                t.StatusRegister == CStatusRegister.Active &&
            //                t.IdAccount == idAccount)
            //    .GroupBy(t => new { t.IdCampaign, t.StatusRegister, t.IdAccount, t.IdStatusTask, t.StatusTask.Name })
            //    .Select(t => new TaskPerViewDto() { IdStatusTask = t.Key.IdStatusTask, Count = t.Count(), StatusName = t.Key.Name })
            //    .ToDictionary(t => t.StatusName, t => t.Count);
        }
        public List<equiposcampanias> Getvistaequiposcampanias()
        {
            var query = $@"SELECT 
                    IdTrade,	
                    URI,	
                    Nombre,	
                    Contacto,	
                    [Cédula/RUC] as Cedula_RUC,	
                    Direccion,	
                    [Tipo de Equipo] as Tipo_de_Equipo,	
                    [MID-TID] as MID_TID,
                    [Proceso_Visa] as Proceso_Visa,
                    [Adquiencia],
                    [Motivos Rechazo Visa] as Motivos_Rechazo_Visa,
                    [Otros Motivos Rechazo Visa] as Otros_Motivos_Rechazo_Visa,
                    [American Express] as American_Express,
                    [Motivos Rechazo American Express] as Motivos_Rechazo_American_Express ,
                    [Otros Motivos Rechazo American Express] as Otros_Motivos_Rechazo_American_Express,
                    [EQUIPO],
                    [CORE],
                    [Campaña] as Campania,
                     CONVERT(VARCHAR(10), Fecha, 23) as FechaNormal,
					Fecha as FechaHoras
                    FROM    vw_equipos_bg_campañas";
            var result =Context.Query<equiposcampanias>(query).ToList();

            return result;
        }

        public void ImplementTask(Guid idTask, Guid idStatus, Guid idAccount , Guid status)
        {
            var task = Get(idTask, idAccount);
            task.DateModification = DateTime.Now;
            task.IdStatusTask = status;
            InsertOrUpdate(task);
        }
        public void ImplementTaskGemini(Guid idTask, Guid idStatus, Guid idAccount, Guid status,string Codigo)
        {
            var task = Get(idTask, idAccount);
            task.DateModification = DateTime.Now;
            task.IdStatusTask = status;
            task.CodeGemini = Codigo;
            InsertOrUpdate(task);
        }
        public void ValidateTask(Guid idTask, Guid idStatus, Guid idAccount, Guid idUser)
        {
            var task = Get(idTask, idAccount);
            task.DateModification = DateTime.Now;
            task.IdStatusTask = idStatus;
            task.UserValidator = idUser;
            task.DateValidation = DateTime.Now;
            InsertOrUpdate(task);
        }

        public List<TaskCampaign> GetPaginatedTasksList(Guid idAccount, List<FilterValue> filters, int pageIndex, int pageSize)
        {
            var strPredicate =
                $" StatusRegister == \"{CStatusRegister.Active}\" && IdAccount == \"{idAccount.ToString()}\"";

            strPredicate += GetFilterPredicate(filters);

            var resultList = Context.TaskCampaigns
                .Where(strPredicate)
                .OrderByDescending(s => s.StartDate)
                .Skip((pageIndex - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            return resultList;
        }

        public int GetPaginatedTasksCount(Guid idAccount, List<FilterValue> filters)
        {
            var strPredicate =
                $" StatusRegister == \"{CStatusRegister.Active}\" && IdAccount == \"{idAccount.ToString()}\"";

            strPredicate += GetFilterPredicate(filters);

            return Context.TaskCampaigns
                .Where(strPredicate)
                .Count();
        }
        public List<StatusTask> statusAllow(Guid idaccount, int pageIndex, int pageSize) {

            var _status = from st in Context.StatusTasks
                          join stc in Context.StatusTaskAccounts
                          on st.Id equals stc.Idstatustask
                          where stc.Idaccount == idaccount
                          orderby stc.ORDER
                          select st;

            return _status.ToList();


                
             

        }

        public MigrationTaskViewModel ExcuteMigrationTask(Guid Idcampaign, string _uri)
        {

#if DEBUG
            var myWatch = new Stopwatch();
            myWatch.Start();
#endif

            var task = Context.Query<MigrationTaskViewModel>($@"EXEC dbo.sp_proc_migration_Campaign @idcampaign='{Idcampaign}',@_uri='{_uri}'").FirstOrDefault();

#if DEBUG
            myWatch.Stop();
            Debugger.Log(0, "Consulta", $"ms: {myWatch.ElapsedMilliseconds}");
#endif

            return task;
        }


        public List<AggregateDataViewModels> _dataAggregate(string IMEI, string _form)
        {

#if DEBU


#endif
            try
            {
                var myWatch = new Stopwatch();
                myWatch.Start();
                var task = Context.Query<AggregateDataViewModels>($@"select ID,IMEI_ID,campaing from vw_prc_aggregate_uri where IMEI_ID='{IMEI}' and  TableCore='{_form}'");
                return task.ToList();
                myWatch.Stop();
                Debugger.Log(0, "Consulta", $"ms: {myWatch.ElapsedMilliseconds}");
            }
            catch (Exception e)
            {

                return null;
            }
        
#if DEBUG

#endif

      
        }
        public bool TaskImages(Guid idtask)
        {
            var _exists = false;
            var _Model= from st in  Context.BranchImageses
                    
                        where st.idtask.Equals(idtask)
                        select st;
            if (_Model.ToList().Count() > 0) {
                _exists = true;       
            }
            

            return _exists;





        }
        public List<TableImage> GetMigrationImage(Guid Idcampaign, string _uri)
        {

#if DEBUG
            var myWatch = new Stopwatch();
            myWatch.Start();
#endif

            var images = Context.Query<TableImage>($@"select _uri Uri, [Table],orden, content from vw_migration_image where  campaign='{Idcampaign}' and _TOP_LEVEL_AURI='{_uri}'");

#if DEBUG
            myWatch.Stop();
            Debugger.Log(0, "Consulta", $"ms: {myWatch.ElapsedMilliseconds}");
#endif

            return images.ToList();
        }
        public ImageBinaryModel GetDataImage( string _uri , string table)
        {

#if DEBUG
            var myWatch = new Stopwatch();
            myWatch.Start();
#endif

            var _model = Context.Query<ImageBinaryModel>($@"select VALUE as _VALUE from {table} where  _URI='{_uri}'").FirstOrDefault();

#if DEBUG
            myWatch.Stop();
            Debugger.Log(0, "Consulta", $"ms: {myWatch.ElapsedMilliseconds}");
#endif

            return _model;
        }
        public void saveLogApi(string log) {

            logApi _model = new logApi();
            _model.controller = "MigrationTaskController";
            _model.creationDate = DateTime.Now;
            _model.views = "GET";
            _model.logs = log;

            Context.logApies.Add(_model);
            Context.SaveChanges();


        }
    }
}
