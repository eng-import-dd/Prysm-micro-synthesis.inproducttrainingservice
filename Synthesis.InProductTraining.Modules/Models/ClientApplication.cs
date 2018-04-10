﻿using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Synthesis.InProductTrainingService.Models
{
    public class ClientApplication
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public ClientApplication()
        {
            InProductTrainingSubjects = new HashSet<InProductTrainingSubject>();
        }

        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int ClientApplicationId { get; set; }

        [Required, StringLength(50)]
        public string Name { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public ICollection<InProductTrainingSubject> InProductTrainingSubjects { get; set; }
    }
}