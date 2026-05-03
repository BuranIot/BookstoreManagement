// ============================================================
// FILE: DAL/TheLoaiDAL.cs
// MÔ TẢ: Lớp truy cập dữ liệu cho bảng TheLoai
//        Dùng ADO.NET thuần để minh họa rõ ràng
// ============================================================
using System;
using System.Collections.Generic;
using System.Data;
using Microsoft.Data.Sqlite;
using BookstoreManagement.DTO;

namespace BookstoreManagement.DAL
{
    public class TheLoaiDAL
    {
        /// <summary>Lấy toàn bộ danh sách thể loại từ DB.</summary>
        public List<TheLoaiDTO> GetAll()
        {
            var list = new List<TheLoaiDTO>();
            const string sql = "SELECT MaTheLoai, TenTheLoai, MoTa FROM TheLoai ORDER BY TenTheLoai";

            using (var conn = DBConnection.GetConnection())
            using (var cmd  = new SqliteCommand(sql, conn))
            using (var reader = cmd.ExecuteReader())
            {
                while (reader.Read())
                {
                    list.Add(new TheLoaiDTO
                    {
                        MaTheLoai  = reader.GetInt32(0),
                        TenTheLoai = reader.GetString(1),
                        MoTa       = reader.IsDBNull(2) ? string.Empty : reader.GetString(2)
                    });
                }
            }
            return list;
        }

        /// <summary>Thêm mới một thể loại. Trả về số dòng bị ảnh hưởng.</summary>
        public int Insert(TheLoaiDTO dto)
        {
            const string sql =
                "INSERT INTO TheLoai (TenTheLoai, MoTa) VALUES (@Ten, @MoTa)";

            using (var conn = DBConnection.GetConnection())
            using (var cmd  = new SqliteCommand(sql, conn))
            {
                cmd.Parameters.AddWithValue("@Ten",  dto.TenTheLoai);
                cmd.Parameters.AddWithValue("@MoTa", (object)dto.MoTa ?? DBNull.Value);
                return cmd.ExecuteNonQuery();
            }
        }

        /// <summary>Cập nhật thể loại theo MaTheLoai.</summary>
        public int Update(TheLoaiDTO dto)
        {
            const string sql =
                "UPDATE TheLoai SET TenTheLoai=@Ten, MoTa=@MoTa WHERE MaTheLoai=@Ma";

            using (var conn = DBConnection.GetConnection())
            using (var cmd  = new SqliteCommand(sql, conn))
            {
                cmd.Parameters.AddWithValue("@Ten",  dto.TenTheLoai);
                cmd.Parameters.AddWithValue("@MoTa", (object)dto.MoTa ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@Ma",   dto.MaTheLoai);
                return cmd.ExecuteNonQuery();
            }
        }

        /// <summary>Xóa thể loại theo MaTheLoai. Lưu ý: cần kiểm tra ràng buộc FK.</summary>
        public int Delete(int maTheLoai)
        {
            const string sql = "DELETE FROM TheLoai WHERE MaTheLoai=@Ma";

            using (var conn = DBConnection.GetConnection())
            using (var cmd  = new SqliteCommand(sql, conn))
            {
                cmd.Parameters.AddWithValue("@Ma", maTheLoai);
                return cmd.ExecuteNonQuery();
            }
        }

        /// <summary>Kiểm tra xem thể loại có sách liên quan không (trước khi xóa).</summary>
        public bool HasRelatedBooks(int maTheLoai)
        {
            const string sql = "SELECT COUNT(*) FROM Sach WHERE MaTheLoai=@Ma";

            using (var conn = DBConnection.GetConnection())
            using (var cmd  = new SqliteCommand(sql, conn))
            {
                cmd.Parameters.AddWithValue("@Ma", maTheLoai);
                return Convert.ToInt64(cmd.ExecuteScalar()) > 0;
            }
        }
    }
}
