using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Kutuphane.Models
{
    public class OduncKitap
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int KitapId { get; set; } 

        public int? OgrenciId { get; set; }

        [Required]
        public DateTime OduncAlmaTarihi { get; set; }

        public DateTime? TeslimTarihi { get; set; }

        public bool TeslimDurumu { get; set; }

        [ForeignKey("KitapId")]
        public Kitap? Kitap { get; set; }

        [ForeignKey("OgrenciId")]
        public Ogrenci? Ogrenci { get; set; }
    }
} 