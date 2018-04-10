using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Synthesis.InProductTrainingService.Models
{
    public class UserType
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public UserType()
        {
            InProductTrainingViews = new HashSet<InProductTrainingView>();
        }

        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int UserTypeID { get; set; }

        [Required, StringLength(50)]
        public string Name { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public ICollection<InProductTrainingView> InProductTrainingViews { get; set; }
    }
}