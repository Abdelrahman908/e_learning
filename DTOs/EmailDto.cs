using System.ComponentModel.DataAnnotations;

namespace e_learning.DTOs
{
    public class EmailDto
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }
    }


}
