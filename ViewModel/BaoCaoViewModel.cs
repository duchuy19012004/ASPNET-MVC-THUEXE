using System;
using System.Collections.Generic;

namespace bike.ViewModels
{
    public class BaoCaoViewModel
    {
        // Thời gian lọc
        public DateTime TuNgay { get; set; }
        public DateTime DenNgay { get; set; }
        
        // Filter cho charts
        public string ChartFilter { get; set; } = "7days"; // 7days, week, month, year

        // Thống kê tổng quan
        public int TongDonDatXe { get; set; }
        public decimal DoanhThuHomNay { get; set; }
        public int XeDangChoThue { get; set; }
        public int HopDongHoatDong { get; set; }
        public int KhachHangMoi { get; set; } // Khách hàng đăng ký mới hôm nay
        public int TongSoXe { get; set; }

        // Thống kê so với kỳ trước
        public double PhanTramDonDat { get; set; } // % tăng/giảm so với kỳ trước
        public double PhanTramDoanhThu { get; set; }

        // Dữ liệu cho biểu đồ
        public List<BieuDoItem> BieuDoDoanhThu { get; set; }
        public List<BieuDoItem> BieuDoDonDat { get; set; }
        public List<BieuDoItem> BieuDoKhachHangMoi { get; set; }

        // Top xe được thuê nhiều
        public List<XeThueNhieuItem> TopXeThueNhieu { get; set; }

        // Danh sách đơn gần đây
        public List<DonDatGanDayItem> DonDatGanDay { get; set; }

        public BaoCaoViewModel()
        {
            BieuDoDoanhThu = new List<BieuDoItem>();
            BieuDoDonDat = new List<BieuDoItem>();
            BieuDoKhachHangMoi = new List<BieuDoItem>();
            TopXeThueNhieu = new List<XeThueNhieuItem>();
            DonDatGanDay = new List<DonDatGanDayItem>();
        }
    }

    // Class cho item biểu đồ
    public class BieuDoItem
    {
        public string ?Label { get; set; } // Ngày/Tháng
        public decimal Value { get; set; } // Giá trị
    }

    // Class cho xe thuê nhiều
    public class XeThueNhieuItem
    {
        public string ?TenXe { get; set; }
        public string ?BienSo { get; set; }
        public int SoLanThue { get; set; }
        public decimal DoanhThu { get; set; }
    }

    // Class cho đơn đặt gần đây
    public class DonDatGanDayItem
    {
        public int MaDatCho { get; set; }
        public string ?TenKhach { get; set; }
        public string ?TenXe { get; set; }
        public DateTime NgayDat { get; set; }
        public DateTime NgayTra { get; set; }
        public string ?TrangThai { get; set; }
        public decimal TongTien { get; set; }
    }

    // Class cho chart period
    public class ChartPeriod
    {
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string ?Label { get; set; }
    }
}