// ============================================================
// FILE: BLL/TheLoaiBLL.cs
// MÔ TẢ: Business Logic Layer cho Thể Loại
//        Dùng LINQ để lọc/sắp xếp dữ liệu sau khi lấy từ DAL
// ============================================================
using System;
using System.Collections.Generic;
using System.Linq;
using BookstoreManagement.DAL;
using BookstoreManagement.DTO;

namespace BookstoreManagement.BLL
{
    public class TheLoaiBLL
    {
        private readonly TheLoaiDAL _dal = new TheLoaiDAL();

        /// <summary>Lấy tất cả thể loại, sắp xếp theo tên bằng LINQ.</summary>
        public List<TheLoaiDTO> GetAll()
        {
            var data = _dal.GetAll();
            // Dùng LINQ sắp xếp lại (dù DAL đã ORDER BY, đây là minh họa LINQ)
            return data.OrderBy(t => t.TenTheLoai).ToList();
        }

        /// <summary>Tìm kiếm thể loại theo từ khóa bằng LINQ.</summary>
        public List<TheLoaiDTO> Search(string keyword)
        {
            if (string.IsNullOrWhiteSpace(keyword))
                return GetAll();

            var all = _dal.GetAll();
            // LINQ: lọc theo TenTheLoai hoặc MoTa chứa keyword (không phân biệt HOA/thường)
            return all
                .Where(t => t.TenTheLoai.Contains(keyword, StringComparison.OrdinalIgnoreCase)
                         || (t.MoTa != null && t.MoTa.Contains(keyword, StringComparison.OrdinalIgnoreCase)))
                .OrderBy(t => t.TenTheLoai)
                .ToList();
        }

        /// <summary>Thêm thể loại mới với kiểm tra nghiệp vụ.</summary>
        public (bool success, string message) ThemTheLoai(TheLoaiDTO dto)
        {
            // Kiểm tra dữ liệu đầu vào
            if (string.IsNullOrWhiteSpace(dto.TenTheLoai))
                return (false, "Tên thể loại không được để trống!");

            if (dto.TenTheLoai.Length > 100)
                return (false, "Tên thể loại không được vượt quá 100 ký tự!");

            // Kiểm tra trùng tên (dùng LINQ)
            var all = _dal.GetAll();
            bool exists = all.Any(t =>
                t.TenTheLoai.Equals(dto.TenTheLoai.Trim(), StringComparison.OrdinalIgnoreCase));
            if (exists)
                return (false, $"Thể loại '{dto.TenTheLoai}' đã tồn tại!");

            dto.TenTheLoai = dto.TenTheLoai.Trim();
            _dal.Insert(dto);
            return (true, "Thêm thể loại thành công!");
        }

        /// <summary>Cập nhật thể loại.</summary>
        public (bool success, string message) SuaTheLoai(TheLoaiDTO dto)
        {
            if (string.IsNullOrWhiteSpace(dto.TenTheLoai))
                return (false, "Tên thể loại không được để trống!");

            dto.TenTheLoai = dto.TenTheLoai.Trim();
            _dal.Update(dto);
            return (true, "Cập nhật thể loại thành công!");
        }

        /// <summary>Xóa thể loại – từ chối nếu còn sách liên quan.</summary>
        public (bool success, string message) XoaTheLoai(int maTheLoai)
        {
            // Kiểm tra nghiệp vụ: không xóa nếu còn sách dùng thể loại này
            if (_dal.HasRelatedBooks(maTheLoai))
                return (false, "Không thể xóa vì còn sách thuộc thể loại này!\nVui lòng xóa sách trước.");

            _dal.Delete(maTheLoai);
            return (true, "Xóa thể loại thành công!");
        }
    }
}

