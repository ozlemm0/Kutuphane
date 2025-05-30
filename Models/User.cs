using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Kutuphane.Models
{
    public class User
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Kullanıcı adı zorunludur.")]
        [StringLength(50, MinimumLength = 3, ErrorMessage = "Kullanıcı adı 3-50 karakter arasında olmalıdır.")]
        [RegularExpression(@"^[a-zA-Z0-9_]+$", ErrorMessage = "Kullanıcı adı sadece harf, rakam ve alt çizgi içerebilir.")]
        public string Username { get; set; }

        [Required(ErrorMessage = "E-posta adresi zorunludur.")]
        [StringLength(100)]
        [EmailAddress(ErrorMessage = "Geçerli bir e-posta adresi giriniz.")]
        public string Email { get; set; }

        [Required]
        [NotMapped]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "Şifre en az 6 karakter uzunluğunda olmalıdır.")]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        public string? PasswordHash { get; set; }

        public bool IsAdmin { get; set; } = false;

        [StringLength(100, ErrorMessage = "Ad 100 karakterden uzun olamaz.")]
        public string? Name { get; set; }

        [StringLength(100, ErrorMessage = "Soyad 100 karakterden uzun olamaz.")]
        public string? Surname { get; set; }
    }
} 