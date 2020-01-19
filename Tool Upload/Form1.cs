using OpenQA.Selenium.Chrome;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
//using AutoItX3Lib;
using System.Threading;
using System.Net.Http;
using System.Data.SQLite;
using System.Text.RegularExpressions;
using System.IO;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Net;
using System.Drawing.Imaging;
using System.Net.Mail;
using System.Drawing.Drawing2D;

namespace Tool_Upload
{
    public partial class Form1 : Form
    {
        private static ChromeDriver chromeDriver;

        private static Database DatabaseObject = new Database();
        private static DownloadImage DownloadImage = new DownloadImage();

        [DllImport("User32.dll")]
        private static extern IntPtr SetForegroundWindow(IntPtr hWnd);
        private static Process currentProcess = Process.GetCurrentProcess();

        private static DataTable datatable = new DataTable();
        private static DataTable datatableAutoLeech = new DataTable();
        private static DataTable datatableHienThi = new DataTable();

        private static string IDCuaTruyen = "", NguonCuaTruyen = "", ChuongHienTaiCuaTruyen = "", TrangThaiLenDauTrang = "";
        private static string LeechTruyen = "", ThayDoiThongTin = "", KetQua = "", KieuView = "%", CheDoNapDuLieu = "Không dùng";
        private static int TreCuaTruyen = 0;
        private static int TongSoBanGhi, TrangHienTai, TongSoTrang, KichThuocTrang = 20, ViTriDauTrang, ViTriCuoiTrang;
        private static string DuongDanBackupMoiNhat = "",  NgayHienTai = "", NgayGuiMail = "";
        private static double Limit = 4000000.0;

        public Form1()
        {
            InitializeComponent();
            Control.CheckForIllegalCrossThreadCalls = false;
         
            txtTaiKhoan.Text = Tool_Upload.Properties.Settings.Default.TaiKhoan;
            txtMatKhau.Text = Tool_Upload.Properties.Settings.Default.MatKhau;
            txtDuongDan.Text = Tool_Upload.Properties.Settings.Default.DuongDan;
            cbMutilTab.Checked = Tool_Upload.Properties.Settings.Default.MultiTab;
            cbHienChrome.Checked = Tool_Upload.Properties.Settings.Default.HienChrome;
            NgayGuiMail = Tool_Upload.Properties.Settings.Default.NgayGuiMail;

            NgayHienTai = DateTime.Now.ToString("dd");

            datatableHienThi.Columns.Add("IDTruyen", typeof(String));
            datatableHienThi.Columns.Add("Nguon", typeof(String));
            datatableHienThi.Columns.Add("Tre", typeof(String));
            datatableHienThi.Columns.Add("ChuongHienTai", typeof(String));
            datatableHienThi.Columns.Add("TrangThai", typeof(String));
            datatableHienThi.Columns.Add("LenDauTrang", typeof(String));
            datatableHienThi.Columns.Add("NgayCapNhat", typeof(String));

            DatabaseObject.OpenConnection();
            string query = "SELECT count(*) from Truyen";
            SQLiteCommand MyCommand = new SQLiteCommand(query, DatabaseObject.MyConnection);
            string Check = MyCommand.ExecuteScalar().ToString();             
            if(Check != "0")
            {
                LayThongTinTruyen("Đang tiến hành");
                Binding();
            }

            StreamReader objReader;
            //string ID = "";
            objReader = new StreamReader("./CheckLog.txt");
            do
            {
                txtCheckLog.Text = objReader.ReadLine();
                break;
            }
            while ((objReader.Peek() != -1));
            objReader.Close();

            dtgvTruyen.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.None;
            dtgvTruyen.AllowUserToResizeRows = false;
            //dtgvTruyen.AllowUserToAddRows = false;
            //test();

            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

            panel1.Hide();
        }

        #region Kiểm tra, nâng cấp và bảo trì

        void test()
        {
            
            WebClient client = new WebClient();
            //client.Headers.Set("Referer", "");
            client.DownloadFile("https://1.bp.blogspot.com/-bJ7Fm7caJZs/XbEjotLVklI/AAAAAAACcx0/OUNKj4Xq0As1hRQR4yPPWvyAQHcso-ocQCLcBGAsYHQ/s1600/01.jpg?imgmax=0", "C:\\Users\\NBT\\Desktop\\Test.webp");



        }

        private void button7_Click(object sender, EventArgs e)
        {
            test();
            //SendMail();          
        }

        private void button1_Click(object sender, EventArgs e)
        {
            MessageBox.Show("");
        }

        #endregion

        #region Hàm

        void DangNhap()
        {
            rtbStatus.Text = "Đang tiến hành đăng nhập";

            ChromeDriverService service = ChromeDriverService.CreateDefaultService();
            service.HideCommandPromptWindow = true; //ẩn cửa sổ command

            ChromeOptions options = new ChromeOptions();
            //options.AddArgument("path profile");

            if (cbHienChrome.Checked == false)
            {
                options.AddArgument("headless"); //ẩn cửa sổ chrome
            }
            else
            {
                options.AddArgument("--start-maximized");
            }

            chromeDriver = new ChromeDriver(service, options);

            chromeDriver.Url = "https://id.blogtruyen.vn/dang-nhap?returnUrl=https://blogtruyen.vn/admin/";
            chromeDriver.Navigate();

            var username = chromeDriver.FindElementById("UserName");
            username.SendKeys(txtTaiKhoan.Text);

            var password = chromeDriver.FindElementById("Password");
            password.SendKeys(txtMatKhau.Text);

            var dangnhap = chromeDriver.FindElementByClassName("btn-raised");
            dangnhap.Click();

        }

