using CuttingEdge.Conditions;
using InfiniteSynergy.MobileForHero.BusinessServices.Contracts.BusinessProcess;
using InfiniteSynergy.MobileForHero.BusinessServices.Models.Business;
using InfiniteSynergy.MobileForHero.DataAccess.Contracts.Repository.Business;
using InfinitySynergy.Utility.Patterns;
using InfinitySynergy.Utility.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InfinitySynerge.MobileForHero.BusinessServices.BusinesProcess
{
    public class OrderStatusesService : IOrderStatusesService
    {
        IOrderStatusesRepository _orderStatusesRepository;

        public OrderStatusesService(IOrderStatusesRepository orderStatusesRepository)
        {
            Condition.Requires(orderStatusesRepository, nameof(orderStatusesRepository)).IsNotNull();

            _orderStatusesRepository = orderStatusesRepository;

        }

        public async Task<MethodResult<IEnumerable<OrderStatusBL>>> ReadAsync()
        { 
            return await SafeContext.ExecuteWithResultAsync(
                async () => await _orderStatusesRepository.ReadAsync()
                );
        }
    }
}
