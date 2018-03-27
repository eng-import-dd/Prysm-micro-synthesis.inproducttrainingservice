using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using Synthesis.InProductTrainingService.InternalApi.Enums;
using Synthesis.InProductTrainingService.InternalApi.Responses;

namespace Synthesis.InProductTrainingService.Data
{
    public class InProductTrainingSqlService
    {
        public InProductTrainingViewResponse CreateInProductTrainingView(int inProductTrainingSubjectId, Guid userId, string title, int userTypeId, string createdByUserName)
        {
            try
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

                using (var dc = new SynthesisDataContext())
                {
                    var inProductTrainingSubjectIdParam = new SqlParameter("@InProductTrainingSubjectId", inProductTrainingSubjectId);
                    var userIdParam = new SqlParameter("@UserId", userId);
                    var titleParam = new SqlParameter("@Title", title);
                    var userTypeParam = new SqlParameter("@UserTypeId", userTypeId);
                    var createUserParam = new SqlParameter("@CreateUser", createdByUserName);
                    resultCodeOutputParam = new SqlParameter("@ResultCode", SqlDbType.Int) { Direction = ParameterDirection.Output };

                    result = dc.Database.SqlQuery<InProductTrainingViewResponse>("InsertInProductTrainingViews @InProductTrainingSubjectId, @UserID, @Title, @UserTypeId, @CreateUser, @ResultCode OUTPUT", inProductTrainingSubjectIdParam, userIdParam, titleParam, userTypeParam, createUserParam, resultCodeOutputParam).FirstOrDefault();
                }

                if (result != null && !Enum.TryParse(resultCodeOutputParam.Value.ToString(), out CreateInProductTrainingViewReturnCode returnCode))
                {
                    result.ReturnCode = CreateInProductTrainingViewReturnCode.CreateFailed;
                }

                return result;
            }
            catch (Exception ex)
            {
                // TODO: implement logging here
                //LogError(ex);
                throw ex;

            }
        }

        public async Task<List<InProductTrainingViewResponse>> GetInProductTrainingViewsAsync(Guid userId, int clientApplicationId)
        {
            List<InProductTrainingViewResponse> trainingViews;
            try
            {
                using (var sdc = new SynthesisDataContext())
                {
                    trainingViews = await sdc.InProductTrainingViews
                        .Include(s => s.InProductTrainingSubject)
                        .Where(t => t.UserID == userId && t.InProductTrainingSubject.ClientApplicationID == clientApplicationId)
                        .Select(v => new InProductTrainingViewResponse
                        {
                            InProductTrainingSubjectId = v.InProductTrainingSubjectID,
                            UserId = v.UserID,
                            Title = v.Title,
                            ClientApplicationId = v.InProductTrainingSubject.ClientApplicationID,
                            TrainingMethod = v.InProductTrainingSubject.TrainingMethod
                        })
                        .ToListAsync();
                }
            }
            catch (Exception ex)
            {
                // TODO: implement logging here
                //LogError(ex);
                throw ex;
            }

            return trainingViews;
        }
    }
}