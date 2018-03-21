using Microsoft.Hadoop.Avro;
using Newtonsoft.Json;
using Synthesis.Serialization;
using System;
using System.Runtime.Serialization;

namespace Synthesis.InProductTrainingService.InternalApi.Models
{
    // NOTE: All model classes should be decorated with DataContract.    
    // Use DataMember, ApiMember and DbMember to handle serialization approriately.
    // DataMember should be used if you want the member to be serialized in the event bus.
    // If ApiMember and DbMember are not explicitly set on each member, then the member WILL NOT be serialized in the appropriate scenarios.
    // NullableSchema should be used for custom class members or if it's a nullable type
    // JsonProperty can be used to change how the member name gets serialized

    [DataContract]
    public class InProductTraining
    {
        [JsonProperty("id")]
        [DataMember, ApiMember, DbMember]
        public Guid? Id { get; set; }

        [DataMember, ApiMember, DbMember]
        public Guid ProjectId { get; set; }

        [DataMember, ApiMember, DbMember]
        public string Name { get; set; }

        [DataMember, DbMember]
        [NullableSchema]
        public DateTime? CreatedDate { get; set; }

        [DataMember, DbMember]
        [NullableSchema]
        public DateTime? LastAccessDate { get; set; }

        /// <summary>
        /// Anything nullable that won't be provided by a new default object needs to be supplied so anyone looking at the /docs page can clearly see what kind of object each property is.
        /// </summary>
        /// <returns>A InProductTraining object.</returns>
        public static InProductTraining Example()
        {
            return new InProductTraining
            {
                Id = Guid.NewGuid(),
                Name = "Name for resource",
                CreatedDate = DateTime.UtcNow,
                LastAccessDate = DateTime.UtcNow
            };
        }
    }
}
