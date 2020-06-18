using Autofac;
using Nancy;
using Synthesis.InProductTrainingService.Controllers;
using Synthesis.InProductTrainingService.Modules;
using Synthesis.Nancy.Autofac.Module.Configuration;
using Synthesis.Nancy.Autofac.Module.Microservice;
using Synthesis.PrincipalService.InternalApi.Api;
using Module = Autofac.Module;

namespace Synthesis.InProductTrainingService
{
    public class InProductTrainingAutofacModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterModule<ConfigurationAutofacModule>();
            builder.RegisterModule(
                new MicroserviceAutofacModule(ServiceInformation.ServiceName,
                    ServiceInformation.ServiceNameShort,
                    GetType()));

            // Apis
            builder.RegisterType<UserApi>().As<IUserApi>();

            // Controllers
            builder.RegisterType<InProductTrainingController>().As<IInProductTrainingController>();

            // Nancy Module
            builder.RegisterType<InProductTrainingModule>().As<INancyModule>();
        }
    }
}
