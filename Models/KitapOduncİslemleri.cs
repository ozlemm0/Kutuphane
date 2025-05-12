using System.ComponentModel.DataAnnotations;

namespace Kutuphane.Models
{
    public class KitapOduncİslemleri
    {
    public int Id { get; set; }
    public string OgrenciAdi { get; set; }
    public string KitapAdi { get; set; }
    public DateTime AlinmaTarihi { get; set; }
    public DateTime TeslimTarihi { get; set; } // Planlanan teslim tarihi
    public DateTime? GercekTeslimTarihi { get; set; } // Null olabilir
     public OduncKitap OduncKitap {get;set;}
    }
} 