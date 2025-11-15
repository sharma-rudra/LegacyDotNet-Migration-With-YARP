using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BasicBlog_Migrated.Models
{
    public class Comment
    {
        [Required]
        public int Id { get; set; }

        [Required]
        [Display(Name = "Comment")]
        public string CommentText { get; set; }

        [Required]
        [Display(Name = "Username")]
        public string Username { get; set; }

        [Required]
        [Display(Name = "Left at:")]
        public DateTime TimeCommented { get; set; }

        // This links to the Id in the Blogs table
        public int BlogId { get; set; }

        // This navigation property links BlogId to a Blog
        [ForeignKey("BlogId")]
        public virtual Blog Blog { get; set; }
    }
}