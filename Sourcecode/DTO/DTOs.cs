// ============================================================
// FILE: DTO/TheLoaiDTO.cs
// MÔ TẢ: Data Transfer Object cho Thể Loại sách
// ============================================================
using System;
using System.Collections.Generic;
using System.Linq;
using BookstoreManagement.DAL;
using BookstoreManagement.DTO;

namespace BookstoreManagement.DTO
{
    public class TheLoaiDTO
    {
        public int    MaTheLoai   { get; set; }
        public string TenTheLoai  { get; set; }
        public string MoTa        { get; set; }

        public override string ToString() => TenTheLoai;
    }
}

// ============================================================
// FILE: DTO/SachDTO.cs
// MÔ TẢ: Data Transfer Object cho Sách
// ============================================================
namespace BookstoreManagement.DTO
{
    public class SachDTO
    {
        public string  MaSach      { get; set; }
        public string  TenSach     { get; set; }
        public string  TacGia      { get; set; }
        public int     MaTheLoai   { get; set; }
        public string  TenTheLoai  { get; set; }  // Join từ bảng TheLoai
        public decimal GiaBan      { get; set; }
        public int     SoLuongTon  { get; set; }
    }
}

// ============================================================
// FILE: DTO/HoaDonDTO.cs
// MÔ TẢ: Data Transfer Object cho Hóa Đơn
// ============================================================

namespace BookstoreManagement.DTO
{
    public class HoaDonDTO
    {
        public int      MaHoaDon  { get; set; }
        public DateTime NgayBan   { get; set; }
        public decimal  TongTien  { get; set; }
        public string   GhiChu    { get; set; }
    }
}

// ============================================================
// FILE: DTO/ChiTietHoaDonDTO.cs
// MÔ TẢ: DTO cho Chi Tiết Hóa Đơn (dùng khi tạo hóa đơn)
// ============================================================
namespace BookstoreManagement.DTO
{
    public class ChiTietHoaDonDTO
    {
        public int     MaHoaDon  { get; set; }
        public string  MaSach    { get; set; }
        public string  TenSach   { get; set; }  // Hiển thị trên UI
        public int     SoLuong   { get; set; }
        public decimal DonGia    { get; set; }

        /// <summary>Tính thành tiền của dòng chi tiết</summary>
        public decimal ThanhTien => SoLuong * DonGia;
    }
}
