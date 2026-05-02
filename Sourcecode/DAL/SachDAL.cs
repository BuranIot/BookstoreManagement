// ============================================================
// FILE: DAL/SachDAL.cs
// MÔ TẢ: Lớp truy cập dữ liệu cho bảng Sach
// ============================================================
using System;
using System.Collections.Generic;
using Microsoft.Data.SqlClient;
using BookstoreManagement.DTO;

namespace BookstoreManagement.DAL
{
    public class SachDAL
    {
        // SQL dùng chung: JOIN với TheLoai để lấy TenTheLoai
        private const string BASE_SQL = @"
            SELECT s.MaSach, s.TenSach, s.TacGia, s.MaTheLoai,
                   t.TenTheLoai, s.GiaBan, s.SoLuongTon
            FROM Sach s
            LEFT JOIN TheLoai t ON s.MaTheLoai = t.MaTheLoai";

        /// <summary>Map một dòng SqlDataReader thành SachDTO.</summary>
        private SachDTO MapRow(SqlDataReader r) => new SachDTO
        {
            MaSach     = r.GetString(0),
            TenSach    = r.GetString(1),
            TacGia     = r.GetString(2),
            MaTheLoai  = r.GetInt32(3),
            TenTheLoai = r.IsDBNull(4) ? "" : r.GetString(4),
            GiaBan     = r.GetDecimal(5),
            SoLuongTon = r.GetInt32(6)
        };

        /// <summary>Lấy toàn bộ danh sách sách.</summary>
        public List<SachDTO> GetAll()
        {
            var list = new List<SachDTO>();
            using (var conn = DBConnection.GetConnection())
            using (var cmd  = new SqlCommand(BASE_SQL + " ORDER BY s.TenSach", conn))
            using (var r    = cmd.ExecuteReader())
                while (r.Read()) list.Add(MapRow(r));
            return list;
        }

        /// <summary>Tìm sách theo tên (LIKE), mã, hoặc thể loại.</summary>
        public List<SachDTO> Search(string keyword, string field = "TenSach")
        {
            var list = new List<SachDTO>();

            // Xây điều kiện WHERE động dựa vào field tìm kiếm
            string where = field switch
            {
                "MaSach"    => "WHERE s.MaSach = @kw",
                "TenTheLoai"=> "WHERE t.TenTheLoai LIKE @kw",
                _           => "WHERE s.TenSach LIKE @kw OR s.TacGia LIKE @kw"
            };

            string sql = BASE_SQL + " " + where + " ORDER BY s.TenSach";

            using (var conn = DBConnection.GetConnection())
            using (var cmd  = new SqlCommand(sql, conn))
            {
                // Dùng LIKE cho tìm kiếm gần đúng
                string paramVal = field == "MaSach" ? keyword : $"%{keyword}%";
                cmd.Parameters.AddWithValue("@kw", paramVal);
                using (var r = cmd.ExecuteReader())
                    while (r.Read()) list.Add(MapRow(r));
            }
            return list;
        }

        /// <summary>Lấy sách sắp hết hàng (tồn kho dưới ngưỡng).</summary>
        public List<SachDTO> GetLowStock(int threshold = 5)
        {
            var list = new List<SachDTO>();
            string sql = BASE_SQL + " WHERE s.SoLuongTon < @threshold ORDER BY s.SoLuongTon";

            using (var conn = DBConnection.GetConnection())
            using (var cmd  = new SqlCommand(sql, conn))
            {
                cmd.Parameters.AddWithValue("@threshold", threshold);
                using (var r = cmd.ExecuteReader())
                    while (r.Read()) list.Add(MapRow(r));
            }
            return list;
        }

