using Synthesis.InProductTrainingService.Models;
using System;
using System.Threading.Tasks;

namespace Synthesis.InProductTrainingService.Controllers
{
    public interface IInProductTrainingController
    {
        Task<InProductTraining> CreateInProductTrainingViewAsync(InProductTraining model);
        Task<InProductTraining> GetViewedInProductTrainingAsync(Guid inProductTrainingId);
    }
}