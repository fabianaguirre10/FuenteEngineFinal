using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using Mardis.Engine.DataAccess;
using Mardis.Engine.DataAccess.MardisCore;
using Mardis.Engine.Framework.Resources;
using Mardis.Engine.Web.ViewModel.BranchViewModels;
using Microsoft.EntityFrameworkCore;

namespace Mardis.Engine.DataObject.MardisCore
{
    public class CampaignServicesDao : ADao
    {
        public CampaignServicesDao(MardisContext mardisContext) : base(mardisContext)
        {
        }

        public List<CampaignServices> GetCampaignServicesByCampaign(Guid idCampaign, Guid idAccount)
        {
            return Context.CampaignsServices
                .Include(cs => cs.Service)
                .Where(cs => cs.IdCampaign == idCampaign &&
                              cs.StatusRegister == CStatusRegister.Active &&
                              cs.IdAccount == idAccount)
                .ToList();
        }

        public List<CampaignServices> GetCampaignServicesByService(Guid idService, Guid idAccount)
        {
            return Context.CampaignsServices
                .Where(cs => cs.IdService == idService &&
                             cs.StatusRegister == CStatusRegister.Active &&
                             cs.IdAccount == idAccount)
                .ToList();
        }

        public async Task<IQueryable<CampaignServices>> GetCompleteCampaignServices(Guid idCampaign, Guid idAccount)
        {
            return await Task.FromResult(Context.CampaignsServices
                .Include(cs => cs.Service.ServiceDetails)
                    .ThenInclude(sd => sd.Questions)
                        .ThenInclude(q => q.TypePoll)
                .Include(cs => cs.Service.ServiceDetails)
                    .ThenInclude(sd => sd.Questions)
                        .ThenInclude(q => q.QuestionDetails)
                .Include(cs => cs.Service.ServiceDetails)
                    .ThenInclude(sd => sd.Sections)
                        .ThenInclude(s => s.Questions)
                            .ThenInclude(q => q.QuestionDetails)
                .Include(cs => cs.Service.ServiceDetails)
                    .ThenInclude(sd => sd.Sections)
                        .ThenInclude(s => s.Questions)
                            .ThenInclude(q => q.TypePoll)
                .Where(cs => cs.IdCampaign == idCampaign &&
                             cs.StatusRegister == CStatusRegister.Active &&
                             cs.IdAccount == idAccount));
        }

        #region Administracion de Rutas

        public IList<RouteBranchViewModel> GetActRoute(Guid idAccount)
        {


            IList<RouteBranchViewModel> _model = new List<RouteBranchViewModel>(); 
            //var query = Context.Branches.Where(x => x.IdAccount.Equals(idAccount)).Select(new { });
            //var query = from data in Context.Branches
            //                  .GroupBy(g => new { g.RUTAAGGREGATE, g.IdAccount })
            //                  .Where(w => w.Key.IdAccount.Equals(idAccount))
            //                 .Select(s => new { ruta = s.Key.RUTAAGGREGATE, numero = s.Key.IdAccount } );

            var query = from data in Context.Branches
                        where data.IdAccount == idAccount
                        group data by new { data.IdAccount, data.RUTAAGGREGATE } into grupo
                        select new { route = grupo.Key.RUTAAGGREGATE , numbreBranches = grupo.Count()};

            var result = query.ToList();
            foreach (var item in result)
            {
                //var result2 = from c in Context.Branches
                //             group c by new { c.IdAccount, c.RUTAAGGREGATE, c.ESTADOAGGREGATE } into grp
                //             where grp.Count() > 1 & grp.Key.ESTADOAGGREGATE=="S" & grp.Key.RUTAAGGREGATE == item.route
                //              select grp.Key;
               var active = Context.Branches.Where(x => x.IdAccount == idAccount && x.RUTAAGGREGATE == item.route && x.ESTADOAGGREGATE=="S").Select( x=>x.Id).Count();
               
                Boolean isActive = active > 0 ? true : false; 


                RouteBranchViewModel route = new RouteBranchViewModel();
                _model.Add(new RouteBranchViewModel() { route=item.route, numbreBranches=item.numbreBranches,status= isActive });

            }

            return _model;
        }

        public async Task<int> UpdateStatusRoute( Guid idAccount ,string route)
        {

            try
            {
    

                using (var transaction = Context.Database.BeginTransaction())
                {
                    var updatebranches = Context.Branches.Where(x => x.RUTAAGGREGATE.Equals(route) && x.IdAccount == idAccount).ToList();
                    var Isactive = Context.Branches.Where(x => x.IdAccount == idAccount && x.RUTAAGGREGATE == route && x.ESTADOAGGREGATE == "S").Select(x => x.Id).FirstOrDefault();
                    var estados = Isactive != Guid.Parse("00000000-0000-0000-0000-000000000000") ? "" : "S";
                    updatebranches.ForEach(a => a.ESTADOAGGREGATE = estados) ;
                    Context.Branches.UpdateRange(updatebranches);
                    Context.SaveChanges();
                    transaction.Commit();
                }

            }
            catch (Exception e)
            {
               
                e.Message.ToString();
                return 0;
            }
          

            return 1;
        }
    }
    #endregion


}