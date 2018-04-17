using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Synthesis.InProductTrainingService.InternalApi.Models;
using Synthesis.InProductTrainingService.InternalApi.Requests;
using Synthesis.InProductTrainingService.InternalApi.Responses;

namespace Synthesis.InProductTrainingService.Controllers
{
    public interface IInProductTrainingController
    {
        Task<InProductTrainingViewResponse> CreateInProductTrainingViewAsync(InProductTrainingViewRequest model, Guid userId);
        Task<IEnumerable<InProductTrainingViewResponse>> GetViewedInProductTrainingAsync(int clientApplicationId, Guid userId);
        Task<WizardViewResponse> CreateWizardViewAsync(ViewedWizard model);
        Task<WizardViewResponse> GetWizardViewsByUserIdAsync(Guid userId);
    }
}