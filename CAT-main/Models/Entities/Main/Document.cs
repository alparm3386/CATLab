﻿using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CAT.Models.Entities.Main
{
    [Table("Documents")]
    public class Document
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [System.Diagnostics.CodeAnalysis.MaybeNull]
        public string OriginalFileName { get; set; }

        [System.Diagnostics.CodeAnalysis.MaybeNull]
        public string FileName { get; set; }

        public int FilterId { get; set; }

        public int AnalisysId { get; set; }

    }
}
