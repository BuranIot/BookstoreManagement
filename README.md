# BookstoreManagement
Projent mon Lap trinh huong doi tuong
# 📚 Ứng Dụng Quản Lý Nhà Sách

Ứng dụng desktop quản lý nhà sách xây dựng bằng **C# Windows Forms** theo kiến trúc **3 lớp (3-Tier Architecture)**, kết hợp **SQL Server** làm cơ sở dữ liệu.

---

## 📋 Yêu Cầu Hệ Thống

| Thành phần | Yêu cầu |
|-----------|---------|
| Hệ điều hành | Windows 10/11 (64-bit) |
| SQL Server | SQL Server Express (miễn phí) |
| SSMS | SQL Server Management Studio |
| .NET Runtime | .NET 10 (nếu không dùng Self-contained) |

---

## ⚙️ Hướng Dẫn Cài Đặt

### Bước 1 — Cài SQL Server Express

Tải tại: https://www.microsoft.com/en-us/sql-server/sql-server-downloads

Chọn bản **Express** → cài đặt theo hướng dẫn → ghi lại tên Server.

---

### Bước 2 — Tạo Database

1. Mở **SQL Server Management Studio (SSMS)**
2. Kết nối vào SQL Server của bạn
3. Mở file `Database_NhaSach.sql`
4. Bấm **F5** để chạy → tạo database và dữ liệu mẫu
5. Kiểm tra trong Object Explorer thấy `QuanLyNhaSach` là thành công

---

### Bước 3 — Chạy Ứng Dụng

Vào thư mục `App\` → double click **BookStoreManagement.exe**

> ⚠️ **Lưu ý:** Phải copy **toàn bộ thư mục** `App\`, không chỉ copy mình file `.exe`

---

## 🖥️ Hướng Dẫn Sử Dụng

### Tab 1 — 📖 Quản Lý Sách

![Quản lý sách](https://via.placeholder.com/800x400?text=Tab+Quan+Ly+Sach)

**Thêm sách mới:**
1. Nhập đầy đủ thông tin vào các ô bên phải: Mã sách, Tên sách, Tác giả, Thể loại, Giá bán, Số lượng tồn
2. Bấm **➕ Thêm**

**Sửa thông tin sách:**
1. Bấm chọn sách trong bảng → thông tin tự điền vào ô nhập liệu
2. Chỉnh sửa thông tin cần thay đổi
3. Bấm **✏️ Sửa**

**Xóa sách:**
1. Chọn sách trong bảng
2. Bấm **🗑 Xóa** → xác nhận

**Tìm kiếm:**
- Nhập từ khóa vào ô tìm kiếm
- Chọn tìm theo: Tên sách / Mã sách / Thể loại
- Bấm **🔍 Tìm** hoặc Enter

> 🔴 Các dòng màu đỏ = sách có tồn kho dưới 5 cuốn

---

### Tab 2 — 📂 Quản Lý Thể Loại

**Thêm thể loại:**
1. Nhập Tên thể loại và Mô tả
2. Bấm **➕ Thêm**

**Sửa / Xóa:**
1. Chọn thể loại trong bảng
2. Bấm **✏️ Sửa** hoặc **🗑 Xóa**

> ⚠️ Không thể xóa thể loại còn sách liên quan

---

### Tab 3 — 🛒 Bán Hàng

**Tạo hóa đơn:**

1. **Tìm sách** — nhập tên vào ô tìm kiếm bên trái, danh sách lọc real-time
2. **Chọn sách** — bấm vào sách muốn bán
3. **Nhập số lượng** — điền số lượng vào ô "Số lượng"
4. **Thêm vào giỏ** — bấm **🛒 Thêm vào giỏ**
5. Lặp lại bước 2-4 cho các sách khác
6. **Thanh toán** — bấm **💳 THANH TOÁN** → xác nhận

> ✅ Tồn kho tự động được trừ sau khi thanh toán thành công

**Xóa sách khỏi giỏ:**
- Chọn dòng trong giỏ hàng → bấm **🗑 Xóa dòng**

**Làm mới giỏ hàng:**
- Bấm **🔄 Làm mới** để xóa toàn bộ giỏ

---

### Tab 4 — 📊 Thống Kê

Hiển thị tổng quan hệ thống gồm 4 chỉ số:

| Chỉ số | Mô tả |
|--------|-------|
| 📚 Tổng đầu sách | Tổng số đầu sách trong hệ thống |
| 💰 Trị giá tồn kho | Tổng giá trị hàng tồn (giá × số lượng) |
| 🧾 Tổng hóa đơn | Số hóa đơn đã tạo |
| 💵 Doanh thu | Tổng tiền thu được từ bán hàng |

**Bảng trái** — Danh sách sách sắp hết hàng (tồn kho < 5)

**Bảng phải** — Lịch sử hóa đơn gần đây

Bấm **🔄 Làm mới** để cập nhật số liệu mới nhất.

---

## 🗂️ Cấu Trúc Thư Mục

```
QuanLyNhaSach_Submit/
├── App/
│   ├── BookStoreManagement.exe     ← chạy file này
│   └── (các file .dll đi kèm)
├── SourceCode/
│   ├── DTO/                        ← Data Transfer Objects
│   ├── DAL/                        ← Data Access Layer
│   ├── BLL/                        ← Business Logic Layer
│   └── GUI/                        ← Giao diện người dùng
├── Database_NhaSach.sql            ← script tạo database
└── README.md                       ← file này
```

---

## 🏗️ Kiến Trúc Hệ Thống

```
┌─────────────────────────────────────┐
│         GUI (Windows Forms)         │  ← Giao diện người dùng
├─────────────────────────────────────┤
│      BLL (Business Logic Layer)     │  ← Xử lý nghiệp vụ, LINQ
├─────────────────────────────────────┤
│      DAL (Data Access Layer)        │  ← ADO.NET, Transaction
├─────────────────────────────────────┤
│         SQL Server Database         │  ← Lưu trữ dữ liệu
└─────────────────────────────────────┘
```

---

## ❓ Xử Lý Lỗi Thường Gặp

**Lỗi: Không thể kết nối database**
```
Kiểm tra SQL Server đang chạy:
Windows + R → services.msc → tìm "SQL Server" → Start
```

**Lỗi: Dll was not found**
```
Copy toàn bộ thư mục App\ thay vì chỉ copy file .exe
```

**Lỗi: Database không tồn tại**
```
Chạy lại file Database_NhaSach.sql trong SSMS
```

