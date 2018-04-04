namespace Synthesis.InProductTrainingService.Constants
{
    public class ResponseReasons
    {
        // Internal server errors
        public const string InternalServerErrorCreateInProductTraining = "An error occurred while creating the InProductTraining";

        public const string InternalServerErrorDeleteInProductTraining = "An error occurred deleting the InProductTraining";
        public const string InternalServerErrorGetInProductTraining = "An error occurred retrieving the InProductTraining";
        public const string InternalServerErrorUpdateInProductTraining = "An error occurred updating the InProductTraining";

        // Not found
        public const string NotFoundInProductTraining = "InProductTraining Not Found";
        public const string NotFoundUserIdForInProductTraining = "UserId Not Found";
    }
}
