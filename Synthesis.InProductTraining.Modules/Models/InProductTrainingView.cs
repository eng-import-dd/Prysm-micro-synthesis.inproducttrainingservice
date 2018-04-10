using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Synthesis.InProductTrainingService.Models
{
    public class InProductTrainingView
    {
        /// <summary>
        /// The identifier of the in-product training subject that was viewed.
        /// </summary>
        [Key, Column(Order = 0), DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int InProductTrainingSubjectId { get; set; }

        /// <summary>
        /// The Id of the SynthesisUser to which the view record is related 
        /// </summary>
        [Key, Column(Order = 1)]
        public Guid UserId { get; set; }

        /// <summary>
        /// The title of the viewed in-product training, which provides a more detailed description of the training content than the subject. 
        /// Over time, there can be multiple different training titles with separate content tied to the same subject. For example, an update 
        /// to the feature set of a particular subject, such as projects, workspaces, mirroring, or collaboration, can trigger a new 
        /// training title and associated content to be released.
        /// </summary>
        [Required, StringLength(256)]
        public string Title { get; set; }

        /// <summary>
        /// The date that the view record was created
        /// </summary>
        public DateTime CreateDate { get; set; }

        /// <summary>
        /// The user who created the view
        /// </summary>
        [StringLength(128)]
        public string CreateUser { get; set; }

        /// <summary>
        /// The user type, such as Licensed or Guest, the user viewed the campaign as
        /// </summary>
        public int UserTypeId { get; set; }

        /// <summary>
        /// The subject associated with the InProductTrainingView
        /// </summary>
        public InProductTrainingSubject InProductTrainingSubject { get; set; }

        public UserType UserType { get; set; }

        /// <summary>
        /// Anything nullable that won't be provided by a new default object needs to be supplied so anyone looking
        /// at the /docs page can clearly see what kind of object each property is.
        /// </summary>
        /// <returns>An InProductTrainingView object.</returns>
        public static InProductTrainingView Example()
        {
            return new InProductTrainingView
            {
                InProductTrainingSubjectId = 0,
                UserId = Guid.Empty,
                Title = "Example InProductTrainingView Title",
                CreateDate = DateTime.UtcNow,
                CreateUser = "ExampleUser",
                UserTypeId = 0
            };
        }
    }
}