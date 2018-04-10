using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Synthesis.InProductTrainingService.Models
{
    public class InProductTrainingSubject
    {
        /// <summary>
        /// The identifier of the in-product training subject that was viewed.
        /// </summary>
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int InProductTrainingSubjectId { get; set; }

        /// <summary>
        /// The identifier of the client application within which the training was viewed.
        /// </summary>
        /// <remarks>
        /// This property is denormalized.
        /// </remarks>
        [Required]
        public int ClientApplicationId { get; set; }

        /// <summary>
        /// The specific method utilized in the InProductTraining
        /// </summary>
        [Required, StringLength(128)]
        public string TrainingMethod { get; set; }

        /// <summary>
        /// The InProductTrainingSubject 
        /// </summary>
        [Required, StringLength(256)]
        public string Subject { get; set; }

        public ClientApplication ClientApplication { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public ICollection<InProductTrainingView> InProductTrainingViews { get; set; }

        /// <summary>
        /// Anything nullable that won't be provided by a new default object needs to be supplied so anyone looking
        /// at the /docs page can clearly see what kind of object each property is.
        /// </summary>
        /// <returns>An InProductTrainingSubject object.</returns>
        public static InProductTrainingSubject Example()
        {
            return new InProductTrainingSubject
            {
                InProductTrainingSubjectId = 0,
                ClientApplicationId = 0,
                TrainingMethod = "ExampleTrainingMethod",
                Subject = "ExampleSubject"
            };
        }
    }
}