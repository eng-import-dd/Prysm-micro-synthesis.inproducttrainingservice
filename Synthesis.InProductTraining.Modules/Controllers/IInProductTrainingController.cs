using Synthesis.InProductTrainingService.Models;
using System;
using System.Threading.Tasks;

namespace Synthesis.InProductTrainingService.Controllers
{
    public interface IInProductTrainingController
    {
        Task<InProductTraining> CreateInProductTrainingAsync(InProductTraining model);

        Task<InProductTraining> GetInProductTrainingAsync(Guid inProductTrainingId);

        Task<InProductTraining> UpdateInProductTrainingAsync(Guid inProductTrainingId, InProductTraining model);

        Task DeleteInProductTrainingAsync(Guid inProductTrainingId);
    }
}
