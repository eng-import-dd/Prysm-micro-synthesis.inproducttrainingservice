using System;
using System.Threading.Tasks;
using Synthesis.Http.Microservice;
using Synthesis.InProductTrainingService.InternalApi.Models;

namespace Synthesis.InProductTrainingService.InternalApi.Api
{
    public interface IInProductTrainingApi
    {
        Task<MicroserviceResponse<InProductTraining>> GetInProductTrainingById(Guid id);
    }
}
