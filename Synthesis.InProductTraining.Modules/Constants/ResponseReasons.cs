namespace Synthesis.InProductTrainingService.Constants
{
    public class ResponseReasons
    {
        // Internal server errors
        public const string InternalServerErrorCreateInProductTrainingView = "An error occurred while creating the InProductTraining";
        public const string InternalServerErrorCreateWizardView = "An error occurred while creating the ViewedWizard";

        public const string InternalServerErrorDeleteInProductTrainingViews = "An error occurred deleting the InProductTraining";
        public const string InternalServerErrorDeleteWizardViews = "An error occurred deleting the ViewedWizard";

        public const string InternalServerErrorGetInProductTrainingViews = "An error occurred retrieving the InProductTraining";
        public const string InternalServerErrorGetWizardViews = "An error occurred retrieving the ViewedWizard";

        public const string InternalServerErrorUpdateInProductTrainingViews = "An error occurred updating the InProductTraining";
        public const string InternalServerErrorUpdateWizardViews = "An error occurred updating the ViewedWizard";

        // Not found
        public const string NotFoundInProductTrainingViews = "InProductTraining Not Found";
        public const string NotFoundUserIdForInProductTrainingViews = "UserId Not Found";
        public const string NotFoundWizardViews = "ViewedWizard Not Found";
    }
}
