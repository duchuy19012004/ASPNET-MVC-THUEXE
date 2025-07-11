using Microsoft.EntityFrameworkCore;
using bike.Models;

namespace bike.Repository
{
    // DbContext là class quản lý kết nối và tương tác với database
    public class BikeDbContext : DbContext
    {
        // Constructor nhận vào options để cấu hình database
        public BikeDbContext(DbContextOptions<BikeDbContext> options)
            : base(options)
        {
        }
        // DbSet đại diện cho bảng Xe trong database
        // Tên DbSet sẽ là tên bảng trong SQL Server
        public DbSet<Xe> Xe { get; set; }
        public DbSet<LoaiXe> LoaiXe { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<DatCho> DatCho { get; set; }
        public DbSet<HopDong> HopDong { get; set; }
        public DbSet<ChiTieu> ChiTieu { get; set; }
        public DbSet<HoaDon> HoaDon { get; set; }
        public DbSet<Banner> Banner { get; set; }
        public DbSet<ChiTietHopDong> ChiTietHopDong { get; set; }
        public DbSet<BaoCaoThietHai> BaoCaoThietHai { get; set; }
        public DbSet<HinhAnhXe> HinhAnhXe { get; set; }

        // cấu hình thêm cho database (nếu cần)
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<ChiTieu>(e => e.Property(p => p.SoTien).HasColumnType("decimal(18, 2)"));
            // Cấu hình HopDong
            modelBuilder.Entity<HopDong>(e =>
            {
                e.Property(p => p.PhuPhi).HasColumnType("decimal(18, 2)");
                e.Property(p => p.TienCoc).HasColumnType("decimal(18, 2)");
                e.Property(p => p.TongTien).HasColumnType("decimal(18, 2)");
                
                // Cấu hình quan hệ với User - dùng NoAction để tránh cascade conflicts
                e.HasOne(h => h.KhachHang)
                 .WithMany(u => u.HopDongKhachHang)
                 .HasForeignKey(h => h.MaKhachHang)
                 .OnDelete(DeleteBehavior.NoAction);
                 
                e.HasOne(h => h.NguoiTao)
                 .WithMany(u => u.HopDongNguoiTao)
                 .HasForeignKey(h => h.MaNguoiTao)
                 .OnDelete(DeleteBehavior.NoAction);
            });
            
            // Cấu hình ChiTietHopDong
            modelBuilder.Entity<ChiTietHopDong>(e =>
            {
                e.Property(p => p.GiaThueNgay).HasColumnType("decimal(18, 2)");
                e.Property(p => p.ThanhTien).HasColumnType("decimal(18, 2)");
                e.Property(p => p.PhiDenBu).HasColumnType("decimal(18, 2)");
                
                // Cấu hình quan hệ
                e.HasOne(ct => ct.HopDong)
                 .WithMany(h => h.ChiTietHopDong)
                 .HasForeignKey(ct => ct.MaHopDong)
                 .OnDelete(DeleteBehavior.Cascade);
                 
                e.HasOne(ct => ct.Xe)
                 .WithMany(x => x.ChiTietHopDong)
                 .HasForeignKey(ct => ct.MaXe)
                 .OnDelete(DeleteBehavior.NoAction);
            });
            // Cấu hình Xe
            modelBuilder.Entity<Xe>(e => 
            {
                e.Property(p => p.GiaThue).HasColumnType("decimal(18, 2)");
                e.Property(p => p.GiaTriXe).HasColumnType("decimal(18, 2)");
                e.Property(p => p.ChiPhiSuaChua).HasColumnType("decimal(18, 2)");
            });
            // Cấu hình cho HoaDon
            modelBuilder.Entity<HoaDon>(e => {
                e.Property(p => p.SoTien).HasColumnType("decimal(18, 2)");
                
                // Cấu hình quan hệ 1-1 với HopDong
                e.HasOne(h => h.HopDong)
                 .WithOne(hd => hd.HoaDon)
                 .HasForeignKey<HoaDon>(h => h.MaHopDong)
                 .OnDelete(DeleteBehavior.Cascade);
                 
                // Cấu hình quan hệ với User (NguoiTao) - ngăn cascade conflict
                e.HasOne(h => h.NguoiTao)
                 .WithMany()
                 .HasForeignKey(h => h.MaNguoiTao)
                 .OnDelete(DeleteBehavior.NoAction);
            });

            // Cấu hình BaoCaoThietHai
            modelBuilder.Entity<BaoCaoThietHai>(e =>
            {
                e.Property(p => p.ChiPhiSuaChuaUocTinh).HasColumnType("decimal(18, 2)");
                e.Property(p => p.ChiPhiSuaChuaThucTe).HasColumnType("decimal(18, 2)");
                e.Property(p => p.PhiDenBuKhachHang).HasColumnType("decimal(18, 2)");
                e.Property(p => p.GiaTriXeTruocKhiHong).HasColumnType("decimal(18, 2)");
                e.Property(p => p.GiaTriXeSauKhiHong).HasColumnType("decimal(18, 2)");
                
                // Cấu hình quan hệ với ChiTietHopDong
                e.HasOne(b => b.ChiTietHopDong)
                 .WithMany()
                 .HasForeignKey(b => b.MaChiTiet)
                 .OnDelete(DeleteBehavior.Cascade);
                 
                // Cấu hình quan hệ với User (NguoiTao)
                e.HasOne(b => b.NguoiTao)
                 .WithMany()
                 .HasForeignKey(b => b.MaNguoiTao)
                 .OnDelete(DeleteBehavior.NoAction);
            });

            // Cấu hình HinhAnhXe
            modelBuilder.Entity<HinhAnhXe>(e =>
            {
                // Cấu hình quan hệ với Xe
                e.HasOne(h => h.Xe)
                 .WithMany(x => x.HinhAnhXes)
                 .HasForeignKey(h => h.MaXe)
                 .OnDelete(DeleteBehavior.Cascade);
                 
                // Cấu hình index cho trường MaXe và ThuTu
                e.HasIndex(h => new { h.MaXe, h.ThuTu }).IsUnique(false);
                
                // Cấu hình index cho trường LaAnhChinh
                e.HasIndex(h => new { h.MaXe, h.LaAnhChinh }).IsUnique(false);
            });
        }
    }
}