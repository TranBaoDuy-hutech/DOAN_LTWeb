using System.ComponentModel.DataAnnotations;

namespace TravelWeb.Models
{
    public class Guides
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập tên hướng dẫn viên")]
        public string Name { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập số điện thoại")]
        public string Phone { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập email")]
        [EmailAddress(ErrorMessage = "Email không hợp lệ")]
        [Display(Name = "Email")]
        public string Email { get; set; }

        public bool IsActive { get; set; }

        public string Skills { get; set; }
     
    }
}
