using System;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;
using Synthesis.InProductTrainingService.InternalApi.Enums;

namespace Synthesis.InProductTrainingService.InternalApi.Models
{
    [Serializable]
    [DataContract]
    public class WizardView
    {
        /// <summary>
        /// The guid of the user.
        /// </summary>
        [DataMember]
        public Guid UserId { get; set; }

        /// <summary>
        /// The number id that represents the wizard type for the user.
        /// </summary>
        [DataMember(Name = "WizardType")]
        [EnumDataType(typeof(WizardType))]
        public WizardType WizardType { get; set; }
    }
}