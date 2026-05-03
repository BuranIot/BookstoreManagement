using System;
using Microsoft.Data.Sqlite;

namespace BookstoreManagement.DAL
{
    public static class DBConnection
    {
        private static readonly string DB_PATH = System.IO.Path.Combine(
            AppDomain.CurrentDomain.BaseDirectory, "QuanLyNhaSach.db");

        private static readonly string CONNECTION_STRING =
            $"Data Source={DB_PATH}";

        public static SqliteConnection GetConnection()
        {
            try
            {
                var conn = new SqliteConnection(CONNECTION_STRING);
                conn.Open();
                KhoiTaoDatabase(conn);
                return conn;
            }
            catch (Exception ex)
            {
                throw new Exception(
                    $"Không thể mở database!\n\nChi tiết: {ex.Message}", ex);
            }
        }

        private static void KhoiTaoDatabase(SqliteConnection conn)
        {
            using var cmd = conn.CreateCommand();

            // Bước 1: Tạo bảng (luôn chạy, IF NOT EXISTS bảo vệ)
            cmd.CommandText = @"
        CREATE TABLE IF NOT EXISTS TheLoai (
            MaTheLoai   INTEGER PRIMARY KEY AUTOINCREMENT,
            TenTheLoai  TEXT NOT NULL,
            MoTa        TEXT
        );
        CREATE TABLE IF NOT EXISTS Sach (
            MaSach      TEXT PRIMARY KEY,
            TenSach     TEXT NOT NULL,
            TacGia      TEXT NOT NULL,
            MaTheLoai   INTEGER NOT NULL,
            GiaBan      REAL NOT NULL,
            SoLuongTon  INTEGER NOT NULL,
            FOREIGN KEY (MaTheLoai) REFERENCES TheLoai(MaTheLoai)
        );
        CREATE TABLE IF NOT EXISTS HoaDon (
            MaHoaDon  INTEGER PRIMARY KEY AUTOINCREMENT,
            NgayBan   TEXT NOT NULL,
            TongTien  REAL NOT NULL,
            GhiChu    TEXT
        );
        CREATE TABLE IF NOT EXISTS ChiTietHoaDon (
            MaChiTiet INTEGER PRIMARY KEY AUTOINCREMENT,
            MaHoaDon  INTEGER NOT NULL,
            MaSach    TEXT NOT NULL,
            SoLuong   INTEGER NOT NULL,
            DonGia    REAL NOT NULL,
            FOREIGN KEY (MaHoaDon) REFERENCES HoaDon(MaHoaDon),
            FOREIGN KEY (MaSach)   REFERENCES Sach(MaSach)
        );
    ";
            cmd.ExecuteNonQuery();

            // Bước 2: Chỉ insert dữ liệu mẫu khi bảng TRỐNG
            cmd.CommandText = "SELECT COUNT(*) FROM TheLoai";
            long count = (long)cmd.ExecuteScalar();
            if (count > 0) return;  // ← đã có dữ liệu rồi, bỏ qua

            cmd.CommandText = @"
        INSERT INTO TheLoai (TenTheLoai, MoTa) VALUES
        ('Văn học',           'Tiểu thuyết, truyện ngắn, thơ ca'),
        ('Khoa học kỹ thuật', 'Lập trình, điện tử, cơ khí'),
        ('Kinh tế',           'Tài chính, kinh doanh, marketing'),
        ('Lịch sử',           'Lịch sử Việt Nam và thế giới'),
        ('Thiếu nhi',         'Truyện tranh, sách giáo dục trẻ em');

        INSERT OR IGNORE INTO Sach VALUES
        ('S001','Dế Mèn Phiêu Lưu Ký','Tô Hoài',          1, 65000, 20),
        ('S002','Số Đỏ',               'Vũ Trọng Phụng',   1, 72000,  3),
        ('S003','Lập Trình C# Cơ Bản', 'Nguyễn Văn A',     2,120000, 15),
        ('S004','Clean Code',           'Robert C. Martin', 2,250000,  8),
        ('S005','Đắc Nhân Tâm',         'Dale Carnegie',    3, 89000, 30),
        ('S006','Tư Duy Nhanh Và Chậm','Daniel Kahneman',  3,145000,  2),
        ('S007','Lịch Sử Việt Nam',     'Phan Huy Lê',      4, 95000, 12),
        ('S008','Sapiens',              'Yuval Noah Harari',4,195000,  4),
        ('S009','Doraemon Tập 1',       'Fujiko F. Fujio',  5, 35000, 50),
        ('S010','Conan Tập 1',          'Gosho Aoyama',     5, 32000,  1);
    ";
            cmd.ExecuteNonQuery();
        }

        public static bool TestConnection()
        {
            try
            {
                using var conn = GetConnection();
                return conn.State == System.Data.ConnectionState.Open;
            }
            catch { return false; }
        }
    }
}