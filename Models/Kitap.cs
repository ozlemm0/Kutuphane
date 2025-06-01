using System.ComponentModel.DataAnnotations;

namespace Kutuphane.Models
{
    public class Kitap
    {
        public Kitap()
        {
            OduncKitaplar = new List<OduncKitap>();
        }

        public int Id { get; set; }

        [Required(ErrorMessage = "Kitap adı zorunludur.")]
        public string KitapAdi { get; set; } = string.Empty;

        [Required(ErrorMessage = "Yazar adı zorunludur.")]
        public string Yazar { get; set; } = string.Empty;

        [Required(ErrorMessage = "Yayınevi zorunludur.")]
        public string Yayinevi { get; set; } = string.Empty;

        [Required(ErrorMessage = "ISBN numarası zorunludur.")]
        public string ISBN { get; set; } = string.Empty;

        [Required(ErrorMessage = "Kategori seçimi zorunludur.")]
        public int KategoriId { get; set; }
        public Kategori? Kategori { get; set; }

        public bool OduncVerildiMi { get; set; } = false;
        public DateTime EklenmeTarihi { get; set; } = DateTime.Now;

        public ICollection<OduncKitap> OduncKitaplar { get; set; }
        public bool AktifMi { get; set; } = true;
    }
} 