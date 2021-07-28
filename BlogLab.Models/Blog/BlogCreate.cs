using System.ComponentModel.DataAnnotations;

namespace BlogLab.Models.Blog
{
    public class BlogCreate
    {
        public int BlogId { get; set; }
        
        [Required(ErrorMessage = "Title is required")]
        [MinLength(10, ErrorMessage = "Must be at least 10 characters")]        
        [MaxLength(50, ErrorMessage = "Cannot exceed 50 characters")]
        public string Title { get; set; }
        
        [Required(ErrorMessage = "Content is required")]
        [MinLength(20, ErrorMessage = "Must be at least 20 characters")]
        public string Content { get; set; }

        public int? PhotoId { get; set; }
    }
}