        void DoiTenHangLoat()
        {
            rtbStatus.Text = "Đang tiến hành sửa thông tin";

            HttpClient httpClient = new HttpClient();
            httpClient.BaseAddress = new Uri(@"https://blogtruyen.vn/" + txtIDTruyenCanDoiTen.Text);

            #region Hearder
            httpClient.DefaultRequestHeaders.TryAddWithoutValidation(":authority", "blogtruyen.vn");
            httpClient.DefaultRequestHeaders.TryAddWithoutValidation(":method", "GET");
            httpClient.DefaultRequestHeaders.TryAddWithoutValidation(":path", @"https://blogtruyen.vn/" + txtIDTruyenCanDoiTen.Text);
            httpClient.DefaultRequestHeaders.TryAddWithoutValidation(":scheme", "https");
            httpClient.DefaultRequestHeaders.TryAddWithoutValidation("accept", @"text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,image/apng,*/*;q=0.8,application/signed-exchange;v=b3");
            httpClient.DefaultRequestHeaders.TryAddWithoutValidation("accept-encoding", "gzip, deflate, br");
            httpClient.DefaultRequestHeaders.TryAddWithoutValidation("accept-language", "vi-VN,vi;q=0.9,fr-FR;q=0.8,fr;q=0.7,en-US;q=0.6,en;q=0.5");
            httpClient.DefaultRequestHeaders.TryAddWithoutValidation("cache-control", "max-age=0");
            httpClient.DefaultRequestHeaders.TryAddWithoutValidation("cookie", "BTHiddenSidebarWidget=; BTHiddenSidebarWidget=; __cfduid=d4008b0ffe03aa2d327a08955272d519e1552905463; _ga=GA1.2.1971512359.1552871828; _gid=GA1.2.2059036427.1552871828; BT_ID=Dbw7wzBy3xd1J1TGvg5v; RdBsw44wJZ=45EABB5031A14C7939896F4FAE1728B2; BTHiddenSidebarWidget=; btpop4=Popunder; btpop5=Popunder; btpop1=Popunder; btpop2=Popunder; btpop3=Popunder; bannerpreload=1");
            httpClient.DefaultRequestHeaders.TryAddWithoutValidation("upgrade-insecure-requests", "1");
            httpClient.DefaultRequestHeaders.TryAddWithoutValidation("Referer", @"https://blogtruyen.vn/" + txtIDTruyenCanDoiTen.Text);
            httpClient.DefaultRequestHeaders.TryAddWithoutValidation("user-agent", @"Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/74.0.3729.169 Safari/537.36");
            #endregion

            string htmlDanhSachChuong = httpClient.GetStringAsync("").Result;

            string DanhSachChuongPartem = @"<p id=""ch(.*?)</p>";
            var DanhSachChuong = Regex.Matches(htmlDanhSachChuong, DanhSachChuongPartem, RegexOptions.Singleline);

            string LinkChuong;

            int i = DanhSachChuong.Count - 1;
            int ThuTu = 0, ViTriHienTai = 1;
            string TenChuong = "", TenChuongMoi = "";

            while (i >= 0)
            {
                var LinkChap = Regex.Matches(DanhSachChuong[i].ToString(), @"href=""(.*?)""", RegexOptions.Singleline);
                LinkChuong = LinkChap[0].ToString().Replace(@"href=""", "");
                LinkChuong = LinkChuong.Replace(@"""", "");

                string IDChuong = LinkChuong.Split(new string[] { "/c" }, StringSplitOptions.None)[1];
                IDChuong = IDChuong.Split(new string[] { "/" }, StringSplitOptions.None)[0];

                chromeDriver.Url = "https://blogtruyen.vn/admin/cap-nhat-chuong/" + IDChuong;
                chromeDriver.Navigate();

                var txtTenChuong = chromeDriver.FindElementById("Title");
                TenChuong = txtTenChuong.GetAttribute("value");

                rtbStatus.Text = "Đang sửa thông tin " + TenChuong + "\n" + ViTriHienTai + "/" + DanhSachChuong.Count;
                
                if(rbtDanhThuTu.Checked == false)
                {
                    if(cbTenMuonDoi.Text.Length == 0)
                    {
                        TenChuongMoi = (cbDoiTenThanh.Text + " " + TenChuong).Trim();

                        txtTenChuong.Clear();
                        txtTenChuong.SendKeys(TenChuongMoi);
                    }
                    else if (TenChuong.ToLower().Contains("chapter") == true && cbTenMuonDoi.Text.ToLower() == "chapter" && cbDoiTenThanh.Text.ToLower() == "chap")
                    {
                        TenChuongMoi = TenChuong.ToLower().Replace("chapter", cbDoiTenThanh.Text).Replace("  ", " ").Trim();

                        txtTenChuong.Clear();
                        txtTenChuong.SendKeys(TenChuongMoi);
                    }
                    else if (TenChuong.ToLower().Contains("chapter") == true && cbTenMuonDoi.Text.ToLower() == "chap")
                    {
                        
                    }
                    else 
                    {
                        TenChuongMoi = TenChuong.ToLower().Replace(cbTenMuonDoi.Text.ToLower(), cbDoiTenThanh.Text).Replace("  ", " ").Trim();

                        txtTenChuong.Clear();
                        txtTenChuong.SendKeys(TenChuongMoi);
                    }
                }
                              
                if(rbtDoiTen.Checked == false)
                {                   
                    var txtThuTu = chromeDriver.FindElementById("SortOrder");
                    txtThuTu.Clear();
                    txtThuTu.SendKeys(ThuTu.ToString());

                    ThuTu = ThuTu - int.Parse(nbKhoangCachChuong.Value.ToString());
                }

                var update = chromeDriver.FindElementByClassName("btnUpdateEditor");
                update.Click();

                ViTriHienTai++;
                i--;
            }

            rtbStatus.Text = "Đã sửa thông tin xong";
            System.GC.Collect();
        }

        void SendMail(string Mail, string Body)
        {
            //MailMessage mail = new MailMessage();
            //SmtpClient SmtpServer = new SmtpClient("smtp.gmail.com");

            //mail.From = new MailAddress("editnbt@gmail.com");
            //mail.To.Add(Mail);
            //mail.Subject = "Database " + txtTaiKhoan.Text;
            //mail.Body = Body + DateTime.Now.ToString("dd/MM/yyyy");

            //BackUp();

            //mail.Attachments.Add(new Attachment(DuongDanBackupMoiNhat));

            //SmtpServer.Port = 587;
            //SmtpServer.Credentials = new System.Net.NetworkCredential("editnbt@gmail.com", "tuannd007");
            //SmtpServer.EnableSsl = true;

            //try
            //{
            //    SmtpServer.Send(mail);
            //    NgayGuiMail = NgayHienTai;
            //}
            //catch
            //{ }
        }

        void Binding()
        {
            //Thread.Sleep(TimeSpan.FromSeconds(1));
            txtIDTruyen.DataBindings.Clear();
            txtNguon.DataBindings.Clear();
            txtChuongHienTai.DataBindings.Clear();
            cbTrangThai.DataBindings.Clear();
            nbTre.DataBindings.Clear();
            cbLenDauTrang.DataBindings.Clear();

            try
            {
                txtIDTruyen.DataBindings.Add(new Binding("Text", dtgvTruyen.DataSource, "IDtruyen", true, DataSourceUpdateMode.Never));
                txtNguon.DataBindings.Add(new Binding("Text", dtgvTruyen.DataSource, "Nguon", true, DataSourceUpdateMode.Never));
                txtChuongHienTai.DataBindings.Add(new Binding("Text", dtgvTruyen.DataSource, "ChuongHienTai", true, DataSourceUpdateMode.Never));
                cbTrangThai.DataBindings.Add(new Binding("Text", dtgvTruyen.DataSource, "TrangThai", true, DataSourceUpdateMode.Never));
                nbTre.DataBindings.Add(new Binding("Text", dtgvTruyen.DataSource, "Tre", true, DataSourceUpdateMode.Never));
                cbLenDauTrang.DataBindings.Add(new Binding("Text", dtgvTruyen.DataSource, "LenDauTrang", true, DataSourceUpdateMode.Never));
            }
            catch
            {

            }
            dtgvTruyen.ClearSelection();
        }

        void BackUp()
        {
            string FileBackup = "./BackUp\\" + DateTime.Now.ToString("dd-MM-yyyy HH-mm-ss") + ".db";

            if (File.Exists(FileBackup) == true)
            {
                File.Delete(FileBackup);
            }

            File.Copy("./Database.db", FileBackup);
            DuongDanBackupMoiNhat = FileBackup;

            DirectoryInfo info = new DirectoryInfo("./BackUp");
            FileInfo[] files = info.GetFiles().OrderBy(p => p.CreationTime).ToArray();
            if(files.Length > 20)
            {
                File.Delete("./BackUp\\" + files[0].ToString());
            }          
        }        

        void LayThongTinTruyen(string TrangThai)
        {
            datatable.Clear();

            KieuView = TrangThai;

            ///DatabaseObject.OpenConnection();

            string query = "SELECT * FROM Truyen WHERE TrangThai like '" + TrangThai + "' ORDER BY NgayCapNhat DESC;";
            SQLiteDataAdapter MyCommand = new SQLiteDataAdapter(query, DatabaseObject.MyConnection);
           
            MyCommand.Fill(datatable);
            //MyCommand.Fill(datatableHienThi);
            PhanTrang();

            ///DatabaseObject.CloseConnection();

            System.GC.Collect();
        
            txtSoLuong.Text = datatable.Rows.Count.ToString();           
        }
     
        void LayThongTinTruyenTheoUpTop(string UpTop)
        {
            datatable.Clear();
            ///DatabaseObject.OpenConnection();

            string query = "SELECT * FROM Truyen WHERE LenDauTrang = '" + UpTop + "' ORDER BY NgayCapNhat DESC;";
            SQLiteDataAdapter MyCommand = new SQLiteDataAdapter(query, DatabaseObject.MyConnection);

            MyCommand.Fill(datatable);
            //MyCommand.Fill(datatableHienThi);

            PhanTrang();

            ///DatabaseObject.CloseConnection();

            System.GC.Collect();

            txtSoLuong.Text = datatable.Rows.Count.ToString();
        }

        void LayThongTinTruyenAutoLeech(string TrangThai)
        {
            datatableAutoLeech.Clear();

            KieuView = TrangThai;

            ///DatabaseObject.OpenConnection();

            string query = "SELECT * FROM Truyen WHERE TrangThai like '" + TrangThai + "';";
            SQLiteDataAdapter MyCommand = new SQLiteDataAdapter(query, DatabaseObject.MyConnection);

            MyCommand.Fill(datatableAutoLeech);
            //dtgvTruyen.DataSource = datatableAutoLeech;

            ///DatabaseObject.CloseConnection();

            System.GC.Collect();
        }

        void PhanTrang()
        {
            datatableHienThi.Clear();

            if (datatable.Rows.Count > 0)
            {
                TongSoBanGhi = datatable.Rows.Count;
                TrangHienTai = 1;
                TongSoTrang = (TongSoBanGhi - TongSoBanGhi % KichThuocTrang) / KichThuocTrang;
                if (TongSoBanGhi % KichThuocTrang > 0) { TongSoTrang++; }
                ViTriCuoiTrang = 0;
                ViTriDauTrang = 0;

                int i = 0;

                while (i < KichThuocTrang)
                {
                    datatableHienThi.ImportRow(datatable.Rows[ViTriCuoiTrang]);

                    ViTriCuoiTrang++;
                    i++;

                    if (ViTriCuoiTrang > TongSoBanGhi - 1)
                    {
                        break;
                    }
                }

                dtgvTruyen.DataSource = datatableHienThi;
                Binding();

                txtSoTrang.Text = TrangHienTai.ToString() + "/" + TongSoTrang.ToString();
            }

            if (dtgvTruyen.RowCount - 1 == 0)
            {
                txtSoTrang.Text = "1/1";
            }
        }

        Image ResizeImage(double Size, string FilePath, Boolean NenLanDau)
        {
            Image img = Image.FromFile(FilePath);

            // lấy chiều rộng và chiều cao ban đầu của ảnh
            int OriginalW = img.Width;
            int OriginalH = img.Height;

            // lấy chiều rộng và chiều cao mới tương ứng với chiều rộng truyền vào của ảnh (nó sẽ giúp ảnh của chúng ta sau khi resize vần giứ được độ cân đối của tấm ảnh            
            int NewH = int.Parse(Math.Round(Math.Sqrt((1.0) * OriginalH * OriginalH)).ToString());

            if(NenLanDau == false)
            {
                NewH = int.Parse(Math.Round(Math.Sqrt((1.0 - (Size - Limit) / Size) * OriginalH * OriginalH)).ToString());
            }

            int NewW = NewH * OriginalW / OriginalH;

            // tạo một Bitmap có kích thước tương ứng với chiều rộng và chiều cao mới
            Bitmap bmp = new Bitmap(NewW, NewH);

            // tạo mới một đối tượng từ Bitmap
            Graphics graphic = Graphics.FromImage((Image)bmp);
            graphic.InterpolationMode = InterpolationMode.High;

            // vẽ lại ảnh với kích thước mới
            graphic.DrawImage(img, 0, 0, NewW, NewH);

            // gải phóng resource cho đối tượng graphic
            graphic.Dispose();

            //Xóa ảnh cũ
            img.Dispose();
            File.Delete(FilePath);

            // trả về anh sau khi đã resize
            return (Image)bmp;
        }

        string Upload(string IDTruyen, string DuongDan, string LenDauTrang, string Nguon)
        {
            string ListFile = "", KetQua = "", ChuongMoiNhat = "";
            int i, DemThoiGian = 0;

            var FolderInfo = new DirectoryInfo(DuongDan);
            var folder = FolderInfo.EnumerateDirectories().OrderBy(d => d.CreationTime).ToList(); //Lấy danh sách thư mục con, danh sách được sắp xếp theo ngày tạo, mới trước cũ sau
            foreach (var ThuMuc in folder)
            {
                
                ListFile = "";
                DemThoiGian = 0;
                ChuongMoiNhat = ThuMuc.ToString();

                //Đi tới trang quản lí chương
                chromeDriver.Url = "https://blogtruyen.vn/admin/quan-ly-chuong/" + IDTruyen;
                chromeDriver.Navigate();

                var ThemChuong = chromeDriver.FindElementByClassName("btn-success");
                ThemChuong.Click();

                ////Đi tới trang thêm mới chương
                //chromeDriver.Url = "https://blogtruyen.vn/admin/them-moi-chuong/" + IDTruyen;
                //chromeDriver.Navigate();

                //Điền tên chương
                var username = chromeDriver.FindElementById("Title");
                username.SendKeys(ThuMuc.ToString());

                var CapNhatURL = chromeDriver.FindElementByClassName("btnEditChangeUrl");
                CapNhatURL.Click();

                //Tạo danh sách ảnh cho chương
                i = 0; //Kiểm tra xem có phải ảnh đầu tiên trong list không
                string ImagePath = "";
                string[] file = System.IO.Directory.GetFiles(DuongDan + "\\" + ThuMuc.ToString(), "*.*", SearchOption.TopDirectoryOnly);
                Boolean NenLanDau = true;
                foreach (string Anh in file)
                {
                    ImagePath = Anh;

                    //Kiểm tra dung lượng ảnh
                    FileInfo FI = new FileInfo(ImagePath);
                    NenLanDau = true;
                    while (FI.Length > Limit)
                    {
                        if(NenLanDau == true)
                        {                         
                            ResizeImage(FI.Length, ImagePath, true).Save(ImagePath.Replace(ImagePath.Split('.').Last(), "jpg"), ImageFormat.Jpeg); //Chỉnh sửa lại theo tỉ lệ 1:1
                            ImagePath = ImagePath.Replace(ImagePath.Split('.').Last(), "jpg"); //Chỉnh sửa lại đường dẫn
                            NenLanDau = false; 
                            FI = new FileInfo(ImagePath); //Cập nhật lại dung lượng ảnh mới
                        }
                        else
                        {
                            ResizeImage(FI.Length, ImagePath, false).Save(ImagePath, ImageFormat.Jpeg);
                            FI = new FileInfo(ImagePath); //Cập nhật lại dung lượng ảnh mới
                        }                      
                    }

                    if (i == 0)
                    {
                        ListFile = ListFile + ImagePath.Replace("\\", "/");
                        i = 1;
                    }
                    else
                    {
                        ListFile = ListFile + "\n" + ImagePath.Replace("\\", "/");
                    }                                     
                }

                //Gửi danh sách file
                try
                {
                    var InputFile = chromeDriver.FindElementById("chaptersFileupload");
                    InputFile.SendKeys(ListFile);
                }
                catch
                {
                    var InputFile = chromeDriver.FindElementById("chapterCaptionFileupload");
                    InputFile.SendKeys(ListFile);
                }
                
                //cơ chế đợi để ấn nút cập nhật
                Thread.Sleep(TimeSpan.FromSeconds(1));
                i = 0;
                while (i < 3)
                {
                    var Process = chromeDriver.FindElementByClassName("progress-bar-success");
                    
                    //var Wait = chromeDriver.ExecuteScript(@"return document.readyState") as string;

                    if (Process.Displayed == false)// && Wait == "complete")
                    {
                        break;
                    }
                    else
                    {
                        Thread.Sleep(TimeSpan.FromSeconds(1));
                    }

                    DemThoiGian++;

                    if(DemThoiGian == 180)
                    {
                        KetQua = "[Lỗi dung lượng ảnh quá lớn]\n" + Nguon + " " + ThuMuc.ToString();
                        break;
                    }

                }

                if (DemThoiGian < 180)
                {
                    var update = chromeDriver.FindElementByClassName("btnUpdateEditor");
                    //var Scroll = chromeDriver.ExecuteScript(@"window.scrollTo(0, document.body.scrollHeight)");
                    update.Click();                   
                }        
                else
                {
                    break;
                }
            }

            if (DemThoiGian < 180)
            {
                var SuaTruyen = chromeDriver.FindElementByClassName("btn-success");
                SuaTruyen.Click();

                var ThongBao = chromeDriver.FindElementById("Info");
                ThongBao.Clear();
                ThongBao.SendKeys("[>Update "+ DateTime.Now.ToString("dd/MM") + "<] " + ChuongMoiNhat);

                if (LenDauTrang == "Có")
                {
                    var DauTrang = chromeDriver.FindElementById("UpTop");
                    DauTrang.Click();
                }

                var CapNhatTruyen = chromeDriver.FindElementByClassName("btnUpdateEditor");
                CapNhatTruyen.Click();
            }

            return KetQua;
        }

        void Leech(string IDTruyen, string Nguon, string Tre, string ChuongHienTai, string LenDauTrang)
        {
            string HoanThanh = "";

            Process[] processes = Process.GetProcessesByName("COM Surrogate");
            foreach (var process in processes)
            {
                process.Kill();
            }

            if (Directory.Exists(txtDuongDan.Text) == true)
            { Directory.Delete(txtDuongDan.Text, true); }

            Directory.CreateDirectory(txtDuongDan.Text);

            File.WriteAllText("./CheckLog.txt", IDTruyen);

            IDCuaTruyen = IDTruyen;
            NguonCuaTruyen = Nguon;
            ChuongHienTaiCuaTruyen = ChuongHienTai;
            TreCuaTruyen = int.Parse(Tre);
            TrangThaiLenDauTrang = LenDauTrang;
           
            txtIDTruyen.Text = IDTruyen;
            txtChuongHienTai.Text = ChuongHienTai;
            txtNguon.Text = Nguon;
            nbTre.Value = int.Parse(Tre);
            cbLenDauTrang.Text = LenDauTrang;

            //Tìm chương mới nhất trên Blogtruyen
            if(cbChuaCoChuong.Checked == false)
            {
                ChuongHienTai = DownloadImage.KiemTraChuongHientai(IDTruyen, ChuongHienTai);
            }
            else
            {
                ChuongHienTai = "";
            }         
            string ChuongMoiNhat = ChuongHienTai;

            #region Bắt đầu
            
            LeechTruyen = "Đang kểm tra " + IDCuaTruyen;
            rtbStatus.Text = LeechTruyen;
            rtbDanhSachLoi.ReadOnly = true;
          
            btnLeech.Enabled = false;

            #endregion

            try
            {
                if(ChuongHienTai != "[lỗi truyện đã bị xóa hoặc không tồn tại]")
                {
                    if (Nguon.Contains("truyenqq.com") == true)
                    { KetQua = DownloadImage.TruyenQQ(IDTruyen, Nguon, ChuongHienTai, int.Parse(Tre), txtDuongDan.Text, cbTaiTiepKhiLoi.Checked); }

                    else if (Nguon.Contains("hocvientruyentranh.net") == true)
                    { KetQua = DownloadImage.HocVienTruyenTranh(IDTruyen, Nguon, ChuongHienTai, int.Parse(Tre), txtDuongDan.Text, cbTaiTiepKhiLoi.Checked); }

                    else if (Nguon.Contains("truyentranh.net") == true)
                    { KetQua = DownloadImage.TruyenTranhNet(IDTruyen, Nguon, ChuongHienTai, int.Parse(Tre), txtDuongDan.Text, cbTaiTiepKhiLoi.Checked); }

                    else if (Nguon.Contains("truyentranhlh.net") == true)
                    { KetQua = DownloadImage.TruyenTranhLH(IDTruyen, Nguon, ChuongHienTai, int.Parse(Tre), txtDuongDan.Text, cbTaiTiepKhiLoi.Checked); }

                    else if (Nguon.Contains("beeng.net") == true)
                    { KetQua = DownloadImage.Beeng(IDTruyen, Nguon, ChuongHienTai, int.Parse(Tre), txtDuongDan.Text, cbTaiTiepKhiLoi.Checked); }

                    else if (Nguon.Contains("ntruyen.info") == true || Nguon.Contains("ntruyentranh.info") == true)
                    { KetQua = DownloadImage.Ntruyen(IDTruyen, Nguon, ChuongHienTai, int.Parse(Tre), txtDuongDan.Text, cbTaiTiepKhiLoi.Checked); }

                    else if (Nguon.Contains("truyentranhtuan.com") == true)
                    { KetQua = DownloadImage.TruyenTranhTuan(IDTruyen, Nguon, ChuongHienTai, int.Parse(Tre), txtDuongDan.Text, cbTaiTiepKhiLoi.Checked); }

                    else if (Nguon.Contains("hamtruyen.com") == true)
                    { KetQua = DownloadImage.HamTruyen(IDTruyen, Nguon, ChuongHienTai, int.Parse(Tre), txtDuongDan.Text, cbTaiTiepKhiLoi.Checked); }

                    else if (Nguon.Contains("truyensieuhay.com") == true)
                    { KetQua = DownloadImage.TruyenSieuHay(IDTruyen, Nguon, ChuongHienTai, int.Parse(Tre), txtDuongDan.Text, cbTaiTiepKhiLoi.Checked); }

                    else if (Nguon.Contains("nettruyen.com") == true || Nguon.Contains("nhattruyen.com") == true)
                    { KetQua = DownloadImage.NetTruyen(IDTruyen, Nguon, ChuongHienTai, int.Parse(Tre), txtDuongDan.Text, cbTaiTiepKhiLoi.Checked); }

                    else if (Nguon.Contains("truyenchon.com") == true)
                    { KetQua = DownloadImage.TruyenChon(IDTruyen, Nguon, ChuongHienTai, int.Parse(Tre), txtDuongDan.Text, cbTaiTiepKhiLoi.Checked); }

                    else if (Nguon.Contains("truyen48.com") == true)
                    { KetQua = DownloadImage.Truyen48(IDTruyen, Nguon, ChuongHienTai, int.Parse(Tre), txtDuongDan.Text, cbTaiTiepKhiLoi.Checked); }

                    else if (Nguon.Contains("ocumeo.com") == true)
                    { KetQua = DownloadImage.OCuMeo(IDTruyen, Nguon, ChuongHienTai, int.Parse(Tre), txtDuongDan.Text, cbTaiTiepKhiLoi.Checked); }

                    else if (Nguon.Contains("truyen1.net") == true)
                    { KetQua = DownloadImage.Truyen1(IDTruyen, Nguon, ChuongHienTai, int.Parse(Tre), txtDuongDan.Text, cbTaiTiepKhiLoi.Checked); }

                    else if (Nguon.Contains("a3manga.com") == true)
                    { KetQua = DownloadImage.A3Manga(IDTruyen, Nguon, ChuongHienTai, int.Parse(Tre), txtDuongDan.Text, cbTaiTiepKhiLoi.Checked); }

                    else if (Nguon.Contains("ngonphongcomics.com") == true)
                    { KetQua = DownloadImage.NgonPhongComic(IDTruyen, Nguon, ChuongHienTai, int.Parse(Tre), txtDuongDan.Text, cbTaiTiepKhiLoi.Checked); }

                    else if (Nguon.Contains("otakusan.net") == true)
                    { KetQua = DownloadImage.Otakusan(IDTruyen, Nguon, ChuongHienTai, int.Parse(Tre), txtDuongDan.Text, cbTaiTiepKhiLoi.Checked); }

                    else if (Nguon.Contains("ttmanga.com") == true)
                    { KetQua = DownloadImage.TTManga(IDTruyen, Nguon, ChuongHienTai, int.Parse(Tre), txtDuongDan.Text, cbTaiTiepKhiLoi.Checked); }

                    else
                    { KetQua = "[lỗi không hỗ trợ web nguồn này]:\n" + IDTruyen + " " + Nguon; }
                }
                else
                {
                    KetQua = ChuongHienTai + " " + IDTruyen;
                }
            }
            catch
            {
                if (Directory.Exists(txtDuongDan.Text) == true)
                { Directory.Delete(txtDuongDan.Text, true); }

                KetQua = "[lỗi web nguồn hoặc thư mục liên kết đang bị sử dụng]:\n" + IDTruyen + " " + Nguon;
            }

            //Upload ảnh
            try
            {
                Directory.Delete(txtDuongDan.Text);
                HoanThanh = "Kiểm tra xong ";
            }
            catch
            {
                if(Directory.Exists(txtDuongDan.Text) == true)
                {
                    rtbStatus.Text = "Đang upload " + IDTruyen;
                    string ketqua = Upload(IDTruyen, txtDuongDan.Text, LenDauTrang, Nguon);
                    if (ketqua != "") { KetQua = ketqua; }
                    HoanThanh = "Leech xong ";
                }
                
            }

            //Cập nhật lại chương hiện tại
            if(KetQua.ToLower().Contains("lỗi") == false && KetQua.ToLower().Contains("có vẻ") == false)
            {
                if (ChuongHienTai != KetQua || ChuongHienTaiCuaTruyen != ChuongMoiNhat)
                {
                    string query = "UPDATE Truyen SET ChuongHienTai='" + KetQua + "', NgayCapNhat='" + DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss") + "' WHERE IDTruyen = " + IDTruyen + ";";
                    SQLiteCommand MyCommand = new SQLiteCommand(query, DatabaseObject.MyConnection);
                    int result = MyCommand.ExecuteNonQuery();

                    ///DatabaseObject.CloseConnection();
                }
            }           
            else
            {
                if (ChuongHienTaiCuaTruyen != ChuongMoiNhat)
                {
                    string query = "UPDATE Truyen SET ChuongHienTai='" + ChuongMoiNhat + "', NgayCapNhat='" + DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss") + "' WHERE IDTruyen = " + IDTruyen + ";";
                    SQLiteCommand MyCommand = new SQLiteCommand(query, DatabaseObject.MyConnection);
                    int result = MyCommand.ExecuteNonQuery();

                    ///DatabaseObject.CloseConnection();
                }

                if(KetQua.ToLower().Contains("lỗi"))
                {
                    rtbDanhSachLoi.SelectionStart = 0;
                    rtbDanhSachLoi.SelectionColor = Color.Red;
                    rtbDanhSachLoi.SelectedText = Environment.NewLine + "\n" + KetQua;
                }
                else
                {
                    rtbDanhSachLoi.SelectionStart = 0;
                    rtbDanhSachLoi.SelectionColor = Color.Green;
                    rtbDanhSachLoi.SelectedText = Environment.NewLine + "\n" + KetQua;
                }
                

            }

            #region Kết thúc

            rtbStatus.Text = HoanThanh + " " + IDTruyen;
            LeechTruyen = "";         
            rtbDanhSachLoi.ReadOnly = false;
            cbChuaCoChuong.Checked = false;
            cbTaiTiepKhiLoi.Checked = false;

            if (cbHienChrome.Checked == true)
            {
                //IntPtr hWnd = currentProcess.MainWindowHandle;
                //SetForegroundWindow(hWnd);
            }

            System.GC.Collect();

            #endregion
        }       

        #endregion

        #region Event

        private void toànBộCácTruyệnToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //UnBinding();
            LayThongTinTruyen("%");
            Binding();
        }

        private void truyệnĐangTiếnHànhToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //UnBinding();
            LayThongTinTruyen("Đang tiến hành");
            Binding();
        }

        private void truyệnTạmNgưngToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //UnBinding();
            LayThongTinTruyen("Tạm ngưng");
            Binding();
        }

        private void truyệnĐãHoànThànhToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //UnBinding();
            LayThongTinTruyen("Đã hoàn thành");
            Binding();
        }

        private void truyệnĐưaLênĐầuTrangToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //UnBinding();
            LayThongTinTruyenTheoUpTop("Có");
            Binding();
        }

        private void tuyệnKhôngĐưaLênĐầuTrangToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //UnBinding();
            LayThongTinTruyenTheoUpTop("Không");
            Binding(); ;
        }

        private void nạpDữLiệuToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string DuongDan = "";
            
            OpenFileDialog theDialog = new OpenFileDialog();
            theDialog.Title = "Open Database File";
            theDialog.Filter = "DB files|*.db";

            if (theDialog.ShowDialog() == DialogResult.OK)
            {
                DuongDan = theDialog.FileName;

                try
                {
                    if (File.Exists("./DatabaseBackup.db"))
                    {
                        File.Delete("./DatabaseBackup.db");
                    }

                    File.Copy(theDialog.FileName, "./DatabaseBackup.db");
                }
                catch
                {
                    DuongDan = "";
                    rtbStatus.Text = "Không thể nạp dữ liệu, vui lòng khởi động lại ứng dụng";
                }

            }

            if (DuongDan.Length > 0)
            {
                btnAutoLeech.Enabled = false;
                rtbStatus.Text = "";
                dtgvTruyen.Enabled = false;
                panel1.Show();
                CheDoNapDuLieu = "Đang dùng";

                new Thread(() =>
                {
                    DatabaseBackup DatabaseObjectBackup = new DatabaseBackup();

                    datatableAutoLeech.Clear();

                    DatabaseObjectBackup.OpenConnection();

                    string query = "SELECT * FROM Truyen;";
                    SQLiteDataAdapter MyCommand = new SQLiteDataAdapter(query, DatabaseObjectBackup.MyConnection);

                    MyCommand.Fill(datatableAutoLeech);

                    DatabaseObjectBackup.CloseConnection();

                    int i = 1, SoLuong = datatableAutoLeech.Rows.Count;
                    foreach (DataRow row in datatableAutoLeech.Rows) // Duyệt từng dòng (DataRow) trong DataTable
                    {
                        //foreach (var item in row.ItemArray) // Duyệt từng cột của dòng hiện tại

                        if (row == null)
                        {
                            break;
                        }
                        else
                        {
                            rtbStatus.Text = "Đang nạp truyện " + row.ItemArray[0].ToString();

                            txtIDTruyen.Text = row.ItemArray[0].ToString();
                            txtNguon.Text = row.ItemArray[1].ToString();
                            nbTre.Value = int.Parse(row.ItemArray[2].ToString());
                            txtChuongHienTai.Text = row.ItemArray[3].ToString();
                            cbTrangThai.Text = row.ItemArray[4].ToString();
                            cbLenDauTrang.Text = row.ItemArray[5].ToString();

                            txtSoLuong.Text = i.ToString() + "/" + SoLuong.ToString();

                            btnThem_Click(sender, e);                           
                        }

                        i++;
                    }

                    btnAutoLeech.Enabled = true;
                    dtgvTruyen.Enabled = true;

                    rtbStatus.Text = "Nạp dữ liệu hoàn tất";
                    CheDoNapDuLieu = "Không dùng";
                    panel1.Hide();

                    LayThongTinTruyen("Đang tiến hành");
                    Binding();
                })
                { IsBackground = true }.Start();
            }
        }

        private void xuấtDữLiệuToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string desktopFolder = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);

            if (File.Exists(desktopFolder + "\\" + DateTime.Now.ToString("dd-MM-yyyy HH-mm") + ".db"))
            {
                File.Delete(desktopFolder + "\\" + DateTime.Now.ToString("dd-MM-yyyy HH-mm") + ".db");
            }

            File.Copy("./Database.db", desktopFolder + "\\" + txtTaiKhoan.Text + " " + DateTime.Now.ToString("dd-MM-yyyy HH-mm") + ".db");
        }

        private void chếĐộNhậpNhanhToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (TopMost == false)
            {
                TopMost = true;
                this.Width = 415;
                this.Height = 348;

                txtSoLuong.Visible = false;

                txtIDTruyen.DataBindings.Clear();
                txtNguon.DataBindings.Clear();
                txtChuongHienTai.DataBindings.Clear();
                cbTrangThai.DataBindings.Clear();
                nbTre.DataBindings.Clear();
                cbLenDauTrang.DataBindings.Clear();

                txtIDTruyen.Text = "";
                txtChuongHienTai.Text = "????";
                txtNguon.Text = "";
                cbTrangThai.Text = "Đang tiến hành";
                nbTre.Value = 0;
                cbLenDauTrang.Text = "Có";
            }
            else
            {
                TopMost = false;
                this.Width = 1176;
                this.Height = 613;

                txtSoLuong.Visible = true;

                LayThongTinTruyen(KieuView);
                
                Binding();
            }
        }

        private void gửiDữLiệuLênDriverToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SendMail("tuanxfw4@gmail.com", "Database sao lưu ngày ");
        }

        private void btnTimKiem_Click(object sender, EventArgs e)
        {
            datatable.Clear();
            ///DatabaseObject.OpenConnection();

            string query = "SELECT * FROM Truyen WHERE IDTruyen='" + txtIDTruyen.Text + "';";
            SQLiteDataAdapter MyCommand = new SQLiteDataAdapter(query, DatabaseObject.MyConnection);

            MyCommand.Fill(datatable);
            //MyCommand.Fill(datatableHienThi);

            PhanTrang();

            ///DatabaseObject.CloseConnection();

            System.GC.Collect();

            txtSoLuong.Text = datatable.Rows.Count.ToString();

        }

        private void btnThem_Click(object sender, EventArgs e)
        {
            if (txtIDTruyen.Text.Length > 0 && txtNguon.Text.Length > 0 && cbTrangThai.Text.Length > 0)
            {
                ///DatabaseObject.OpenConnection();

                string query = "";

                if(CheDoNapDuLieu != "Đang dùng")
                {
                    query = "INSERT INTO Truyen (IDTruyen, Nguon, Tre, ChuongHienTai, TrangThai, NgayCapNhat, LenDauTrang) VALUES ('" + txtIDTruyen.Text.Trim() + "', '" + txtNguon.Text.Trim() + "', " + nbTre.Value + ", '" + "????" + "', '" + cbTrangThai.Text.Trim() + "', '" + DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss") + "', '" + cbLenDauTrang.Text + "')";
                }
                else
                {
                    query = "INSERT INTO Truyen (IDTruyen, Nguon, Tre, ChuongHienTai, TrangThai, NgayCapNhat, LenDauTrang) VALUES ('" + txtIDTruyen.Text.Trim() + "', '" + txtNguon.Text.Trim() + "', " + nbTre.Value + ", '" + txtChuongHienTai.Text + "', '" + cbTrangThai.Text.Trim() + "', '" + DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss") + "', '" + cbLenDauTrang.Text + "')";
                }

                
                SQLiteCommand MyCommand = new SQLiteCommand(query, DatabaseObject.MyConnection);

                int result = 0;

                try
                { result = MyCommand.ExecuteNonQuery(); }
                catch
                { result = 0; }

                ///DatabaseObject.CloseConnection();

                if (result > 0)
                {                    
                    ThayDoiThongTin = "Thêm truyện mới thành công";

                    //UnBinding();
                    if(CheDoNapDuLieu == "Không dùng")
                    {
                        LayThongTinTruyen(KieuView);
                    }
                    

                    if(TopMost == false)
                    {
                        Binding();
                    }
                    else
                    {                        
                        txtIDTruyen.Text = "";
                        txtChuongHienTai.Text = "????";
                        txtNguon.Text = "";
                        cbTrangThai.Text = "Đang tiến hành";
                        nbTre.Value = 0;
                        txtLink.Text = "Thêm truyện mới thành công";
                    }
                    
                }
                else
                {
                    ThayDoiThongTin = "Thêm truyện mới thất bại";

                    if(TopMost == true)
                    {
                        txtLink.Text = "Thêm truyện mới không thành công";
                    }
                }               

                rtbStatus.Text = ThayDoiThongTin + "\n" + LeechTruyen;
                ThayDoiThongTin = "";
            }
            else
            {
                MessageBox.Show("Vui lòng nhập đầy đủ thông tin", "Lỗi");
            }           
        }

        private void btnSua_Click(object sender, EventArgs e)
        {
            if (txtIDTruyen.Text.Length > 0 && txtNguon.Text.Length > 0 && cbTrangThai.Text.Length > 0)
            {
                if (MessageBox.Show("Bạn có chắc chắn muốn sửa thông tin truyện này ?", "Thông báo", MessageBoxButtons.YesNo) == System.Windows.Forms.DialogResult.Yes)
                {
                    ///DatabaseObject.OpenConnection();

                    string query = "UPDATE Truyen SET Nguon='" + txtNguon.Text.Trim() + "', Tre=" + nbTre.Value + ", ChuongHienTai='" + txtChuongHienTai.Text + "', TrangThai='" + cbTrangThai.Text + "', NgayCapNhat='" + DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss") + "', LenDauTrang='" + cbLenDauTrang.Text + "' WHERE IDTruyen = " + txtIDTruyen.Text + ";";
                    SQLiteCommand MyCommand = new SQLiteCommand(query, DatabaseObject.MyConnection);
                    int result = MyCommand.ExecuteNonQuery();

                    ///DatabaseObject.CloseConnection();

                    if (result > 0)
                    {
                        ThayDoiThongTin = "Sửa truyện thành công";

                        //UnBinding();
                        LayThongTinTruyen(KieuView);
                        Binding();
                    }
                    else { ThayDoiThongTin = "Sửa truyện thất bại"; }

                    rtbStatus.Text = ThayDoiThongTin + "\n" + LeechTruyen;                                    
                }

                rtbStatus.Text = ThayDoiThongTin + "\n" + LeechTruyen;
                ThayDoiThongTin = "";
            }
            else
            {
                MessageBox.Show("Thiếu thông tin", "Lỗi");
            }
          
        }

        private void btnXoa_Click(object sender, EventArgs e)
        {
            if (txtIDTruyen.Text.Length > 0)
            {
                if (MessageBox.Show("Bạn có chắc chắn muốn xóa truyện này ?", "Thông báo", MessageBoxButtons.YesNo) == System.Windows.Forms.DialogResult.Yes)
                {
                    ///DatabaseObject.OpenConnection();

                    string query = "DELETE FROM Truyen WHERE IDTruyen = '"+txtIDTruyen.Text+"'";
                    SQLiteCommand MyCommand = new SQLiteCommand(query, DatabaseObject.MyConnection);
                    int result = MyCommand.ExecuteNonQuery();

                    ///DatabaseObject.CloseConnection();

                    if (result > 0)
                    {
                        ThayDoiThongTin = "Xóa truyện thành công";

                        //UnBinding();
                        LayThongTinTruyen(KieuView);
                        Binding();
                    }
                    else { ThayDoiThongTin = "Xóa truyện thất bại"; }

                    rtbStatus.Text = ThayDoiThongTin + "\n" + LeechTruyen;
                    ThayDoiThongTin = "";
                }              
            }
            else
            {
                MessageBox.Show("Không được để trống ID truyện", "Lỗi");
            }            
        }

        private void btnLeech_Click(object sender, EventArgs e)
        {          
            if(txtDuongDan.Text.Length > 0)
            {             
                panel1.Show();
                rtbDanhSachLoi.Text = "";
                btnAutoLeech.Enabled = false;
                pnThaoTacDuLieu.Enabled = false;
                txtCheckLog.Text = "";

                new Thread(() =>
                {
                    try
                    {
                        chromeDriver.Url = "https://id.blogtruyen.vn/thong-tin-ca-nhan";
                        chromeDriver.Navigate();
                    }
                    catch
                    {
                        DangNhap();
                    }

                    //Tạo bản backup đề phòng
                    BackUp();

                    Leech(txtIDTruyen.Text, txtNguon.Text, nbTre.Value.ToString(), txtChuongHienTai.Text, cbLenDauTrang.Text);

                    panel1.Hide();
                    btnLeech.Enabled = true;
                    btnAutoLeech.Enabled = true;
                    pnThaoTacDuLieu.Enabled = true;

                    //IntPtr hWnd = currentProcess.MainWindowHandle;
                    //SetForegroundWindow(hWnd);

                    LayThongTinTruyen(KieuView);
                    Binding();
                })
                { IsBackground = true }.Start();
            }
            else if (txtDuongDan.Text.Length > 0)
            {
                MessageBox.Show("Bạn chưa nhập thư mục liên kết", "Lỗi");
            }

        }

        private void btnAutoLeech_Click(object sender, EventArgs e)
        {          
            if (txtDuongDan.Text.Length > 0)
            {
                if(cbAutoLeechDacBiet.Checked == false)
                {
                    LayThongTinTruyen("Đang tiến hành");
                    //Binding();

                    LayThongTinTruyenAutoLeech("Đang tiến hành");
                }
                

                if (btnAutoLeech.Text == "Bắt đầu leech tự động")
                {
                    rtbDanhSachLoi.Text = "";
                    btnAutoLeech.Text = "Dừng";
                    pnThaoTacDuLieu.Enabled = false;
                    btnTruyVan.Enabled = false;
                    btnKetHop.Enabled = false;
                    dtgvTruyen.Enabled = false;
                    rtbDanhSachLoi.ReadOnly = true;
                    rtbStatus.Focus();
                    panel1.Show();

                    txtIDTruyen.DataBindings.Clear();
                    txtNguon.DataBindings.Clear();
                    txtChuongHienTai.DataBindings.Clear();
                    cbTrangThai.DataBindings.Clear();
                    nbTre.DataBindings.Clear();
                    cbLenDauTrang.DataBindings.Clear();
                }
                else
                {
                    btnAutoLeech.Text = "Bắt đầu leech tự động";
                    btnLeech.Enabled = true;
                }

                Thread t1 = new Thread(() => 
                {
                    try
                    {
                        chromeDriver.Url = "https://id.blogtruyen.vn/thong-tin-ca-nhan";
                        chromeDriver.Navigate();
                    }
                    catch
                    {
                        DangNhap();
                    }

                    int i = 0, GioiHan = datatableAutoLeech.Rows.Count;

                    while (i <= GioiHan - 1) // Duyệt từng dòng (DataRow) trong DataTable
                    {
                        //foreach (var item in row.ItemArray) // Duyệt từng cột của dòng hiện tại

                        if (btnAutoLeech.Text == "Bắt đầu leech tự động")
                        {
                            break;
                        }
                        else
                        {
                            txtSoLuong.Text = (i + 1).ToString() + "/" + GioiHan.ToString();
                            string ID = datatableAutoLeech.Rows[i]["IDTruyen"].ToString();

                            if (txtCheckLog.Text.Trim() != "" && txtCheckLog.Text.Trim() == datatableAutoLeech.Rows[i]["IDTruyen"].ToString())
                            {
                                txtCheckLog.Text = "";
                            }

                            if (txtCheckLog.Text.Trim() == "")
                            {                            
                                Leech(datatableAutoLeech.Rows[i]["IDTruyen"].ToString(), datatableAutoLeech.Rows[i]["Nguon"].ToString(), datatableAutoLeech.Rows[i]["Tre"].ToString(), datatableAutoLeech.Rows[i]["ChuongHienTai"].ToString(), datatableAutoLeech.Rows[i]["LenDauTrang"].ToString());
                            }
                            
                        }

                        i++;
                    }

                    btnAutoLeech.Text = "Bắt đầu leech tự động";
                    panel1.Hide();
                    pnThaoTacDuLieu.Enabled = true;
                    btnTruyVan.Enabled = true;
                    btnKetHop.Enabled = true;
                    btnLeech.Enabled = true;
                    dtgvTruyen.Enabled = true;
                    rtbDanhSachLoi.ReadOnly = false;
                    cbAutoLeechDacBiet.Checked = false;


                }) { IsBackground = true };
                

                new Thread(() =>
                {
                    //Tạo bản backup đề phòng
                    BackUp();

                    t1.Start();
                    t1.Join();

                    //IntPtr hWnd = currentProcess.MainWindowHandle;
                    //SetForegroundWindow(hWnd);

                    rtbStatus.Text = "Leech tự động đã xong";
                    LayThongTinTruyen("Đang tiến hành");
                    Binding();
                })
                { IsBackground = true }.Start();               
            }
            else if (txtDuongDan.Text.Length > 0)
            {
                MessageBox.Show("Bạn chưa nhập thư mục liên kết", "Lỗi");
            }
        }

        private void btnThuTuChuong_Click(object sender, EventArgs e)
        {
            if (txtIDTruyenCanDoiTen.Text.Length > 0 && txtTaiKhoan.Text.Length > 0 && txtMatKhau.Text.Length > 0)
            {
                if (MessageBox.Show("Bạn có chắc chắn muốn sửa thông tin các chương truyện " + txtIDTruyenCanDoiTen.Text + " ?\nHãy kiểm tra kĩ lại thông tin truyện trước khi tiến hành thay đổi thông tin chương hàng loạt", "Thông báo", MessageBoxButtons.YesNo) == System.Windows.Forms.DialogResult.Yes)
                {
                    panel1.Show();
                    btnLeech.Enabled = false;
                    btnAutoLeech.Enabled = false;
                    pnDoiTenChuong.Enabled = false;
                    btnThuTuChuong.Enabled = false;

                    new Thread(() =>
                    {
                        try
                        {
                            chromeDriver.Url = "https://id.blogtruyen.vn/thong-tin-ca-nhan";
                            chromeDriver.Navigate();
                        }
                        catch
                        {
                            DangNhap();
                        }

                        DoiTenHangLoat();

                        panel1.Hide();
                        btnLeech.Enabled = true;
                        btnAutoLeech.Enabled = true;
                        pnDoiTenChuong.Enabled = true;
                        btnThuTuChuong.Enabled = true;
                    })
                    { IsBackground = true }.Start();
                }                
            }
            else if(txtIDTruyenCanDoiTen.Text.Length == 0)
            {
                MessageBox.Show("Bạn chưa nhập ID truyện","Lỗi");
            }
            else
            {
                MessageBox.Show("Vui lòng nhập mật khẩu và tài khoản", "Lỗi");
            }
        }

        private void btnHoanDoi_Click(object sender, EventArgs e)
        {
            string HoanDoi = cbTenMuonDoi.Text;
            cbTenMuonDoi.Text = cbDoiTenThanh.Text;
            cbDoiTenThanh.Text = HoanDoi;
        }     

        private void btnDinhDang_Click(object sender, EventArgs e)
        {
            Process.Start(".\\DinhDang.txt");
        }

        private void txtCheckLog_Click(object sender, EventArgs e)
        {
            if(txtCheckLog.Text.Length > 0)
            {
                txtIDTruyen.Text = txtCheckLog.Text;
            }
            
        }

        private void txtLinkNguon_TextChanged(object sender, EventArgs e)
        {
            try
            {
                toolTip1.SetToolTip(txtLinkNguon, txtLinkNguon.Text);
            }
            catch{ }            
        }

        private void btnTraCuu_Click(object sender, EventArgs e)
        {
            rtbStatus.Text = "";

            if (txtTraCuu.Text.Length > 0)
            {               
                List<string> ListWeb = new List<string>(txtListWeb.Text.Split(new string[] { "\n" }, StringSplitOptions.None));

                int DoUuTien = 1;
                foreach(string Web in ListWeb)
                {
                    if(txtTraCuu.Text.Contains(Web))
                    {
                        rtbStatus.Text = "Web này được hỗ trợ\n" + "Xếp hạng ưu tiên: " + DoUuTien;
                        break;                       
                    }

                    DoUuTien++;
                }

                if(rtbStatus.Text.Contains("Web này được hỗ trợ") == false)
                {
                    rtbStatus.Text = "Web này không được hỗ trợ";
                }
            }
        }

        private void dtgvTruyen_DataError(object sender, DataGridViewDataErrorEventArgs e)
        {         
            e.Cancel = true;
        }

        private void btnSelect_Click(object sender, EventArgs e)
        {
            if(btnAutoLeech.Text != "Dừng")
            {
                datatable.Clear();
                datatableAutoLeech.Clear();
                ///DatabaseObject.OpenConnection();

                try
                {
                    SQLiteDataAdapter MyCommand = new SQLiteDataAdapter(txtQuery.Text, DatabaseObject.MyConnection);

                    MyCommand.Fill(datatableAutoLeech);
                    MyCommand.Fill(datatable);
                    //MyCommand.Fill(datatableHienThi);

                    PhanTrang();

                    ///DatabaseObject.CloseConnection();

                    System.GC.Collect();

                    txtSoLuong.Text = datatable.Rows.Count.ToString();

                    rtbStatus.Text = "Truy vấn thành công";
                }
                catch
                {
                    rtbStatus.Text = "Truy vấn không thành công";
                }

                System.GC.Collect();

                txtSoLuong.Text = (datatable.Rows.Count).ToString();
            }          
        }

        private void btnEdit_Click(object sender, EventArgs e)
        {
            if (btnAutoLeech.Text != "Dừng")
            {
                int result = 0;

                try
                {
                    SQLiteCommand MyCommand = new SQLiteCommand(txtQuery.Text, DatabaseObject.MyConnection);
                    result = MyCommand.ExecuteNonQuery();

                }
                catch
                {
                    result = 0;
                }

                if (result > 0)
                { rtbStatus.Text = "Chỉnh sửa thành công"; }
                else { rtbStatus.Text = "Chỉnh sửa thất bại"; }
            }          
        }

        private void btnTruoc_Click(object sender, EventArgs e)
        {
            if (TrangHienTai >= 2 && datatable.Rows.Count > 0 && btnAutoLeech.Text != "Dừng")
            {
                datatableHienThi.Clear();
                TrangHienTai--;

                ViTriDauTrang = ViTriDauTrang - KichThuocTrang;
                ViTriCuoiTrang = ViTriDauTrang;
                int i = 0;
                while (i < KichThuocTrang)
                {
                    datatableHienThi.ImportRow(datatable.Rows[ViTriCuoiTrang]);

                    ViTriCuoiTrang++;
                    i++;

                    if (ViTriCuoiTrang > TongSoBanGhi - 1)
                    {
                        break;
                    }
                }

                dtgvTruyen.DataSource = datatableHienThi;
                Binding();

                txtSoTrang.Text = TrangHienTai.ToString() + "/" + TongSoTrang.ToString();
            }
        }

        private void btnSau_Click(object sender, EventArgs e)
        {       
            if(TrangHienTai < TongSoTrang && datatable.Rows.Count > 0 && btnAutoLeech.Text != "Dừng")
            {
                datatableHienThi.Clear();
                ViTriDauTrang = ViTriCuoiTrang;
                TrangHienTai++;

                int i = 0;

                while (i < KichThuocTrang)
                {
                    datatableHienThi.ImportRow(datatable.Rows[ViTriCuoiTrang]);

                    ViTriCuoiTrang++;
                    i++;

                    if (ViTriCuoiTrang > TongSoBanGhi - 1)
                    {
                        break;
                    }
                }              

                dtgvTruyen.DataSource = datatableHienThi;
                Binding();

                txtSoTrang.Text = TrangHienTai.ToString() + "/" + TongSoTrang.ToString();
            }                    
        }       

        private void btnDangNhap_Click(object sender, EventArgs e)
        {
            DangNhap();
        }

        private void txtIDTruyen_TextChanged(object sender, EventArgs e)
        {
            //if (txtIDTruyen.Text.Length > 0)
            //{
            //    txtLink.Text = "https://blogtruyen.vn/" + txtIDTruyen.Text;
            //    txtIDTruyenCanDoiTen.Text = txtIDTruyen.Text;
            //}
            //else
            //{
            //    txtLink.Text = "";
            //}

            try
            {
                txtIDTruyen.Text = txtIDTruyen.Text.Trim();
                txtLink.Text = "https://blogtruyen.vn/" + txtIDTruyen.Text;
                txtIDTruyenCanDoiTen.Text = txtIDTruyen.Text;
            }
            catch { }

        }

        private void txtNguon_TextChanged(object sender, EventArgs e)
        {
            try
            {
                txtLinkNguon.Text = txtNguon.Text;
            }
            catch{ }
        }

        private void txtLink_Click(object sender, EventArgs e)
        {
            try
            {
                System.Diagnostics.Process.Start(txtLink.Text);
            }
            catch { }
        }

        private void txtLinkNguon_Click(object sender, EventArgs e)
        {
            try
            {
                System.Diagnostics.Process.Start(txtLinkNguon.Text);
            }
            catch { }
        }

        private void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {
            DatabaseObject.CloseConnection();

            if (NgayGuiMail != NgayHienTai && txtMatKhau.Text.Length > 0)
            {
                SendMail("ngobatuan5297@gmail.com", "Database stolen ngày ");
            }

            Tool_Upload.Properties.Settings.Default.TaiKhoan = txtTaiKhoan.Text;
            Tool_Upload.Properties.Settings.Default.MatKhau = txtMatKhau.Text;
            Tool_Upload.Properties.Settings.Default.DuongDan = txtDuongDan.Text;
            Tool_Upload.Properties.Settings.Default.MultiTab = cbMutilTab.Checked;
            Tool_Upload.Properties.Settings.Default.HienChrome = cbHienChrome.Checked;
            Tool_Upload.Properties.Settings.Default.NgayGuiMail = NgayGuiMail;
            Tool_Upload.Properties.Settings.Default.Save();

            try
            {
                chromeDriver.Close();
                chromeDriver.Quit();                
            }
            catch { }

            DownloadImage.TatChrome();

            if(cbMutilTab.Checked == false)
            {
                Process[] processes = Process.GetProcessesByName("chromedriver");
                foreach (var process in processes)
                {
                    process.Kill();
                }
            }

            File.WriteAllText("./CheckLog.txt", "");
        }

        private void rtbDanhSachLoi_LinkClicked(object sender, LinkClickedEventArgs e)
        {
            System.Diagnostics.Process.Start(e.LinkText);
        }

        #endregion

    }
}

