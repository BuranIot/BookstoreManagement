// ============================================================
// FILE: GUI/frmMain.cs
// MÔ TẢ: Form chính duy nhất, dùng TabControl để chứa tất cả
//        chức năng: Sách, Thể Loại, Bán Hàng, Thống Kê
// ============================================================
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using BookstoreManagement.BLL;
using BookstoreManagement.DTO;

namespace BookstoreManagement.GUI
{
    public class frmMain : Form
    {
        // ==================== SHARED BLL ====================
        private readonly SachBLL _sachBLL = new SachBLL();
        private readonly TheLoaiBLL _theLoaiBLL = new TheLoaiBLL();
        private readonly HoaDonBLL _hoaDonBLL = new HoaDonBLL();

        // ==================== TAB: SÁCH ====================
        private DataGridView dgvSach;
        private TextBox txtMaSach, txtTenSach, txtTacGia, txtGiaBan, txtSoLuong, txtTimSach;
        private ComboBox cboTheLoaiSach, cboTimTheo;
        private Button btnThemSach, btnSuaSach, btnXoaSach, btnLamMoiSach, btnTimSach;

        // ==================== TAB: THỂ LOẠI ====================
        private DataGridView dgvTheLoai;
        private TextBox txtMaTL, txtTenTL, txtMoTa;
        private Button btnThemTL, btnSuaTL, btnXoaTL, btnLamMoiTL;

        // ==================== TAB: BÁN HÀNG ====================
        private DataGridView dgvDanhSachSach, dgvGioHang;
        private TextBox txtTimSachBan, txtSoLuongBan, txtGhiChu;
        private Label lblTongTien, lblThongTinSach;
        private Button btnThemVaoGio, btnXoaKhoiGio, btnThanhToan, btnLamMoiGio;
        private List<ChiTietHoaDonDTO> _gioHang = new List<ChiTietHoaDonDTO>();

        // ==================== TAB: THỐNG KÊ ====================
private DataGridView dgvThongKe, dgvHoaDon;  // ← thêm dgvHoaDon
private Label lblTongSach, lblTongTienTK, lblSapHet, lblTongHoaDon, lblDoanhThu;
private Button btnLamMoiTK;

        // ==================== STATUS BAR ====================
        private Label lblStatus;

        public frmMain()
        {
            InitializeComponent();
            LoadTheLoaiCombo();
            LoadSach();
            LoadTheLoai();
            LoadThongKe();
        }

        // ============================================================
        // KHỞI TẠO GIAO DIỆN CHÍNH
        // ============================================================
        private void InitializeComponent()
        {
            this.Text = "📚 Quản Lý Nhà Sách";
            this.Size = new Size(1200, 720);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.MinimumSize = new Size(1000, 600);
            this.BackColor = Color.White;

            // ---- TabControl chính ----
            var tab = new TabControl
            {
                Dock = DockStyle.Fill,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                Padding = new Point(15, 5)
            };

            tab.TabPages.Add(TabSach());
            tab.TabPages.Add(TabTheLoai());
            tab.TabPages.Add(TabBanHang());
            tab.TabPages.Add(TabThongKe());

            // ---- Status bar ----
            lblStatus = new Label
            {
                Dock = DockStyle.Bottom,
                Height = 28,
                BackColor = Color.FromArgb(41, 128, 185),
                ForeColor = Color.White,
                TextAlign = ContentAlignment.MiddleLeft,
                Padding = new Padding(10, 0, 0, 0),
                Font = new Font("Segoe UI", 9),
                Text = "  Sẵn sàng."
            };

            this.Controls.Add(tab);
            this.Controls.Add(lblStatus);
        }

