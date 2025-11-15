using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BasicBlog_Migrated.Models
{
    public class Blog
    {
        public int Id { get; set; }

        [Required]
        [StringLength(255)]
        public string Title { get; set; }

        public DateTime CreatedOn { get; set; }

        [Display(Name = "Your Blog Text")]
        [Required]
        public string BlogText { get; set; }

        // This links to the Id in the AspNetUsers table
        public string BlogOwnerId { get; set; }

        // This navigation property links BlogOwnerId to an ApplicationUser
        [ForeignKey("BlogOwnerId")]
        public ApplicationUser BlogOwner { get; set; }

        public virtual ICollection<Comment> Comments { get; set; }
    }
}