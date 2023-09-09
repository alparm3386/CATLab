﻿using CAT.Areas.Identity.Data;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CAT.Models.Entities.Main
{
    [Table("Linguists")]
    public class Linguist
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [StringLength(50)]
        public string UserId { get; set; } = default!;

        [NotMapped]
        public ApplicationUser User { get; set; } = default!;

        public LinguistLanguagePair LinguistsLanguagePairs { get; set; } = default!;
    }
}