        // ============================================================
        // TAB 1: QUẢN LÝ SÁCH
        // ============================================================
        private TabPage TabSach()
        {
            var page = new TabPage("  📖 Quản lý Sách  ");

            // ---- Panel tìm kiếm ----
            var pnlTop = new Panel { Dock = DockStyle.Top, Height = 48, BackColor = Color.FromArgb(236, 240, 241) };
            var lblTim = new Label { Text = "Tìm:", Left = 10, Top = 15, AutoSize = true, Font = new Font("Segoe UI", 9) };
            txtTimSach = new TextBox { Left = 45, Top = 12, Width = 200, Font = new Font("Segoe UI", 9) };
            txtTimSach.KeyPress += (s, e) => { if (e.KeyChar == 13) BtnTimSach_Click(null, null); };

            cboTimTheo = new ComboBox { Left = 255, Top = 12, Width = 120, DropDownStyle = ComboBoxStyle.DropDownList, Font = new Font("Segoe UI", 9) };
            cboTimTheo.Items.AddRange(new[] { "Tên sách", "Mã sách", "Thể loại" });
            cboTimTheo.SelectedIndex = 0;

            btnTimSach = MakeSmallButton("🔍 Tìm", 385, 10, Color.FromArgb(41, 128, 185));
            btnTimSach.Click += BtnTimSach_Click;
            var btnXoaTim = MakeSmallButton("✖ Xóa", 465, 10, Color.Gray);
            btnXoaTim.Click += (s, e) => { txtTimSach.Clear(); LoadSach(); };

            pnlTop.Controls.AddRange(new Control[] { lblTim, txtTimSach, cboTimTheo, btnTimSach, btnXoaTim });

            // ---- DataGridView ----
            dgvSach = MakeDGV();
            dgvSach.SelectionChanged += (s, e) => DgvSach_Fill();

            // ---- Panel nhập liệu ----
            var pnlRight = MakeInputPanel(300);
            int y = 10;

            pnlRight.Controls.Add(MakeSectionLabel("THÔNG TIN SÁCH", y)); y += 30;
            pnlRight.Controls.Add(MakeLabel("Mã sách:", 10, y));
            txtMaSach = MakeTxt(110, y, 175); pnlRight.Controls.Add(txtMaSach); y += 32;

            pnlRight.Controls.Add(MakeLabel("Tên sách:", 10, y));
            txtTenSach = MakeTxt(110, y, 175); pnlRight.Controls.Add(txtTenSach); y += 32;

            pnlRight.Controls.Add(MakeLabel("Tác giả:", 10, y));
            txtTacGia = MakeTxt(110, y, 175); pnlRight.Controls.Add(txtTacGia); y += 32;

            pnlRight.Controls.Add(MakeLabel("Thể loại:", 10, y));
            cboTheLoaiSach = new ComboBox { Left = 110, Top = y, Width = 175, DropDownStyle = ComboBoxStyle.DropDownList, Font = new Font("Segoe UI", 9) };
            pnlRight.Controls.Add(cboTheLoaiSach); y += 32;

            pnlRight.Controls.Add(MakeLabel("Giá bán (đ):", 10, y));
            txtGiaBan = MakeTxt(110, y, 175); pnlRight.Controls.Add(txtGiaBan); y += 32;

            pnlRight.Controls.Add(MakeLabel("Tồn kho:", 10, y));
            txtSoLuong = MakeTxt(110, y, 175); pnlRight.Controls.Add(txtSoLuong); y += 42;

            btnThemSach = MakeBtn("➕ Thêm", 10, y, Color.FromArgb(39, 174, 96), 275); pnlRight.Controls.Add(btnThemSach); y += 38;
            btnSuaSach = MakeBtn("✏️ Sửa", 10, y, Color.FromArgb(230, 126, 34), 275); pnlRight.Controls.Add(btnSuaSach); y += 38;
            btnXoaSach = MakeBtn("🗑 Xóa", 10, y, Color.FromArgb(192, 57, 43), 275); pnlRight.Controls.Add(btnXoaSach); y += 38;
            btnLamMoiSach = MakeBtn("🔄 Làm mới", 10, y, Color.Gray, 275); pnlRight.Controls.Add(btnLamMoiSach);

            btnThemSach.Click += BtnThemSach_Click;
            btnSuaSach.Click += BtnSuaSach_Click;
            btnXoaSach.Click += BtnXoaSach_Click;
            btnLamMoiSach.Click += (s, e) => LamMoiSach();

            var pnlCenter = new Panel { Dock = DockStyle.Fill };
            pnlCenter.Controls.Add(dgvSach);

            page.Controls.Add(pnlCenter);
            page.Controls.Add(pnlRight);
            page.Controls.Add(pnlTop);
            return page;
        }

        // ============================================================
        // TAB 2: QUẢN LÝ THỂ LOẠI
        // ============================================================
        private TabPage TabTheLoai()
        {
            var page = new TabPage("  📂 Thể Loại  ");

            dgvTheLoai = MakeDGV();
            dgvTheLoai.SelectionChanged += (s, e) => DgvTL_Fill();

            var pnlRight = MakeInputPanel(300);
            int y = 10;

            pnlRight.Controls.Add(MakeSectionLabel("THÔNG TIN THỂ LOẠI", y)); y += 30;
            pnlRight.Controls.Add(MakeLabel("Mã TL:", 10, y));
            txtMaTL = MakeTxt(100, y, 185); txtMaTL.ReadOnly = true; txtMaTL.BackColor = Color.FromArgb(236, 240, 241);
            pnlRight.Controls.Add(txtMaTL); y += 32;

            pnlRight.Controls.Add(MakeLabel("Tên TL:", 10, y));
            txtTenTL = MakeTxt(100, y, 185); pnlRight.Controls.Add(txtTenTL); y += 32;

            pnlRight.Controls.Add(MakeLabel("Mô tả:", 10, y));
            txtMoTa = new TextBox { Left = 100, Top = y, Width = 185, Height = 70, Multiline = true, Font = new Font("Segoe UI", 9) };
            pnlRight.Controls.Add(txtMoTa); y += 82;

            btnThemTL = MakeBtn("➕ Thêm", 10, y, Color.FromArgb(39, 174, 96), 275); pnlRight.Controls.Add(btnThemTL); y += 38;
            btnSuaTL = MakeBtn("✏️ Sửa", 10, y, Color.FromArgb(230, 126, 34), 275); pnlRight.Controls.Add(btnSuaTL); y += 38;
            btnXoaTL = MakeBtn("🗑 Xóa", 10, y, Color.FromArgb(192, 57, 43), 275); pnlRight.Controls.Add(btnXoaTL); y += 38;
            btnLamMoiTL = MakeBtn("🔄 Làm mới", 10, y, Color.Gray, 275); pnlRight.Controls.Add(btnLamMoiTL);

            btnThemTL.Click += BtnThemTL_Click;
            btnSuaTL.Click += BtnSuaTL_Click;
            btnXoaTL.Click += BtnXoaTL_Click;
            btnLamMoiTL.Click += (s, e) => LamMoiTL();

            var pnlCenter = new Panel { Dock = DockStyle.Fill };
            pnlCenter.Controls.Add(dgvTheLoai);

            page.Controls.Add(pnlCenter);
            page.Controls.Add(pnlRight);
            return page;
        }

