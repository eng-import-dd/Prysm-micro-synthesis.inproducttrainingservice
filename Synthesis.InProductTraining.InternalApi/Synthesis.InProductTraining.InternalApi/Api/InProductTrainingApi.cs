using System;
using System.Threading.Tasks;
using Synthesis.Configuration;
using Synthesis.Http.Microservice;
using Synthesis.Http.Microservice.Api;
using Synthesis.InProductTrainingService.InternalApi.Models;

namespace Synthesis.InProductTrainingService.InternalApi.Api
{
    public class InProductTrainingApi : MicroserviceApi, IInProductTrainingApi
    {
        private const string InProductTrainingRelativePath = "/v1/InProductTraining";

        public InProductTrainingApi(IMicroserviceHttpClientResolver microserviceHttpClientResolver, IAppSettingsReader appSettingsReader)
            : base(microserviceHttpClientResolver, appSettingsReader, "InProductTraining.Url")
        {
        }

        protected string InProductTrainingBaseRoute => BuildRoute(InProductTrainingRelativePath);

        /// <inheritdoc/>
        public async Task<MicroserviceResponse<InProductTraining>> GetInProductTrainingById(Guid id)
        {
            return await HttpClient.GetAsync<InProductTraining>(BuildInProductTrainingRoute($"{id}"));
        }

        protected string BuildInProductTrainingRoute(string path)
        {
            return BuildRoute($"{InProductTrainingRelativePath}/{path}");
        }
    }
}
