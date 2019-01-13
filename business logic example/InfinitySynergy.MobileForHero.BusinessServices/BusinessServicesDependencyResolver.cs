using Ninject;
using Ninject.Modules;

using InfinitySynerge.MobileForHero.BusinessServices.Common;
using InfiniteSynergy.MobileForHero.BusinessServices.Product;
using InfinitySynerge.MobileForHero.BusinessServices.BusinesProcess;
using InfinitySynerge.MobileForHero.BusinessServices.DataManagment;

using InfiniteSynergy.MobileForHero.BusinessServices.Contracts.Common;
using InfiniteSynergy.MobileForHero.BusinessServices.Contracts.DataManagment;
using InfiniteSynergy.MobileForHero.BusinessServices.Contracts.ProductCatalog;
using InfiniteSynergy.MobileForHero.BusinessServices.Contracts.BusinessProcess;
using InfinitySynerge.MobileForHero.BusinessServices.Billing;
using InfiniteSynergy.MobileForHero.BusinessServices.Contracts.Communication;
using InfinitySynerge.MobileForHero.BusinessServices.Communication;

namespace InfinitySynerge.MobileForHero.BusinessServices
{
    public class BusinessServicesNInjectModule : NinjectModule
    {
        public override void Load()
        {
            Bind<IOrderService>().To<OrderService>();
            Bind<IDiscountCouponeService>().To<DiscountCouponeService>();

            Bind<ISystemSettingsService>().To<SystemSettingsService>();

            Bind<IBlankService>().To<BlankService>();
            Bind<IBlankBatchService>().To<BlankBatchService>();
            Bind<IBlankBatchParametersService>().To<BlankBatchParametersService>();

            Bind<ILookupService>().To<LookupService>();
            Bind<IPhoneModelService>().To<PhoneModelService>();
            Bind<IColorSchemeService>().To<ColorSchemeService>();
            Bind<IPhoneManufacturerService>().To<PhoneManufacturerService>();
            Bind<IPhoneModelTemplateService>().To<PhoneModelTemplateService>();
            Bind<IPhoneModelColoredBlankService>().To<PhoneModelColoredBlankService>();

            Bind<IProductCatalogService>().To<ProductCatalogService>();

            Bind<IColorService>().To<ColorService>();

            Bind<IBillingService>().To<TinkoffBillingService>();
            Bind<ICaseRenderingService>().To<CaseRenderingService>();

            Bind<IEmailOrchestrationService>().To<EmailOrchestrationService>();
            Bind<IEmailTemplateService>().To<EmailTemplateService>();
        }
    }

    public class BusinessServicesDependencyResolver
    {
        public static IKernel Kernel { get; private set; } = new StandardKernel(new BusinessServicesNInjectModule());

        public static TObject Get<TObject>() => (TObject)Kernel.Get(typeof(TObject));
    }
}
