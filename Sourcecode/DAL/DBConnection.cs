using System;
using Microsoft.Data.SqlClient;

namespace BookstoreManagement.DAL
{
    public static class DBConnection
    {
        /*private const string CONNECTION_STRING =
            "Server=DESKTOP-QK0T7QH;" +
            "Database=QuanLyNhaSach;" +
            "Integrated Security=True;" +
            "TrustServerCertificate=True;" +
            "Encrypt=False;";*/
        private const string CONNECTION_STRING =
        "Data Source=DESKTOP-QK0T7QH;" +
        "Initial Catalog=QuanLyNhaSach;" +
        "Integrated Security=True;" +
        "TrustServerCertificate=True;" +
        "Encrypt=No;";

        public static SqlConnection GetConnection()
        {
            try
            {
                var conn = new SqlConnection(CONNECTION_STRING);
                conn.Open();
                return conn;
            }
            catch (SqlException ex)
            {
                throw new Exception(
                    $"Không thể kết nối đến cơ sở dữ liệu!\n\nChi tiết: {ex.Message}", ex);
            }
        }

        public static bool TestConnection()
        {
            try
            {
                using (var conn = GetConnection())
                    return conn.State == System.Data.ConnectionState.Open;
            }
            catch
            {
                return false;
            }
        }
    }
}