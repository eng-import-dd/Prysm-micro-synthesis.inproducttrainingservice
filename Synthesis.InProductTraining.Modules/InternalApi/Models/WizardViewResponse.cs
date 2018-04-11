using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;
using Synthesis.InProductTrainingService.InternalApi.Enums;

namespace Synthesis.InProductTrainingService.InternalApi.Models
{
    [Serializable]
    [DataContract]
    public class WizardViewResponse
    {
        /// <summary>
        /// The list of WizardViews associated with the response
        /// </summary>
        [DataMember]
        public List<WizardView> WizardViews { get; set; }

        /// <summary>
        /// The result code for the status of the get/create transaction
        /// </summary>
        /// 
        [DataMember(Name = "ResultCode")]
        [EnumDataType(typeof(ResultCode))]
        public ResultCode ResultCode { get; set; }

        /// <summary>
        /// The message associated with the ResultCode
        /// </summary>
        [DataMember]
        public string ResultMessage { get; set; }
    }
}