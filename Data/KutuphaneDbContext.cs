using Kutuphane.Models;
using Kutuphane.Data;
using Kutuphane.Controllers;
using Microsoft.EntityFrameworkCore;

namespace Kutuphane.Data
{
    public class KutuphaneDbContext : DbContext
    {
        public KutuphaneDbContext(DbContextOptions<KutuphaneDbContext> options) : base(options)
        {
        }

        public DbSet<Ogrenci> Ogrenciler { get; set; }
        public DbSet<Sinif> Siniflar { get; set; }
        public DbSet<Kategori> Kategoriler { get; set; }
        public DbSet<Kitap> Kitaplar { get; set; }
        public DbSet<OduncKitap> OduncKitaplar { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
        }
    }
}