        // ============================================================
        // TAB 3: BÁN HÀNG
        // ============================================================
        private TabPage TabBanHang()
        {
            var page = new TabPage("  🛒 Bán Hàng  ");

            // ---- Panel PHẢI: Giỏ hàng (khai báo trước để Fill hoạt động đúng) ----
            var pnlPhai = new Panel { Dock = DockStyle.Right, Width = 420, BackColor = Color.White };

            var lblGio = new Label
            {
                Text = "🛒  GIỎ HÀNG",
                Dock = DockStyle.Top,
                Height = 35,
                BackColor = Color.FromArgb(41, 128, 185),
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 11, FontStyle.Bold),
                TextAlign = ContentAlignment.MiddleCenter
            };

            dgvGioHang = MakeDGV();
            dgvGioHang.Columns.Add("TenSach", "Tên sách");
            dgvGioHang.Columns.Add("SoLuong", "SL");
            dgvGioHang.Columns.Add("DonGia", "Đơn giá");
            dgvGioHang.Columns.Add("ThanhTien", "Thành tiền");
            dgvGioHang.Columns["SoLuong"].Width = 45;
            dgvGioHang.Columns["DonGia"].Width = 90;
            dgvGioHang.Columns["ThanhTien"].Width = 100;

            var pnlTong = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 115,
                BackColor = Color.FromArgb(236, 240, 241),
                Padding = new Padding(10)
            };
            lblTongTien = new Label
            {
                Text = "TỔNG TIỀN:  0 đ",
                Dock = DockStyle.Top,
                Height = 38,
                Font = new Font("Segoe UI", 13, FontStyle.Bold),
                ForeColor = Color.FromArgb(192, 57, 43),
                TextAlign = ContentAlignment.MiddleLeft
            };
            var lblGhiChu = new Label { Text = "Ghi chú:", Left = 0, Top = 46, AutoSize = true, Font = new Font("Segoe UI", 9) };
            txtGhiChu = new TextBox { Left = 65, Top = 43, Width = 180, Font = new Font("Segoe UI", 9) };

            btnXoaKhoiGio = MakeSmallButton("🗑 Xóa dòng", 0, 72, Color.FromArgb(192, 57, 43));
            btnLamMoiGio = MakeSmallButton("🔄 Làm mới", 90, 72, Color.Gray);
            btnThanhToan = MakeSmallButton("💳 THANH TOÁN", 180, 67, Color.FromArgb(39, 174, 96));
            btnThanhToan.Width = 130; btnThanhToan.Height = 38;
            btnThanhToan.Font = new Font("Segoe UI", 10, FontStyle.Bold);

            btnXoaKhoiGio.Click += BtnXoaKhoiGio_Click;
            btnLamMoiGio.Click += (s, e) => LamMoiGio();
            btnThanhToan.Click += BtnThanhToan_Click;

            pnlTong.Controls.AddRange(new Control[] { lblTongTien, lblGhiChu, txtGhiChu, btnXoaKhoiGio, btnLamMoiGio, btnThanhToan });

            var pnlGioCenter = new Panel { Dock = DockStyle.Fill };
            pnlGioCenter.Controls.Add(dgvGioHang);

            pnlPhai.Controls.Add(pnlGioCenter);
            pnlPhai.Controls.Add(pnlTong);
            pnlPhai.Controls.Add(lblGio);

            // ---- Panel TRÁI: Danh sách sách ----
            var pnlTrai = new Panel { Dock = DockStyle.Fill, BackColor = Color.White };

            var pnlTimBan = new Panel { Dock = DockStyle.Top, Height = 48, BackColor = Color.FromArgb(236, 240, 241) };
            var lblTimBan = new Label { Text = "Tìm sách:", Left = 8, Top = 14, AutoSize = true, Font = new Font("Segoe UI", 9) };
            txtTimSachBan = new TextBox { Left = 75, Top = 11, Width = 200, Font = new Font("Segoe UI", 9) };
            txtTimSachBan.TextChanged += (s, e) =>
            {
                var kq = _sachBLL.Search(txtTimSachBan.Text, "TenSach");
                dgvDanhSachSach.DataSource = null;
                dgvDanhSachSach.DataSource = kq;
                FormatDgvSachBan();
            };
            pnlTimBan.Controls.AddRange(new Control[] { lblTimBan, txtTimSachBan });

            dgvDanhSachSach = MakeDGV();
            dgvDanhSachSach.SelectionChanged += DgvSachBan_SelectionChanged;

            lblThongTinSach = new Label
            {
                Dock = DockStyle.Bottom,
                Height = 50,
                BackColor = Color.FromArgb(236, 240, 241),
                Font = new Font("Segoe UI", 9),
                Padding = new Padding(8),
                Text = "← Chọn sách từ danh sách"
            };

