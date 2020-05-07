using System.Reflection;
using Autofac;
using Synthesis.InProductTrainingService.Controllers;
using Synthesis.Nancy.Autofac.Module.Microservice;
using Synthesis.PrincipalService.InternalApi.Api;
using Module = Autofac.Module;

namespace Synthesis.InProductTrainingService
{
    public class InProductTrainingAutofacModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterModule(new MicroserviceAutofacModule(ServiceInformation.ServiceName,
                ServiceInformation.ServiceNameShort,
                Assembly.GetAssembly(GetType()),
                false));

            // Apis
            builder.RegisterType<UserApi>().As<IUserApi>();

            // Controllers
            builder.RegisterType<InProductTrainingController>().As<IInProductTrainingController>();
        }
    }
}