        /// <summary>Thêm sách mới vào CSDL.</summary>
        public int Insert(SachDTO dto)
        {
            const string sql = @"
                INSERT INTO Sach (MaSach, TenSach, TacGia, MaTheLoai, GiaBan, SoLuongTon)
                VALUES (@Ma, @Ten, @TacGia, @MaTL, @Gia, @SLT)";

            using (var conn = DBConnection.GetConnection())
            using (var cmd  = new SqlCommand(sql, conn))
            {
                cmd.Parameters.AddWithValue("@Ma",    dto.MaSach);
                cmd.Parameters.AddWithValue("@Ten",   dto.TenSach);
                cmd.Parameters.AddWithValue("@TacGia",dto.TacGia);
                cmd.Parameters.AddWithValue("@MaTL",  dto.MaTheLoai);
                cmd.Parameters.AddWithValue("@Gia",   dto.GiaBan);
                cmd.Parameters.AddWithValue("@SLT",   dto.SoLuongTon);
                return cmd.ExecuteNonQuery();
            }
        }

        /// <summary>Cập nhật thông tin sách.</summary>
        public int Update(SachDTO dto)
        {
            const string sql = @"
                UPDATE Sach
                SET TenSach=@Ten, TacGia=@TacGia, MaTheLoai=@MaTL,
                    GiaBan=@Gia, SoLuongTon=@SLT
                WHERE MaSach=@Ma";

            using (var conn = DBConnection.GetConnection())
            using (var cmd  = new SqlCommand(sql, conn))
            {
                cmd.Parameters.AddWithValue("@Ten",   dto.TenSach);
                cmd.Parameters.AddWithValue("@TacGia",dto.TacGia);
                cmd.Parameters.AddWithValue("@MaTL",  dto.MaTheLoai);
                cmd.Parameters.AddWithValue("@Gia",   dto.GiaBan);
                cmd.Parameters.AddWithValue("@SLT",   dto.SoLuongTon);
                cmd.Parameters.AddWithValue("@Ma",    dto.MaSach);
                return cmd.ExecuteNonQuery();
            }
        }

        /// <summary>Xóa sách theo MaSach.</summary>
        public int Delete(string maSach)
        {
            const string sql = "DELETE FROM Sach WHERE MaSach=@Ma";
            using (var conn = DBConnection.GetConnection())
            using (var cmd  = new SqlCommand(sql, conn))
            {
                cmd.Parameters.AddWithValue("@Ma", maSach);
                return cmd.ExecuteNonQuery();
            }
        }

        /// <summary>Kiểm tra MaSach đã tồn tại chưa (tránh trùng PK).</summary>
        public bool Exists(string maSach)
        {
            const string sql = "SELECT COUNT(*) FROM Sach WHERE MaSach=@Ma";
            using (var conn = DBConnection.GetConnection())
            using (var cmd  = new SqlCommand(sql, conn))
            {
                cmd.Parameters.AddWithValue("@Ma", maSach);
                return (int)cmd.ExecuteScalar() > 0;
            }
        }

        /// <summary>Trừ số lượng tồn kho sau khi bán – gọi trong transaction.</summary>
        public void TruTonKho(string maSach, int soLuong, SqlConnection conn, SqlTransaction tran)
        {
            const string sql = @"
                UPDATE Sach SET SoLuongTon = SoLuongTon - @SL
                WHERE MaSach = @Ma AND SoLuongTon >= @SL";

            using (var cmd = new SqlCommand(sql, conn, tran))
            {
                cmd.Parameters.AddWithValue("@Ma", maSach);
                cmd.Parameters.AddWithValue("@SL", soLuong);
                int rows = cmd.ExecuteNonQuery();

                // Nếu không update được dòng nào → tồn kho không đủ
                if (rows == 0)
                    throw new Exception(
                        $"Sách [{maSach}] không đủ số lượng tồn kho để bán!");
            }
        }
        public bool DaCoTrongHoaDon(string maSach)
        {
            const string sql =
                "SELECT COUNT(*) FROM ChiTietHoaDon WHERE MaSach = @Ma";
            using (var conn = DBConnection.GetConnection())
            using (var cmd = new SqlCommand(sql, conn))
            {
                cmd.Parameters.AddWithValue("@Ma", maSach);
                return (int)cmd.ExecuteScalar() > 0;
            }
        }
    }
}
