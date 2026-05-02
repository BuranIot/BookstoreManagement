// ============================================================
// FILE: DAL/HoaDonDAL.cs
// MÔ TẢ: Lớp truy cập dữ liệu cho Hóa Đơn
//        Dùng Transaction để đảm bảo toàn vẹn dữ liệu khi bán hàng
// ============================================================
using System;
using System.Collections.Generic;
using Microsoft.Data.SqlClient;
using BookstoreManagement.DTO;

namespace BookstoreManagement.DAL
{
    public class HoaDonDAL
    {
        private readonly SachDAL _sachDAL = new SachDAL();

        /// <summary>
        /// Lưu hóa đơn + chi tiết + TỰ ĐỘNG TRỪ TỒN KHO trong một Transaction.
        /// Nếu bất kỳ bước nào lỗi → ROLLBACK toàn bộ, không mất dữ liệu.
        /// </summary>
        /// <param name="hoaDon">Thông tin đầu hóa đơn</param>
        /// <param name="chiTiets">Danh sách chi tiết các sách đã bán</param>
        /// <returns>MaHoaDon vừa tạo</returns>
        public int TaoHoaDon(HoaDonDTO hoaDon, List<ChiTietHoaDonDTO> chiTiets)
        {
            if (chiTiets == null || chiTiets.Count == 0)
                throw new ArgumentException("Hóa đơn phải có ít nhất một sản phẩm!");

            using (var conn = DBConnection.GetConnection())
            using (var tran = conn.BeginTransaction())
            {
                try
                {
                    // --- BƯỚC 1: Chèn vào bảng HoaDon, lấy lại MaHoaDon ---
                    const string sqlHD = @"
                        INSERT INTO HoaDon (NgayBan, TongTien, GhiChu)
                        VALUES (@Ngay, @Tong, @GhiChu);
                        SELECT SCOPE_IDENTITY();";

                    int maHoaDon;
                    using (var cmd = new SqlCommand(sqlHD, conn, tran))
                    {
                        cmd.Parameters.AddWithValue("@Ngay",   hoaDon.NgayBan);
                        cmd.Parameters.AddWithValue("@Tong",   hoaDon.TongTien);
                        cmd.Parameters.AddWithValue("@GhiChu", (object)hoaDon.GhiChu ?? DBNull.Value);
                        maHoaDon = Convert.ToInt32(cmd.ExecuteScalar());
                    }

                    // --- BƯỚC 2: Chèn từng dòng ChiTietHoaDon + trừ tồn kho ---
                    const string sqlCT = @"
                        INSERT INTO ChiTietHoaDon (MaHoaDon, MaSach, SoLuong, DonGia)
                        VALUES (@MaHD, @MaSach, @SL, @DonGia)";

                    foreach (var ct in chiTiets)
                    {
                        // 2a. Trừ tồn kho (gọi DAL Sach, truyền connection và transaction)
                        _sachDAL.TruTonKho(ct.MaSach, ct.SoLuong, conn, tran);

                        // 2b. Lưu chi tiết hóa đơn
                        using (var cmd = new SqlCommand(sqlCT, conn, tran))
                        {
                            cmd.Parameters.AddWithValue("@MaHD",  maHoaDon);
                            cmd.Parameters.AddWithValue("@MaSach", ct.MaSach);
                            cmd.Parameters.AddWithValue("@SL",     ct.SoLuong);
                            cmd.Parameters.AddWithValue("@DonGia", ct.DonGia);
                            cmd.ExecuteNonQuery();
                        }
                    }

                    // --- BƯỚC 3: Commit khi tất cả thành công ---
                    tran.Commit();
                    return maHoaDon;
                }
                catch
                {
                    // Rollback nếu có bất kỳ lỗi nào
                    tran.Rollback();
                    throw; // Ném lại để BLL/GUI xử lý và hiển thị thông báo
                }
            }
        }

        /// <summary>Lấy danh sách tất cả hóa đơn (để xem lịch sử).</summary>
        public List<HoaDonDTO> GetAll()
        {
            var list = new List<HoaDonDTO>();
            const string sql =
                "SELECT MaHoaDon, NgayBan, TongTien, GhiChu FROM HoaDon ORDER BY NgayBan DESC";

            using (var conn = DBConnection.GetConnection())
            using (var cmd  = new SqlCommand(sql, conn))
            using (var r    = cmd.ExecuteReader())
            {
                while (r.Read())
                {
                    list.Add(new HoaDonDTO
                    {
                        MaHoaDon = r.GetInt32(0),
                        NgayBan  = r.GetDateTime(1),
                        TongTien = r.GetDecimal(2),
                        GhiChu   = r.IsDBNull(3) ? "" : r.GetString(3)
                    });
                }
            }
            return list;
        }
    }
}