            var pnlAddGio = new Panel { Dock = DockStyle.Bottom, Height = 48, BackColor = Color.White };
            var lblSL = new Label { Text = "Số lượng:", Left = 8, Top = 14, AutoSize = true, Font = new Font("Segoe UI", 9) };
            txtSoLuongBan = new TextBox { Left = 78, Top = 11, Width = 60, Text = "1", Font = new Font("Segoe UI", 9) };
            btnThemVaoGio = MakeSmallButton("🛒 Thêm vào giỏ", 148, 10, Color.FromArgb(39, 174, 96));
            btnThemVaoGio.Width = 140;
            btnThemVaoGio.Click += BtnThemVaoGio_Click;
            pnlAddGio.Controls.AddRange(new Control[] { lblSL, txtSoLuongBan, btnThemVaoGio });

            var pnlSachCenter = new Panel { Dock = DockStyle.Fill };
            pnlSachCenter.Controls.Add(dgvDanhSachSach);

            pnlTrai.Controls.Add(pnlSachCenter);
            pnlTrai.Controls.Add(pnlAddGio);
            pnlTrai.Controls.Add(lblThongTinSach);
            pnlTrai.Controls.Add(pnlTimBan);

            // ---- Thêm vào page — PHẢI thêm pnlPhai TRƯỚC pnlTrai ----
            page.Controls.Add(pnlTrai);   // Fill — thêm sau
            page.Controls.Add(pnlPhai);   // Right — thêm trước

