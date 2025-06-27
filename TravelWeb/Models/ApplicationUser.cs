using Microsoft.AspNetCore.Identity;
using System;

namespace TravelWeb.Models
{
    public class ApplicationUser : IdentityUser
    {
        // Họ và tên đầy đủ
        public string? FullName { get; set; }

        // Giới tính: Nam / Nữ / Khác
        public string? Gender { get; set; }

        // Ngày sinh
        public DateTime? DateOfBirth { get; set; }

        // Địa chỉ
        public string? Address { get; set; }

        // Quốc tịch
        public string? Nationality { get; set; }

        // Trạng thái tài khoản: đang hoạt động, bị khóa, v.v.
        public bool IsActive { get; set; } = true;

        // Ngày tạo tài khoản
        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}
