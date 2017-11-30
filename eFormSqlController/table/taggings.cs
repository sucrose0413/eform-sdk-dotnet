﻿namespace eFormSqlController
{
    using System;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    public partial class taggings
    {
        [Key]
        public int id { get; set; }

        [ForeignKey("tag")]
        public int? tag_id { get; set; }

        [ForeignKey("check_list")]
        public int? check_list_id { get; set; }

        public int? tagger_id { get; set; } // this will refer to some user id.

        public int? version { get; set; }

        [StringLength(255)]
        public string workflow_state { get; set; }

        public DateTime? created_at { get; set; }

        public DateTime? updated_at { get; set; }

        public virtual tags tag { get; set; }

        public virtual check_lists check_list { get; set; }
    }
}
