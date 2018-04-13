using System;

namespace Synthesis.InProductTrainingService.Resolvers
{
    public static class KeyResolver
    {
        public static string InProductTrainingViews(Guid userId, int clientApplicationId) { return $"InProductTrainingViewedByUser:{userId}:{clientApplicationId}"; }
        public static string ViewedWizards(Guid userId) { return $"ViewedWizardsForUser:{userId}"; }
    }
}