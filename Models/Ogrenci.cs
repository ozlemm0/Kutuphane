using System.ComponentModel.DataAnnotations;

namespace Kutuphane.Models
{
    public class Ogrenci{
        public int Id { get; set; }
        
        [Required(ErrorMessage = "Öğrenci adı zorunludur.")]
        public string OgrenciAdi { get; set; } = string.Empty;
        
        [Required(ErrorMessage = "Öğrenci soyadı zorunludur.")]
        public string OgrenciSoyadi { get; set; } = string.Empty;
        
        [Required(ErrorMessage = "Okul numarası zorunludur.")]
        public string OkulNumarasi { get; set; } = string.Empty;
        
        public int? SinifId { get; set; } 
        public Sinif? Sinif { get; set; } 

        public DateTime EklenmeTarihi { get; set; } = DateTime.Now;
    }
}