            return page;
        }

        // ============================================================
        // TAB 4: THỐNG KÊ
        // ============================================================
        private TabPage TabThongKe()
        {
            var page = new TabPage("  📊 Thống Kê  ");

            var pnlCards = new Panel
            {
                Dock = DockStyle.Top,
                Height = 100,
                BackColor = Color.FromArgb(245, 246, 250)
            };

            (Panel card, Label valLabel) MakeCard(string title, Color accent, int left)
            {
                var card = new Panel { Left = left, Top = 8, Width = 200, Height = 78, BackColor = Color.White };
                var bar = new Panel { Dock = DockStyle.Left, Width = 6, BackColor = accent };
                var lblT = new Label { Text = title, Left = 14, Top = 8, Width = 175, Height = 20, Font = new Font("Segoe UI", 8, FontStyle.Bold), ForeColor = Color.Gray, AutoSize = false };
                var lblV = new Label { Text = "--", Left = 14, Top = 30, Width = 175, Height = 38, Font = new Font("Segoe UI", 13, FontStyle.Bold), ForeColor = accent, AutoSize = false };
                card.Controls.AddRange(new Control[] { bar, lblT, lblV });
                return (card, lblV);
            }

            var (c1, lbl1) = MakeCard("TỔNG ĐẦU SÁCH", Color.FromArgb(41, 128, 185), 10);
            var (c2, lbl2) = MakeCard("TRỊ GIÁ TỒN KHO", Color.FromArgb(39, 174, 96), 220);
            var (c3, lbl3) = MakeCard("TỔNG HÓA ĐƠN", Color.FromArgb(142, 68, 173), 430);
            var (c4, lbl4) = MakeCard("DOANH THU", Color.FromArgb(192, 57, 43), 640);

            lblTongSach = lbl1;
            lblTongTienTK = lbl2;
            lblTongHoaDon = lbl3;
            lblDoanhThu = lbl4;
            lblSapHet = new Label(); // giữ để tránh null

            btnLamMoiTK = new Button
            {
                Text = "🔄 Làm mới",
                Left = 855,
                Top = 28,
                Width = 100,
                Height = 36,
                BackColor = Color.FromArgb(41, 128, 185),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9, FontStyle.Bold)
            };
            btnLamMoiTK.FlatAppearance.BorderSize = 0;
            btnLamMoiTK.Click += (s, e) => LoadThongKe();

            pnlCards.Controls.AddRange(new Control[] { c1, c2, c3, c4, btnLamMoiTK });

            // ---- Phần dưới: 2 bảng ----
            var pnlBottom = new Panel { Dock = DockStyle.Fill };

            // Bảng phải: Hóa đơn
            var pnlHoaDon = new Panel { Dock = DockStyle.Right, Width = 430, BackColor = Color.White };
            var lblHDTitle = new Label
            {
                Text = "🧾  LỊCH SỬ HÓA ĐƠN",
                Dock = DockStyle.Top,
                Height = 32,
                BackColor = Color.FromArgb(142, 68, 173),
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                TextAlign = ContentAlignment.MiddleCenter
            };
            dgvHoaDon = new DataGridView
            {
                Dock = DockStyle.Fill,
                ReadOnly = true,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect = false,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                RowHeadersVisible = false,
                BackgroundColor = Color.White,
                BorderStyle = BorderStyle.None,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                Font = new Font("Segoe UI", 9),
                ColumnHeadersDefaultCellStyle = new DataGridViewCellStyle
                {
                    BackColor = Color.FromArgb(142, 68, 173),
                    ForeColor = Color.White,
                    Font = new Font("Segoe UI", 9, FontStyle.Bold)
                },
                EnableHeadersVisualStyles = false
            };
            var pnlHDCenter = new Panel { Dock = DockStyle.Fill };
            pnlHDCenter.Controls.Add(dgvHoaDon);
            pnlHoaDon.Controls.Add(pnlHDCenter);
            pnlHoaDon.Controls.Add(lblHDTitle);

            // Bảng trái: Sách sắp hết
            var pnlSapHet = new Panel { Dock = DockStyle.Fill, BackColor = Color.White };
            var lblSapHetTitle = new Label
            {
                Text = "SÁCH SẮP HẾT HÀNG  (tồn kho < 5)",
                Dock = DockStyle.Top,
                Height = 32,
                BackColor = Color.FromArgb(128, 80, 27),
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                TextAlign = ContentAlignment.MiddleCenter
            };
            dgvThongKe = MakeDGV();
            dgvThongKe.ColumnHeadersDefaultCellStyle = new DataGridViewCellStyle
            {
                BackColor = Color.FromArgb(128, 80, 27),
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 9, FontStyle.Bold)
            };
            var pnlTKCenter = new Panel { Dock = DockStyle.Fill };
            pnlTKCenter.Controls.Add(dgvThongKe);
            pnlSapHet.Controls.Add(pnlTKCenter);
            pnlSapHet.Controls.Add(lblSapHetTitle);

            var separator = new Panel { Dock = DockStyle.Right, Width = 1, BackColor = Color.FromArgb(220, 220, 220) };

            pnlBottom.Controls.Add(pnlSapHet);
            pnlBottom.Controls.Add(separator);
            pnlBottom.Controls.Add(pnlHoaDon);

            page.Controls.Add(pnlBottom);
            page.Controls.Add(pnlCards);
            return page;
        }

        // ============================================================
        // LOAD DỮ LIỆU
        // ============================================================
        private void LoadTheLoaiCombo()
        {
            try
            {
                var list = _theLoaiBLL.GetAll();
                cboTheLoaiSach.DataSource = list;
                cboTheLoaiSach.DisplayMember = "TenTheLoai";
                cboTheLoaiSach.ValueMember = "MaTheLoai";
            }
            catch { }
        }

        private void LoadSach(List<SachDTO> data = null)
        {
            try
            {
                var list = data ?? _sachBLL.GetAll();
                dgvSach.DataSource = null;
                dgvSach.DataSource = list;
                SetHeaders(dgvSach, new[] {
                    ("MaSach","Mã sách"), ("TenSach","Tên sách"), ("TacGia","Tác giả"),
                    ("TenTheLoai","Thể loại"), ("GiaBan","Giá bán (đ)"), ("SoLuongTon","Tồn kho")
                });
                SetColumnWidths(dgvSach, new Dictionary<string, int>
                {
                    { "MaSach",     70  },
                    { "TenSach",    220 },
                    { "TacGia",     130 },
                    { "TenTheLoai", 100 },
                    { "GiaBan",     90  },
                });
                if (dgvSach.Columns["MaTheLoai"] != null) dgvSach.Columns["MaTheLoai"].Visible = false;
                if (dgvSach.Columns["GiaBan"] != null) dgvSach.Columns["GiaBan"].DefaultCellStyle.Format = "N0";

                // Tô màu đỏ nhạt cho sách sắp hết
                foreach (DataGridViewRow r in dgvSach.Rows)
                    if (r.DataBoundItem is SachDTO s && s.SoLuongTon < 5)
                        r.DefaultCellStyle.BackColor = Color.FromArgb(255, 200, 200);

                SetStatus($"Sách: {list.Count} cuốn.");

                // Cập nhật luôn danh sách sách bên tab Bán Hàng
                dgvDanhSachSach.DataSource = null;
                dgvDanhSachSach.DataSource = _sachBLL.GetAll();
                FormatDgvSachBan();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi tải sách:\n{ex.Message}", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void FormatDgvSachBan()
        {
            SetHeaders(dgvDanhSachSach, new[] {
                ("MaSach","Mã"), ("TenSach","Tên sách"), ("TacGia","Tác giả"),
                ("TenTheLoai","Thể loại"), ("GiaBan","Giá (đ)"), ("SoLuongTon","Tồn")
            });
            if (dgvDanhSachSach.Columns["MaTheLoai"] != null) dgvDanhSachSach.Columns["MaTheLoai"].Visible = false;
            if (dgvDanhSachSach.Columns["GiaBan"] != null) dgvDanhSachSach.Columns["GiaBan"].DefaultCellStyle.Format = "N0";
            SetColumnWidths(dgvDanhSachSach, new Dictionary<string, int>
            {
                { "MaSach",     60  },
                { "TenSach",    200 },
                { "TacGia",     120 },
                { "TenTheLoai", 100 },
                { "GiaBan",     90  },
                // SoLuongTon → Fill nốt
            });
        }

        private void LoadTheLoai()
        {
            try
            {
                var list = _theLoaiBLL.GetAll();
                dgvTheLoai.DataSource = null;
                dgvTheLoai.DataSource = list;
                SetHeaders(dgvTheLoai, new[] {
                    ("MaTheLoai","Mã"), ("TenTheLoai","Tên thể loại"), ("MoTa","Mô tả")
                });
                SetColumnWidths(dgvTheLoai, new Dictionary<string, int>
                {
                    { "MaTheLoai",  60  },
                    { "TenTheLoai", 160 },
                    // MoTa → tự Fill nốt
                });
                SetStatus($"Thể loại: {list.Count}.");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi tải thể loại:\n{ex.Message}", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void LoadThongKe()
        {
            try
            {
                var sapHet = _sachBLL.GetSachSapHetHang(5);
                var tatCa = _sachBLL.GetAll();
                var dsHD = _hoaDonBLL.GetLichSu();

                decimal tongTriGia = tatCa.Sum(s => s.GiaBan * s.SoLuongTon);
                decimal tongDoanhThu = dsHD.Sum(h => h.TongTien);

                lblTongSach.Text = $"{tatCa.Count} đầu sách";
                lblTongTienTK.Text = $"{tongTriGia:N0} đ";
                lblTongHoaDon.Text = $"{dsHD.Count} hóa đơn";
                lblDoanhThu.Text = $"{tongDoanhThu:N0} đ";

                // Bảng sách sắp hết
                dgvThongKe.DataSource = null;
                dgvThongKe.DataSource = sapHet;
                SetHeaders(dgvThongKe, new[] {
                    ("MaSach","Mã"), ("TenSach","Tên sách"), ("TacGia","Tác giả"),
                    ("TenTheLoai","Thể loại"), ("GiaBan","Giá (đ)"), ("SoLuongTon","Tồn")
                });

                SetColumnWidths(dgvThongKe, new Dictionary<string, int>
                {
                    { "MaSach",     70  },
                    { "TenSach",    180 },
                    { "TacGia",     120 },
                    { "TenTheLoai", 100 },
                    { "GiaBan",     90  },
                    // SoLuongTon → tự Fill nốt
                });
                if (dgvThongKe.Columns["MaTheLoai"] != null) dgvThongKe.Columns["MaTheLoai"].Visible = false;
                if (dgvThongKe.Columns["GiaBan"] != null) dgvThongKe.Columns["GiaBan"].DefaultCellStyle.Format = "N0";
                foreach (DataGridViewRow r in dgvThongKe.Rows)
                    r.DefaultCellStyle.BackColor = Color.FromArgb(255, 230, 230);

                // Bảng lịch sử hóa đơn
                dgvHoaDon.DataSource = null;
                dgvHoaDon.DataSource = dsHD;
                SetHeaders(dgvHoaDon, new[] {
                    ("MaHoaDon","Mã HĐ"), ("NgayBan","Ngày bán"),
                    ("TongTien","Tổng tiền"), ("GhiChu","Ghi chú")
                });
                SetColumnWidths(dgvHoaDon, new Dictionary<string, int>
                {
                    { "MaHoaDon", 60  },
                    { "NgayBan",  150 },
                    { "TongTien", 110 },
                    // GhiChu → tự Fill nốt
                });
                if (dgvHoaDon.Columns["TongTien"] != null)
                    dgvHoaDon.Columns["TongTien"].DefaultCellStyle.Format = "N0";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi thống kê:\n{ex.Message}", "Lỗi",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // ============================================================
        // SỰ KIỆN TAB SÁCH
        // ============================================================
        private void DgvSach_Fill()
        {
            if (dgvSach.SelectedRows.Count == 0) return;
            if (dgvSach.SelectedRows[0].DataBoundItem is not SachDTO s) return;
            txtMaSach.Text = s.MaSach; txtMaSach.ReadOnly = true;
            txtTenSach.Text = s.TenSach;
            txtTacGia.Text = s.TacGia;
            txtGiaBan.Text = s.GiaBan.ToString("N0");
            txtSoLuong.Text = s.SoLuongTon.ToString();
            cboTheLoaiSach.SelectedValue = s.MaTheLoai;
        }

        private void BtnTimSach_Click(object sender, EventArgs e)
        {
            string field = cboTimTheo.SelectedIndex switch { 1 => "MaSach", 2 => "TenTheLoai", _ => "TenSach" };
            LoadSach(_sachBLL.Search(txtTimSach.Text.Trim(), field));
        }

        private void BtnThemSach_Click(object sender, EventArgs e)
        {
            if (!ValidateSach(out string err)) { Warn(err); return; }
            var (ok, msg) = _sachBLL.ThemSach(GetSachDTO());
            ShowResult(ok, msg);
            if (ok) { LamMoiSach(); LoadSach(); LoadTheLoaiCombo(); }
        }

        private void BtnSuaSach_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(txtMaSach.Text)) { Warn("Vui lòng chọn sách cần sửa!"); return; }
            if (!ValidateSach(out string err)) { Warn(err); return; }
            var (ok, msg) = _sachBLL.SuaSach(GetSachDTO());
            ShowResult(ok, msg);
            if (ok) { LamMoiSach(); LoadSach(); }
        }

        private void BtnXoaSach_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(txtMaSach.Text)) { Warn("Vui lòng chọn sách cần xóa!"); return; }
            if (MessageBox.Show($"Xóa sách [{txtMaSach.Text}]?", "Xác nhận", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes) return;
            var (ok, msg) = _sachBLL.XoaSach(txtMaSach.Text);
            ShowResult(ok, msg);
            if (ok) { LamMoiSach(); LoadSach(); }
        }

        // ============================================================
        // SỰ KIỆN TAB THỂ LOẠI
        // ============================================================
        private void DgvTL_Fill()
        {
            if (dgvTheLoai.SelectedRows.Count == 0) return;
            if (dgvTheLoai.SelectedRows[0].DataBoundItem is not TheLoaiDTO t) return;
            txtMaTL.Text = t.MaTheLoai.ToString();
            txtTenTL.Text = t.TenTheLoai;
            txtMoTa.Text = t.MoTa;
        }

        private void BtnThemTL_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtTenTL.Text)) { Warn("Vui lòng nhập tên thể loại!"); return; }
            var (ok, msg) = _theLoaiBLL.ThemTheLoai(new TheLoaiDTO { TenTheLoai = txtTenTL.Text.Trim(), MoTa = txtMoTa.Text.Trim() });
            ShowResult(ok, msg);
            if (ok) { LamMoiTL(); LoadTheLoai(); LoadTheLoaiCombo(); }
        }

        private void BtnSuaTL_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtMaTL.Text)) { Warn("Vui lòng chọn thể loại cần sửa!"); return; }
            var (ok, msg) = _theLoaiBLL.SuaTheLoai(new TheLoaiDTO { MaTheLoai = int.Parse(txtMaTL.Text), TenTheLoai = txtTenTL.Text.Trim(), MoTa = txtMoTa.Text.Trim() });
            ShowResult(ok, msg);
            if (ok) { LamMoiTL(); LoadTheLoai(); LoadTheLoaiCombo(); }
        }

        private void BtnXoaTL_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtMaTL.Text)) { Warn("Vui lòng chọn thể loại cần xóa!"); return; }
            if (MessageBox.Show($"Xóa thể loại [{txtTenTL.Text}]?", "Xác nhận", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes) return;
            var (ok, msg) = _theLoaiBLL.XoaTheLoai(int.Parse(txtMaTL.Text));
            ShowResult(ok, msg);
            if (ok) { LamMoiTL(); LoadTheLoai(); LoadTheLoaiCombo(); }
        }

        // ============================================================
        // SỰ KIỆN TAB BÁN HÀNG
        // ============================================================
        private void DgvSachBan_SelectionChanged(object sender, EventArgs e)
        {
            if (dgvDanhSachSach.SelectedRows.Count == 0) return;
            if (dgvDanhSachSach.SelectedRows[0].DataBoundItem is not SachDTO s) return;
            lblThongTinSach.Text = $"📖 {s.TenSach}  |  Tác giả: {s.TacGia}  |  Giá: {s.GiaBan:N0} đ  |  Tồn: {s.SoLuongTon}";
        }

        private void BtnThemVaoGio_Click(object sender, EventArgs e)
        {
            if (dgvDanhSachSach.SelectedRows.Count == 0) { Warn("Vui lòng chọn sách!"); return; }
            if (dgvDanhSachSach.SelectedRows[0].DataBoundItem is not SachDTO s) return;

            if (!int.TryParse(txtSoLuongBan.Text, out int sl) || sl <= 0)
            { Warn("Số lượng phải là số nguyên lớn hơn 0!"); return; }

            if (sl > s.SoLuongTon)
            { Warn($"Tồn kho chỉ còn {s.SoLuongTon} cuốn!"); return; }

            // Nếu sách đã có trong giỏ → cộng thêm số lượng
            var existing = _gioHang.FirstOrDefault(x => x.MaSach == s.MaSach);
            if (existing != null)
            {
                if (existing.SoLuong + sl > s.SoLuongTon)
                { Warn($"Tổng số lượng vượt tồn kho ({s.SoLuongTon})!"); return; }
                existing.SoLuong += sl;
            }
            else
            {
                _gioHang.Add(new ChiTietHoaDonDTO
                {
                    MaSach = s.MaSach,
                    TenSach = s.TenSach,
                    SoLuong = sl,
                    DonGia = s.GiaBan
                });
            }

            RefreshGioHang();
            txtSoLuongBan.Text = "1";
        }

        private void BtnXoaKhoiGio_Click(object sender, EventArgs e)
        {
            if (dgvGioHang.SelectedRows.Count == 0) { Warn("Vui lòng chọn dòng cần xóa!"); return; }
            int idx = dgvGioHang.SelectedRows[0].Index;
            if (idx >= 0 && idx < _gioHang.Count)
            {
                _gioHang.RemoveAt(idx);
                RefreshGioHang();
            }
        }

        private void BtnThanhToan_Click(object sender, EventArgs e)
        {
            if (_gioHang.Count == 0) { Warn("Giỏ hàng đang trống!"); return; }

            decimal tong = _gioHang.Sum(x => x.ThanhTien);
            var confirm = MessageBox.Show(
                $"Xác nhận thanh toán?\n\nTổng tiền: {tong:N0} đ\nSố mặt hàng: {_gioHang.Count}",
                "Xác nhận", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (confirm != DialogResult.Yes) return;

            var (ok, msg, maHD) = _hoaDonBLL.BanHang(_gioHang, txtGhiChu.Text.Trim());
            ShowResult(ok, msg);
            if (ok)
            {
                LamMoiGio();
                LoadSach();       // Cập nhật tồn kho sau khi bán
                LoadThongKe();    // Cập nhật thống kê
            }
        }

        /// <summary>Cập nhật lại DataGridView giỏ hàng và tổng tiền.</summary>
        private void RefreshGioHang()
        {
            dgvGioHang.Rows.Clear();
            foreach (var ct in _gioHang)
            {
                dgvGioHang.Rows.Add(ct.TenSach, ct.SoLuong, ct.DonGia.ToString("N0"), ct.ThanhTien.ToString("N0"));
            }
            decimal tong = _gioHang.Sum(x => x.ThanhTien);
            lblTongTien.Text = $"TỔNG TIỀN:  {tong:N0} đ";
        }

        // ============================================================
        // LÀM MỚI FORM
        // ============================================================
        private void LamMoiSach()
        {
            txtMaSach.Clear(); txtMaSach.ReadOnly = false;
            txtTenSach.Clear(); txtTacGia.Clear();
            txtGiaBan.Clear(); txtSoLuong.Clear();
            if (cboTheLoaiSach.Items.Count > 0) cboTheLoaiSach.SelectedIndex = 0;
        }

        private void LamMoiTL() { txtMaTL.Clear(); txtTenTL.Clear(); txtMoTa.Clear(); }

        private void LamMoiGio() { _gioHang.Clear(); RefreshGioHang(); txtGhiChu.Clear(); }

        // ============================================================
        // HELPER METHODS
        // ============================================================
        private SachDTO GetSachDTO() => new SachDTO
        {
            MaSach = txtMaSach.Text.Trim(),
            TenSach = txtTenSach.Text.Trim(),
            TacGia = txtTacGia.Text.Trim(),
            MaTheLoai = (int)cboTheLoaiSach.SelectedValue,
            GiaBan = decimal.Parse(txtGiaBan.Text.Replace(",", "").Replace(".", "")),
            SoLuongTon = int.Parse(txtSoLuong.Text)
        };

        private bool ValidateSach(out string err)
        {
            if (string.IsNullOrWhiteSpace(txtMaSach.Text)) { err = "Mã sách không được trống!"; return false; }
            if (string.IsNullOrWhiteSpace(txtTenSach.Text)) { err = "Tên sách không được trống!"; return false; }
            if (!decimal.TryParse(txtGiaBan.Text.Replace(",", "").Replace(".", ""), out decimal g) || g < 0) { err = "Giá bán không hợp lệ!"; return false; }
            if (!int.TryParse(txtSoLuong.Text, out int sl) || sl < 0) { err = "Số lượng không hợp lệ!"; return false; }
            err = ""; return true;
        }

        private void SetHeaders(DataGridView dgv, IEnumerable<(string col, string header)> cols)
        {
            foreach (var (col, header) in cols)
                if (dgv.Columns[col] != null) dgv.Columns[col].HeaderText = header;
        }

        private void SetStatus(string msg) => lblStatus.Text = $"  {msg}";
        private void Warn(string msg) => MessageBox.Show(msg, "Chú ý", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        private void ShowResult(bool ok, string msg) =>
            MessageBox.Show(msg, "Thông báo", MessageBoxButtons.OK, ok ? MessageBoxIcon.Information : MessageBoxIcon.Warning);

        // ---- Factory methods tạo control ----
        private DataGridView MakeDGV() => new DataGridView
        {
            Dock = DockStyle.Fill,
            ReadOnly = true,
            SelectionMode = DataGridViewSelectionMode.FullRowSelect,
            MultiSelect = false,
            AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
            AllowUserToAddRows = false,
            AllowUserToDeleteRows = false,
            BackgroundColor = Color.White,
            RowHeadersVisible = false,
            BorderStyle = BorderStyle.None,
            Font = new Font("Segoe UI", 9),
            AllowUserToResizeColumns = false,   // ← thêm dòng này: khóa kéo cột
            AllowUserToResizeRows = false,
            ColumnHeadersDefaultCellStyle = new DataGridViewCellStyle { BackColor = Color.FromArgb(41, 128, 185), ForeColor = Color.White, Font = new Font("Segoe UI", 9, FontStyle.Bold) },
            EnableHeadersVisualStyles = false
        };

        private Panel MakeInputPanel(int width) => new Panel
        { Dock = DockStyle.Right, Width = width, BackColor = Color.FromArgb(245, 246, 250), Padding = new Padding(10) };

        private Label MakeLabel(string text, int x, int y) =>
            new Label { Text = text, Left = x, Top = y + 3, AutoSize = true, Font = new Font("Segoe UI", 9) };

        private Label MakeSectionLabel(string text, int y) =>
            new Label { Text = text, Left = 10, Top = y, AutoSize = true, Font = new Font("Segoe UI", 9, FontStyle.Bold), ForeColor = Color.FromArgb(41, 128, 185) };

        private Label MakeStatLabel(string text, int x, int y) =>
            new Label { Text = text, Left = x, Top = y, AutoSize = true, Font = new Font("Segoe UI", 10, FontStyle.Bold) };

        private TextBox MakeTxt(int x, int y, int w) =>
            new TextBox { Left = x, Top = y, Width = w, Font = new Font("Segoe UI", 9) };

        private Button MakeBtn(string text, int x, int y, Color color, int w)
        {
            var b = new Button { Text = text, Left = x, Top = y, Width = w, Height = 32, BackColor = color, ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI", 9, FontStyle.Bold) };
            b.FlatAppearance.BorderSize = 0; return b;
        }

        private Button MakeSmallButton(string text, int x, int y, Color color)
        {
            var b = new Button { Text = text, Left = x, Top = y, Width = 85, Height = 28, BackColor = color, ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI", 8, FontStyle.Bold) };
            b.FlatAppearance.BorderSize = 0; return b;
        }
        /// <summary>Đặt độ rộng cột theo % tổng chiều ngang của DataGridView.</summary>
        private void SetColumnWidths(DataGridView dgv, Dictionary<string, int> widths)
        {
            // Tắt AutoSize trước khi set width thủ công
            dgv.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.None;

            foreach (var kv in widths)
                if (dgv.Columns[kv.Key] != null)
                    dgv.Columns[kv.Key].Width = kv.Value;

            // Cột cuối Fill nốt phần còn lại
            dgv.Columns[dgv.Columns.Count - 1].AutoSizeMode =
                DataGridViewAutoSizeColumnMode.Fill;
        }
    }
}