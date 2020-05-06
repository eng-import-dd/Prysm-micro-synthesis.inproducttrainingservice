using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Synthesis.InProductTrainingService.InternalApi.Enums;
using Synthesis.InProductTrainingService.InternalApi.Models;
using Synthesis.InProductTrainingService.InternalApi.Responses;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace Synthesis.InProductTrainingService.Data
{
    public class InProductTrainingSqlService
    {
        private IConfiguration _configuration;

        public InProductTrainingSqlService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public InProductTrainingViewResponse CreateInProductTrainingView(int inProductTrainingSubjectId, Guid userId, string title, int userTypeId, string createdByUserName, ref CreateInProductTrainingViewReturnCode returnCode)
        {
            if (string.IsNullOrWhiteSpace(title))
            {
                throw new ArgumentNullException(title, "Argument cannot be null, empty, or whitespace.");
            }
            if (string.IsNullOrWhiteSpace(createdByUserName))
            {
                throw new ArgumentNullException(createdByUserName, "Argument cannot be null, empty, or whitespace.");
            }

            SqlParameter resultCodeOutputParam;
            InProductTrainingViewResponse result;

            using (var sdc = new SynthesisDataContext(new DbContextOptionsBuilder<SynthesisDataContext>().Options, _configuration))
            {
                var inProductTrainingSubjectIdParam = new SqlParameter("@InProductTrainingSubjectId", inProductTrainingSubjectId);
                var userIdParam = new SqlParameter("@UserId", userId);
                var titleParam = new SqlParameter("@Title", title);
                var userTypeParam = new SqlParameter("@UserTypeId", userTypeId);
                var createUserParam = new SqlParameter("@CreateUser", createdByUserName);
                resultCodeOutputParam = new SqlParameter("@ResultCode", SqlDbType.Int) { Direction = ParameterDirection.Output };

                result = sdc.InProductTrainingViewResponses.FromSqlRaw("InsertInProductTrainingViews @InProductTrainingSubjectId, @UserID, @Title, @UserTypeId, @CreateUser, @ResultCode OUTPUT", inProductTrainingSubjectIdParam, userIdParam, titleParam, userTypeParam, createUserParam, resultCodeOutputParam).FirstOrDefault();
            }
            
            if (!Enum.TryParse(resultCodeOutputParam.Value.ToString(), out returnCode))
            {
                returnCode = CreateInProductTrainingViewReturnCode.CreateFailed;
            }
            return result;
        }

        public async Task<List<InProductTrainingViewResponse>> GetInProductTrainingViewsAsync(int clientApplicationId, Guid userId)
        {
            List<InProductTrainingViewResponse> trainingViews;

            using (var sdc = new SynthesisDataContext(new DbContextOptionsBuilder<SynthesisDataContext>().Options, _configuration))
            {
                trainingViews = await sdc.InProductTrainingViews
                    .Include(s => s.InProductTrainingSubject)
                    .Where(t => t.UserId == userId && t.InProductTrainingSubject.ClientApplicationId == clientApplicationId)
                    .Select(v => new InProductTrainingViewResponse
                    {
                        InProductTrainingSubjectId = v.InProductTrainingSubjectId,
                        UserId = v.UserId,
                        Title = v.Title,
                        ClientApplicationId = v.InProductTrainingSubject.ClientApplicationId,
                        TrainingMethod = v.InProductTrainingSubject.TrainingMethod,
                        UserTypeId = v.UserTypeId
                    })
                    .ToListAsync();
            }

            return trainingViews;
        }

        public async Task<List<ViewedWizard>> GetViewedWizardsAsync(Guid userId)
        {
            using (var sdc = new SynthesisDataContext(new DbContextOptionsBuilder<SynthesisDataContext>().Options, _configuration))
            {
                return await sdc.ViewedWizards.Where(w => w.UserId == userId).ToListAsync();
            }
        }

        public async Task<ViewedWizard> CreateViewedWizardAsync(ViewedWizard wizardView)
        {
            using (var sdc = new SynthesisDataContext(new DbContextOptionsBuilder<SynthesisDataContext>().Options, _configuration))
            {
                sdc.ViewedWizards.Add(wizardView);
                await sdc.SaveChangesAsync();

                return await sdc.ViewedWizards.SingleAsync(x => x.UserId == wizardView.UserId && x.WizardType == wizardView.WizardType);
            }
        }
    }
}