// ============================================================
// FILE: BLL/SachBLL.cs
// MÔ TẢ: Business Logic Layer cho Sách
// ============================================================
namespace BookstoreManagement.BLL
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using BookstoreManagement.DAL;
    using BookstoreManagement.DTO;

    public class SachBLL
    {
        private readonly SachDAL _dal = new SachDAL();

        /// <summary>Lấy tất cả sách.</summary>
        public List<SachDTO> GetAll() => _dal.GetAll();

        /// <summary>Tìm kiếm sách theo từ khóa và tiêu chí.</summary>
        public List<SachDTO> Search(string keyword, string field = "TenSach")
        {
            if (string.IsNullOrWhiteSpace(keyword))
                return GetAll();
            return _dal.Search(keyword.Trim(), field);
        }

        /// <summary>
        /// Lấy danh sách sách sắp hết hàng bằng LINQ,
        /// sắp xếp tăng dần theo số lượng tồn.
        /// </summary>
        public List<SachDTO> GetSachSapHetHang(int nguong = 5)
        {
            var all = _dal.GetAll();
            return all
                .Where(s => s.SoLuongTon < nguong)
                .OrderBy(s => s.SoLuongTon)
                .ThenBy(s => s.TenSach)
                .ToList();
        }

        /// <summary>Thêm sách mới với đầy đủ kiểm tra nghiệp vụ.</summary>
        public (bool success, string message) ThemSach(SachDTO dto)
        {
            // --- Validate ---
            if (string.IsNullOrWhiteSpace(dto.MaSach))
                return (false, "Mã sách không được để trống!");
            if (string.IsNullOrWhiteSpace(dto.TenSach))
                return (false, "Tên sách không được để trống!");
            if (string.IsNullOrWhiteSpace(dto.TacGia))
                return (false, "Tác giả không được để trống!");
            if (dto.GiaBan < 0)
                return (false, "Giá bán không được âm!");
            if (dto.SoLuongTon < 0)
                return (false, "Số lượng tồn không được âm!");

            // Kiểm tra trùng mã sách
            if (_dal.Exists(dto.MaSach.Trim().ToUpper()))
                return (false, $"Mã sách '{dto.MaSach}' đã tồn tại trong hệ thống!");

            dto.MaSach  = dto.MaSach.Trim().ToUpper();
            dto.TenSach = dto.TenSach.Trim();
            dto.TacGia  = dto.TacGia.Trim();
            _dal.Insert(dto);
            return (true, "Thêm sách thành công!");
        }

        /// <summary>Cập nhật thông tin sách.</summary>
        public (bool success, string message) SuaSach(SachDTO dto)
        {
            if (string.IsNullOrWhiteSpace(dto.TenSach))
                return (false, "Tên sách không được để trống!");
            if (dto.GiaBan < 0)
                return (false, "Giá bán không được âm!");
            if (dto.SoLuongTon < 0)
                return (false, "Số lượng tồn không được âm!");

            _dal.Update(dto);
            return (true, "Cập nhật sách thành công!");
        }

        /// <summary>Xóa sách khỏi hệ thống.</summary>
        public (bool success, string message) XoaSach(string maSach)
        {
            if (!_dal.Exists(maSach))
                return (false, "Không tìm thấy sách trong hệ thống!");

            // Kiểm tra sách đã xuất hiện trong hóa đơn chưa
            if (_dal.DaCoTrongHoaDon(maSach))
                return (false, "Không thể xóa!\nSách này đã có trong lịch sử hóa đơn.\nChỉ có thể đặt số lượng tồn về 0.");

            _dal.Delete(maSach);
            return (true, "Xóa sách thành công!");
        }
    }
}

// ============================================================
// FILE: BLL/HoaDonBLL.cs
// MÔ TẢ: Business Logic Layer cho Bán Hàng / Hóa Đơn
// ============================================================
namespace BookstoreManagement.BLL
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using BookstoreManagement.DAL;
    using BookstoreManagement.DTO;

    public class HoaDonBLL
    {
        private readonly HoaDonDAL _hoaDonDal = new HoaDonDAL();
        private readonly SachDAL   _sachDal   = new SachDAL();

        /// <summary>
        /// Xử lý nghiệp vụ bán hàng:
        /// - Kiểm tra giỏ hàng hợp lệ
        /// - Tính tổng tiền
        /// - Gọi DAL để lưu hóa đơn + trừ tồn kho (trong Transaction)
        /// </summary>
        public (bool success, string message, int maHoaDon) BanHang(
            List<ChiTietHoaDonDTO> gioHang, string ghiChu = "")
        {
            // --- Kiểm tra giỏ hàng ---
            if (gioHang == null || gioHang.Count == 0)
                return (false, "Giỏ hàng trống, vui lòng chọn sách!", 0);

            // Dùng LINQ kiểm tra số lượng hợp lệ
            var dongLoi = gioHang.Where(ct => ct.SoLuong <= 0).ToList();
            if (dongLoi.Any())
                return (false, "Số lượng sách phải lớn hơn 0!", 0);

            // --- Tính tổng tiền bằng LINQ ---
            decimal tongTien = gioHang.Sum(ct => ct.ThanhTien);

            // --- Tạo đối tượng HoaDon ---
            var hoaDon = new HoaDonDTO
            {
                NgayBan  = DateTime.Now,
                TongTien = tongTien,
                GhiChu   = ghiChu
            };

            try
            {
                int maHD = _hoaDonDal.TaoHoaDon(hoaDon, gioHang);
                return (true, $"Bán hàng thành công!\nMã hóa đơn: {maHD}\nTổng tiền: {tongTien:N0} đ", maHD);
            }
            catch (Exception ex)
            {
                return (false, $"Lỗi khi lưu hóa đơn:\n{ex.Message}", 0);
            }
        }

        /// <summary>Lấy lịch sử hóa đơn.</summary>
        public List<HoaDonDTO> GetLichSu() => _hoaDonDal.GetAll();
    }
}
