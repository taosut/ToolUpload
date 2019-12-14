using OpenQA.Selenium.Chrome;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;


namespace Tool_Upload
{
    public class DownloadImage
    {
        static List<string> ListDinhDang = new List<string>();
        static List<string> ListHanChe = new List<string>();
        static ChromeDriver chromeDriver;

        public DownloadImage()
        {
            #region Tạo danh sách hạn chế

            ListHanChe.Add(":");
            ListHanChe.Add("\\");
            ListHanChe.Add("?");
            ListHanChe.Add("/");
            ListHanChe.Add("|");
            ListHanChe.Add("<");
            ListHanChe.Add(">");
            ListHanChe.Add(@"""");
            ListHanChe.Add(@"'");

            #endregion

            #region Khởi tạo chrome

            ChromeDriverService service = ChromeDriverService.CreateDefaultService();
            service.HideCommandPromptWindow = true; //ẩn cửa sổ command

            ChromeOptions options = new ChromeOptions();
            options.AddArgument("headless"); //ẩn cửa sổ chrome

            chromeDriver = new ChromeDriver(service, options);

            #endregion
        }

        #region Hàm thiết lập

        public static void TaoDanhSachDinhDang()
        {
            StreamReader objReader;

            string filename = "./DinhDang.txt";

            objReader = new StreamReader(filename);

            //đọc từng dòng trong file txt rồi đưa lên list
            do
            {
                ListDinhDang.Add(objReader.ReadLine());
            }
            while (objReader.Peek() != -1);

            objReader.Close();
        }

        public static string DinhDang(string LinkAnh)
        {
            string Format = ".webp";

            foreach (string i in ListDinhDang)
            {
                if (LinkAnh.ToLower().Contains(i) == true)
                {
                    Format = i;

                    break;
                }
            }

            return Format;
        }

        public static string KiemTraChuongHientai(string IDTruyen, string ChuongHienTai)
        {
            HttpClient httpClient = new HttpClient();
            httpClient.BaseAddress = new Uri(@"https://blogtruyen.com/" + IDTruyen);

            #region Hearder
            httpClient.DefaultRequestHeaders.TryAddWithoutValidation(":authority", "blogtruyen.com");
            httpClient.DefaultRequestHeaders.TryAddWithoutValidation(":method", "GET");
            httpClient.DefaultRequestHeaders.TryAddWithoutValidation(":path", @"https://blogtruyen.com/" + IDTruyen);
            httpClient.DefaultRequestHeaders.TryAddWithoutValidation(":scheme", "https");
            httpClient.DefaultRequestHeaders.TryAddWithoutValidation("accept", @"text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,image/apng,*/*;q=0.8,application/signed-exchange;v=b3");
            httpClient.DefaultRequestHeaders.TryAddWithoutValidation("accept-encoding", "gzip, deflate, br");
            httpClient.DefaultRequestHeaders.TryAddWithoutValidation("accept-language", "vi-VN,vi;q=0.9,fr-FR;q=0.8,fr;q=0.7,en-US;q=0.6,en;q=0.5");
            httpClient.DefaultRequestHeaders.TryAddWithoutValidation("cache-control", "max-age=0");
            httpClient.DefaultRequestHeaders.TryAddWithoutValidation("cookie", "BTHiddenSidebarWidget=; BTHiddenSidebarWidget=; __cfduid=d4008b0ffe03aa2d327a08955272d519e1552905463; _ga=GA1.2.1971512359.1552871828; _gid=GA1.2.2059036427.1552871828; BT_ID=Dbw7wzBy3xd1J1TGvg5v; RdBsw44wJZ=45EABB5031A14C7939896F4FAE1728B2; BTHiddenSidebarWidget=; btpop4=Popunder; btpop5=Popunder; btpop1=Popunder; btpop2=Popunder; btpop3=Popunder; bannerpreload=1");
            httpClient.DefaultRequestHeaders.TryAddWithoutValidation("upgrade-insecure-requests", "1");
            httpClient.DefaultRequestHeaders.TryAddWithoutValidation("Referer", @"https://blogtruyen.com/" + IDTruyen);
            httpClient.DefaultRequestHeaders.TryAddWithoutValidation("user-agent", @"Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/74.0.3729.169 Safari/537.36");
            #endregion

            string htmlDanhSachChuong = httpClient.GetStringAsync("").Result;

            if(htmlDanhSachChuong.Contains("Truyện đã bị xóa hoặc không tồn tại!") == true)
            {
                ChuongHienTai = "[Lỗi truyện đã bị xóa hoặc không tồn tại]";
            }
            else
            {
                string TenTruyen = Regex.Match(htmlDanhSachChuong, "<title>(.*?)</title>", RegexOptions.Singleline).Value;
                TenTruyen = TenTruyen.Replace("<title>", "").Replace("</title>", "").Replace(" | BlogTruyen.Com", "");
                TenTruyen = TenTruyen.Trim();

                string DanhSachChuongPartem = @"<p id=""ch(.*?)</p>";
                var DanhSachChuong = Regex.Matches(htmlDanhSachChuong, DanhSachChuongPartem, RegexOptions.Singleline);

                string Ten;

                var TenChuong = Regex.Matches(DanhSachChuong[0].ToString(), @"title=""(.*?)""", RegexOptions.Singleline);
                Ten = TenChuong[0].ToString().Replace(@"title=""", "");
                Ten = Ten.Replace(@"""", "");
                Ten = HttpUtility.HtmlDecode(Ten).Replace(TenTruyen, "").Trim();

                if (ChuongHienTai == "????" || ChuongHienTai != Ten)
                { ChuongHienTai = Ten; }
            }

            return ChuongHienTai;
        }

        public void TatChrome()
        {
            try
            {
                chromeDriver.Close();
                chromeDriver.Quit();
            }
            catch
            { }
        }

        #endregion

        #region Download

        public static string TruyenQQ (string IDTruyen, string Nguon, string ChuongHienTai, int Tre, string DuongDan)
        {
            TaoDanhSachDinhDang();

            HttpClient httpClient = new HttpClient();
            httpClient.BaseAddress = new Uri(Nguon);

            string htmlDanhSachChuong = httpClient.GetStringAsync("").Result;

            //Lấy danh sách chương có trong truyện
            string DanhSachChuongPartem1 = @"<div class=""works-chapter-list"">(.*?)<div class=""block03"">";
            var DanhSachChuong1 = Regex.Matches(htmlDanhSachChuong, DanhSachChuongPartem1, RegexOptions.Singleline);

            string DanhSachChuongPartem2 = @"<a(.*?)</a>";
            var DanhSachChuong2 = Regex.Matches(DanhSachChuong1[0].ToString(), DanhSachChuongPartem2, RegexOptions.Singleline);

            string Ten = "";
            string Link, LinkAnh = "";
            int DemAnh = 0;

            //int i = DanhSachChuong2.Count -  1 - Tre; //Giới hạn danh sách với độ trễ (giới hạn cuối tùy kiểu danh sách)
            int i = DanhSachChuong2.Count - 1;

            //Nếu danh sách rỗng tức là xảy ra lỗi không tương thích
            if(i == 0)
            {
                i = -1;
                Ten = "[Lỗi tool không còn tương thích với web nguồn]\n" + IDTruyen + " " + Nguon;
            }

            //Tìm tới vị trí chương hiện tại và bắt đầu từ chương kế sau
            string TonTai = "null";
            while (i >= 0 + Tre) //Giới hạn danh sách với độ trễ (giới hạn đầu tùy kiểu danh sách)
            {
                TonTai = "Không";

                var TenChuong = Regex.Matches(DanhSachChuong2[i].ToString(), @">(.*?)<", RegexOptions.Singleline);
                Ten = TenChuong[0].ToString().Replace(@">", "");
                Ten = Ten.Replace(@"<", "");

                i--;
               
                //Loại bỏ kí tự hạn chế trong tên
                foreach (string KiTu in ListHanChe)
                {
                    try
                    {
                        ChuongHienTai = ChuongHienTai.Replace(KiTu, "");
                        Ten = Ten.Replace(KiTu, ""); 
                    }
                    catch { }
                }
                
                if (Ten.ToLower().Replace("  "," ").Trim() == ChuongHienTai.ToLower().Replace("  ", " ").Trim()) 
                {
                    TonTai = "Có";
                    break;
                }
            }

            if(TonTai == "Không")
            {
                ChuongHienTai = "[Lỗi tên chương 2 bên không tương đồng]\n" + IDTruyen + " " + ChuongHienTai + "\n" + Nguon + " " + Ten;
            }

            //Tải anh có trong chương
            while (i >= 0 + Tre) //Giới hạn danh sách với độ trễ
            {              
                //Lọc lấy link chương
                var LinkChuong = Regex.Matches(DanhSachChuong2[i].ToString(), @"href=""(.*?)""", RegexOptions.Singleline);
                Link = LinkChuong[0].ToString().Replace(@"href=""", "");
                Link = Link.Replace(@"""", "");

                //Lọc lấy tên chương
                var TenChuong = Regex.Matches(DanhSachChuong2[i].ToString(), @">(.*?)<", RegexOptions.Singleline);
                Ten = TenChuong[0].ToString().Replace(@">", "");
                Ten = Ten.Replace(@"<", "");

                if (Ten.ToLower().Contains("raw") || Ten.ToLower().Contains("eng"))
                {
                    ChuongHienTai = "[Chương này có vẻ là raw hoặc eng]\n" + " " + IDTruyen + " " + Nguon + " " + Ten;
                    break;
                }

                //Loại bỏ kí tự hạn chế trong tên
                foreach (string KiTu in ListHanChe)
                {
                    try
                    {
                        ChuongHienTai = ChuongHienTai.Replace(KiTu, "");
                        Ten = Ten.Replace(KiTu, "");
                    }
                    catch { }
                }

                //Lấy danh sách ảnh có trong chương
                HttpClient httpClient2 = new HttpClient();
                httpClient2.BaseAddress = new Uri(Link);

                string htmlDanhSachAnh = httpClient2.GetStringAsync("").Result;

                string DanhSachAnhPartem1 = @"<div class=""story-see-content"">(.*?)<div id=""stop""";
                var DanhSachAnh1 = Regex.Matches(htmlDanhSachAnh, DanhSachAnhPartem1, RegexOptions.Singleline);

                string DanhSachAnhPartem2 = @"lazy"" src=""(.*?)""";
                var DanhSachAnh2 = Regex.Matches(DanhSachAnh1[0].ToString(), DanhSachAnhPartem2, RegexOptions.Singleline);

                //Nếu danh sách rỗng tức là xảy ra lỗi không tương thích
                if (DanhSachAnh2.Count==0)
                {
                    Ten = "[Lỗi tool không còn tương thích với web nguồn]\n" + IDTruyen + " " + Nguon;
                    ChuongHienTai = Ten;

                    break;                   
                }

                DemAnh = 0;

                //Tải các ảnh có trong chương
                foreach (var anh in DanhSachAnh2)
                {
                    LinkAnh = anh.ToString().Replace(@"lazy"" src=""", "");
                    LinkAnh = LinkAnh.Replace(@"""", "");

                    try
                    {
                        if (Directory.Exists(DuongDan + "\\" + Ten) == false)
                        { Directory.CreateDirectory(DuongDan + "\\" + Ten); }

                        WebClient client = new WebClient();
                        client.Headers.Set("Referer", Link);

                        if (DemAnh < 10)
                        { client.DownloadFile(LinkAnh, DuongDan + "\\" + Ten + "\\0" + DemAnh + DinhDang(LinkAnh)); }
                        else
                        { client.DownloadFile(LinkAnh, DuongDan + "\\" + Ten + "\\" + DemAnh + DinhDang(LinkAnh)); }
                    }
                    catch
                    {
                        try
                        {
                            if (Directory.Exists(DuongDan + "\\" + Ten) == false)
                            { Directory.CreateDirectory(DuongDan + "\\" + Ten); }

                            WebClient client = new WebClient();
                            //client.Headers.Set("Referer", Link);

                            if (DemAnh < 10)
                            { client.DownloadFile(LinkAnh, DuongDan + "\\" + Ten + "\\0" + DemAnh + DinhDang(LinkAnh)); }
                            else
                            { client.DownloadFile(LinkAnh, DuongDan + "\\" + Ten + "\\" + DemAnh + DinhDang(LinkAnh)); }
                        }
                        catch
                        {
                            if (Directory.Exists(DuongDan + "\\" + Ten) == true)
                            { Directory.Delete(DuongDan + "\\" + Ten, true); }

                            i = -1; //Tải thất bại thì gán i âm để ngừng vòng lặp ngoài
                            Ten = "[Lỗi tải] " + IDTruyen + " " + Nguon + " " + Ten;

                            break;
                        }

                    }

                    DemAnh++;
                }               

                ChuongHienTai = Ten; //Cập nhật lại "chương hiện tại"

                i--;
            }

            return ChuongHienTai;
        }

        public static string Truyen48(string IDTruyen, string Nguon, string ChuongHienTai, int Tre, string DuongDan)
        {
            TaoDanhSachDinhDang();

            HttpClient httpClient = new HttpClient();
            httpClient.BaseAddress = new Uri(Nguon);

            string htmlDanhSachChuong = httpClient.GetStringAsync("").Result;

            //Lấy danh sách chương có trong truyện
            string DanhSachChuongPartem1 = @"<div class=""works-chapter-list"">(.*?)<div class=""block03"">";
            var DanhSachChuong1 = Regex.Matches(htmlDanhSachChuong, DanhSachChuongPartem1, RegexOptions.Singleline);

            string DanhSachChuongPartem2 = @"<a(.*?)</a>";
            var DanhSachChuong2 = Regex.Matches(DanhSachChuong1[0].ToString(), DanhSachChuongPartem2, RegexOptions.Singleline);

            string Ten = "";
            string Link, LinkAnh = "";
            int DemAnh = 0;

            //int i = DanhSachChuong2.Count -  1 - Tre; //Giới hạn danh sách với độ trễ (giới hạn cuối tùy kiểu danh sách)
            int i = DanhSachChuong2.Count - 1;

            //Nếu danh sách rỗng tức là xảy ra lỗi không tương thích
            if (i == 0)
            {
                i = -1;
                Ten = "[Lỗi tool không còn tương thích với web nguồn]\n" + IDTruyen + " " + Nguon;
            }

            //Tìm tới vị trí chương hiện tại và bắt đầu từ chương kế sau
            string TonTai = "null";
            while (i >= 0 + Tre) //Giới hạn danh sách với độ trễ (giới hạn đầu tùy kiểu danh sách)
            {
                TonTai = "Không";

                var TenChuong = Regex.Matches(DanhSachChuong2[i].ToString(), @">(.*?)<", RegexOptions.Singleline);
                Ten = TenChuong[0].ToString().Replace(@">", "");
                Ten = Ten.Replace(@"<", "");

                i--;

                //Loại bỏ kí tự hạn chế trong tên
                foreach (string KiTu in ListHanChe)
                {
                    try
                    {
                        ChuongHienTai = ChuongHienTai.Replace(KiTu, "");
                        Ten = Ten.Replace(KiTu, "");
                    }
                    catch { }
                }

                if (Ten.ToLower().Replace("  "," ").Trim() == ChuongHienTai.ToLower().Replace("  ", " ").Trim())
                {
                    TonTai = "Có";
                    break;
                }
            }

            if (TonTai == "Không")
            {
                ChuongHienTai = "[Lỗi tên chương 2 bên không tương đồng]\n" + IDTruyen + " " + ChuongHienTai + "\n" + Nguon + " " + Ten;
            }

            //Tải anh có trong chương
            while (i >= 0 + Tre) //Giới hạn danh sách với độ trễ
            {
                //Lọc lấy link chương
                var LinkChuong = Regex.Matches(DanhSachChuong2[i].ToString(), @"href=""(.*?)""", RegexOptions.Singleline);
                Link = LinkChuong[0].ToString().Replace(@"href=""", "");
                Link = Link.Replace(@"""", "");

                //Lọc lấy tên chương
                var TenChuong = Regex.Matches(DanhSachChuong2[i].ToString(), @">(.*?)<", RegexOptions.Singleline);
                Ten = TenChuong[0].ToString().Replace(@">", "");
                Ten = Ten.Replace(@"<", "");

                if (Ten.ToLower().Contains("raw") || Ten.ToLower().Contains("eng"))
                {
                    ChuongHienTai = "[Chương này có vẻ là raw hoặc eng]\n" + " " + IDTruyen + " " + Nguon + " " + Ten;
                    break;
                }

                //Loại bỏ kí tự hạn chế trong tên
                foreach (string KiTu in ListHanChe)
                {
                    try
                    {
                        ChuongHienTai = ChuongHienTai.Replace(KiTu, "");
                        Ten = Ten.Replace(KiTu, "");
                    }
                    catch { }
                }

                //Lấy danh sách ảnh có trong chương
                HttpClient httpClient2 = new HttpClient();
                httpClient2.BaseAddress = new Uri(Link);

                string htmlDanhSachAnh = httpClient2.GetStringAsync("").Result;

                string DanhSachAnhPartem1 = @"<div class=""story-see-content"">(.*?)<div id=""stop""";
                var DanhSachAnh1 = Regex.Matches(htmlDanhSachAnh, DanhSachAnhPartem1, RegexOptions.Singleline);

                string DanhSachAnhPartem2 = @"lazy"" src=""(.*?)""";
                var DanhSachAnh2 = Regex.Matches(DanhSachAnh1[0].ToString(), DanhSachAnhPartem2, RegexOptions.Singleline);

                //Nếu danh sách rỗng tức là xảy ra lỗi không tương thích
                if (DanhSachAnh2.Count == 0)
                {
                    Ten = "[Lỗi tool không còn tương thích với web nguồn]\n" + IDTruyen + " " + Nguon;
                    ChuongHienTai = Ten;

                    break;
                }

                DemAnh = 0;

                //Tải các ảnh có trong chương
                foreach (var anh in DanhSachAnh2)
                {
                    LinkAnh = anh.ToString().Replace(@"lazy"" src=""", "");
                    LinkAnh = LinkAnh.Replace(@"""", "");

                    try
                    {
                        if (Directory.Exists(DuongDan + "\\" + Ten) == false)
                        { Directory.CreateDirectory(DuongDan + "\\" + Ten); }

                        WebClient client = new WebClient();
                        client.Headers.Set("Referer", Link);

                        if (DemAnh < 10)
                        { client.DownloadFile(LinkAnh, DuongDan + "\\" + Ten + "\\0" + DemAnh + DinhDang(LinkAnh)); }
                        else
                        { client.DownloadFile(LinkAnh, DuongDan + "\\" + Ten + "\\" + DemAnh + DinhDang(LinkAnh)); }
                    }
                    catch
                    {
                        try
                        {
                            if (Directory.Exists(DuongDan + "\\" + Ten) == false)
                            { Directory.CreateDirectory(DuongDan + "\\" + Ten); }

                            WebClient client = new WebClient();
                            //client.Headers.Set("Referer", Link);

                            if (DemAnh < 10)
                            { client.DownloadFile(LinkAnh, DuongDan + "\\" + Ten + "\\0" + DemAnh + DinhDang(LinkAnh)); }
                            else
                            { client.DownloadFile(LinkAnh, DuongDan + "\\" + Ten + "\\" + DemAnh + DinhDang(LinkAnh)); }
                        }
                        catch
                        {
                            if (Directory.Exists(DuongDan + "\\" + Ten) == true)
                            { Directory.Delete(DuongDan + "\\" + Ten, true); }

                            i = -1; //Tải thất bại thì gán i âm để ngừng vòng lặp ngoài
                            Ten = "[Lỗi tải] " + IDTruyen + " " + Nguon + " " + Ten;

                            break;
                        }

                    }

                    DemAnh++;
                }

                ChuongHienTai = Ten; //Cập nhật lại "chương hiện tại"

                i--;
            }

            return ChuongHienTai;
        }

        public static string TruyenTranhNet (string IDTruyen, string Nguon, string ChuongHienTai, int Tre, string DuongDan)
        {
            TaoDanhSachDinhDang();

            HttpClient httpClient = new HttpClient();
            httpClient.BaseAddress = new Uri(Nguon);

            string htmlDanhSachChuong = httpClient.GetStringAsync("").Result;

            #region Xử lí tên truyện (dành riêng cho TruyenTranhNet)

            //Lấy tên truyện trên title
            string TenTitle = Regex.Match(htmlDanhSachChuong, @"title-manga"">(.*?)<", RegexOptions.Singleline).Value;

            //Lấy tên truyện khác
            string TenKhac = Regex.Match(htmlDanhSachChuong, @"Tên khác:</span>(.*?)<br", RegexOptions.Singleline).Value;
            TenKhac = TenKhac.Replace("Tên khác:</span>", "").Replace("<br>", "").Replace(";", ",").Replace(".", ",");

            //Tạo danh sách tên truyện
            List<string> ListTenTruyen = new List<string>(TenKhac.Split(','));
            ListTenTruyen.Add(TenTitle);

            #endregion

            //Lấy danh sách chương có trong truyện
            string DanhSachChuongPartem1 = @"<div class=""content mCustomScrollbar"">(.*?)</div>";
            var DanhSachChuong1 = Regex.Matches(htmlDanhSachChuong, DanhSachChuongPartem1, RegexOptions.Singleline);

            string DanhSachChuongPartem2 = @"<p>(.*?)</p>";
            var DanhSachChuong2 = Regex.Matches(DanhSachChuong1[0].ToString(), DanhSachChuongPartem2, RegexOptions.Singleline);

            string Ten = "";
            string Link, LinkAnh = "";
            int DemAnh = 0;
            int i = 0;

            //Nếu danh sách rỗng tức là xảy ra lỗi không tương thích
            if (DanhSachChuong2.Count == 0)
            {
                i = DanhSachChuong2.Count - 1 - Tre +1;
                Ten = "[Lỗi tool không còn tương thích với web nguồn]\n" + IDTruyen + " " + Nguon;
            }

            //Tìm tới vị trí chương hiện tại và bắt đầu từ chương kế sau
            string TonTai = "null";
            while (i <= DanhSachChuong2.Count - 1 - Tre)
            {
                TonTai = "Không";

                var TenChuong = Regex.Matches(DanhSachChuong2[i].ToString(), @"title=""(.*?)""", RegexOptions.Singleline);
                Ten = TenChuong[0].ToString().Replace(@"title=""", "");
                Ten = Ten.Replace(@"""", "");

                i++;

                //Loại bỏ kí tự hạn chế trong tên
                foreach (string KiTu in ListHanChe)
                {
                    try
                    {
                        ChuongHienTai = ChuongHienTai.Replace(KiTu, "");
                        Ten = Ten.Replace(KiTu, "");
                    }
                    catch { }
                }

                if (Ten.ToLower().Replace("  "," ").Trim() == ChuongHienTai.ToLower().Replace("  ", " ").Trim())
                {
                    TonTai = "Có";
                    break;
                }
            }

            if (TonTai == "Không")
            {
                ChuongHienTai = "[Lỗi tên chương 2 bên không tương đồng]\n" + IDTruyen + " " + ChuongHienTai + "\n" + Nguon + " " + Ten;
            }

            //Tải anh có trong chương
            while (i <= DanhSachChuong2.Count - 1 - Tre) //Giới hạn danh sách với độ trễ
            {
                //Lọc lấy link chương
                var LinkChuong = Regex.Matches(DanhSachChuong2[i].ToString(), @"href=""(.*?)""", RegexOptions.Singleline);
                Link = LinkChuong[0].ToString().Replace(@"href=""", "");
                Link = Link.Replace(@"""", "");

                //Lọc lấy tên chương
                var TenChuong = Regex.Matches(DanhSachChuong2[i].ToString(), @"title=""(.*?)""", RegexOptions.Singleline);
                Ten = TenChuong[0].ToString().Replace(@"title=""", "");
                Ten = Ten.Replace(@"""", "");

                if (Ten.ToLower().Contains("raw") || Ten.ToLower().Contains("eng"))
                {
                    ChuongHienTai = "[Chương này có vẻ là raw hoặc eng]\n" + " " + IDTruyen + " " + Nguon + " " + Ten;
                    break;
                }

                //Loại bỏ kí tự hạn chế trong tên
                foreach (string KiTu in ListHanChe)
                {
                    try
                    {
                        ChuongHienTai = ChuongHienTai.Replace(KiTu, "");
                        Ten = Ten.Replace(KiTu, "");
                    }
                    catch { }
                }

                //Xóa tên truyện khỏi tên chương
                foreach (string TenTruyen in ListTenTruyen)
                {
                    Ten = Ten.Replace(TenTruyen.Trim(), "").Trim();
                }

                //Lấy danh sách ảnh có trong chương
                HttpClient httpClient2 = new HttpClient();
                httpClient2.BaseAddress = new Uri(Link);

                string htmlDanhSachAnh = httpClient2.GetStringAsync("").Result;

                string DanhSachAnhPartem1 = @"<div class=""each-page"" id=""pageDetail"">(.*?)<div class=""row"">";
                var DanhSachAnh1 = Regex.Matches(htmlDanhSachAnh, DanhSachAnhPartem1, RegexOptions.Singleline);

                string DanhSachAnhPartem2 = @"<img src=""(.*?)""";
                var DanhSachAnh2 = Regex.Matches(DanhSachAnh1[0].ToString(), DanhSachAnhPartem2, RegexOptions.Singleline);

                //Nếu danh sách rỗng tức là xảy ra lỗi không tương thích
                if (DanhSachAnh2.Count == 0)
                {
                    Ten = "[Lỗi tool không còn tương thích với web nguồn]\n" + IDTruyen + " " + Nguon;
                    ChuongHienTai = Ten;
                    break;
                    
                }

                DemAnh = 0;

                //Tải các ảnh có trong chương
                foreach (var anh in DanhSachAnh2)
                {
                    LinkAnh = anh.ToString().Replace(@"<img src=""", "");
                    LinkAnh = LinkAnh.Replace(@"""", "");

                    try
                    {
                        if (Directory.Exists(DuongDan + "\\" + Ten) == false)
                        { Directory.CreateDirectory(DuongDan + "\\" + Ten); }

                        WebClient client = new WebClient();
                        client.Headers.Set("Referer", Link);

                        if (DemAnh < 10)
                        { client.DownloadFile(LinkAnh, DuongDan + "\\" + Ten + "\\0" + DemAnh + DinhDang(LinkAnh)); }
                        else
                        { client.DownloadFile(LinkAnh, DuongDan + "\\" + Ten + "\\" + DemAnh + DinhDang(LinkAnh)); }
                    }
                    catch
                    {
                        try
                        {
                            if (Directory.Exists(DuongDan + "\\" + Ten) == false)
                            { Directory.CreateDirectory(DuongDan + "\\" + Ten); }

                            WebClient client = new WebClient();
                            //client.Headers.Set("Referer", Link);

                            if (DemAnh < 10)
                            { client.DownloadFile(LinkAnh, DuongDan + "\\" + Ten + "\\0" + DemAnh + DinhDang(LinkAnh)); }
                            else
                            { client.DownloadFile(LinkAnh, DuongDan + "\\" + Ten + "\\" + DemAnh + DinhDang(LinkAnh)); }
                        }
                        catch
                        {
                            if (Directory.Exists(DuongDan + "\\" + Ten) == true)
                            { Directory.Delete(DuongDan + "\\" + Ten, true); }

                            i = -1; //Tải thất bại thì gán i âm để ngừng vòng lặp ngoài
                            Ten = "[Lỗi tải] " + IDTruyen + " " + Nguon + " " + Ten;

                            break;
                        }

                    }

                    DemAnh++;
                }

                ChuongHienTai = Ten; //Cập nhật lại "chương hiện tại"

                i++;
            }         

            return ChuongHienTai;
        }

        public static string TruyenTranhLH (string IDTruyen, string Nguon, string ChuongHienTai, int Tre, string DuongDan)
        {
            TaoDanhSachDinhDang();

            HttpClient httpClient = new HttpClient();
            httpClient.BaseAddress = new Uri(Nguon);

            string htmlDanhSachChuong = httpClient.GetStringAsync("").Result;

            #region Xử lí tên truyện (dành riêng cho TruyenTranhLH)

            //Lấy tên truyện
            string TenTruyen = Regex.Match(htmlDanhSachChuong, @"<h1>(.*?)</h1>", RegexOptions.Singleline).Value;
            TenTruyen = TenTruyen.Replace("<h1>", "").Replace("</h1>", "");

            #endregion

            //Lấy danh sách chương có trong truyện
            string DanhSachChuongPartem1 = @"<tbody>(.*?)</tbody>";
            var DanhSachChuong1 = Regex.Matches(htmlDanhSachChuong, DanhSachChuongPartem1, RegexOptions.Singleline);

            string DanhSachChuongPartem2 = @"<a class=""chapter""(.*?)</a>";
            var DanhSachChuong2 = Regex.Matches(DanhSachChuong1[0].ToString(), DanhSachChuongPartem2, RegexOptions.Singleline);

            string Ten = "";
            string Link, LinkAnh = "";
            int DemAnh = 0;

            //int i = DanhSachChuong2.Count -  1 - Tre; //Giới hạn danh sách với độ trễ (giới hạn cuối tùy kiểu danh sách)
            int i = DanhSachChuong2.Count - 1;

            //Nếu danh sách rỗng tức là xảy ra lỗi không tương thích
            if (i == 0)
            {
                i = -1;
                Ten = "[Lỗi tool không còn tương thích với web nguồn]\n" + IDTruyen + " " + Nguon;
            }

            //Tìm tới vị trí chương hiện tại và bắt đầu từ chương kế sau
            string TonTai = "null";
            while (i >= 0 + Tre) //Giới hạn danh sách với độ trễ (giới hạn đầu tùy kiểu danh sách)
            {
                TonTai = "Không";

                var TenChuong = Regex.Matches(DanhSachChuong2[i].ToString(), @"<b>(.*?)</b>", RegexOptions.Singleline);
                Ten = TenChuong[0].ToString().Replace(@"<b>", "");
                Ten = Ten.Replace(@"</b>", "");
                Ten = Ten.ToLower().Replace(TenTruyen.ToLower(), "").Replace("-", "").Trim();

                i--;

                //Loại bỏ kí tự hạn chế trong tên
                foreach (string KiTu in ListHanChe)
                {
                    try
                    {
                        ChuongHienTai = ChuongHienTai.Replace(KiTu, "");
                        Ten = Ten.Replace(KiTu, "");
                    }
                    catch { }
                }

                if (Ten.ToLower().Replace("  "," ").Trim() == ChuongHienTai.ToLower().Replace("  ", " ").Trim())
                {
                    TonTai = "Có";
                    break;
                }
            }

            if (TonTai == "Không")
            {
                ChuongHienTai = "[Lỗi tên chương 2 bên không tương đồng]\n" + IDTruyen + " " + ChuongHienTai + "\n" + Nguon + " " + Ten;
            }

            //Tải anh có trong chương
            while (i >= 0 + Tre) //Giới hạn danh sách với độ trễ
            {
                //Lọc lấy link chương
                var LinkChuong = Regex.Matches(DanhSachChuong2[i].ToString(), @"href='(.*?)'", RegexOptions.Singleline);
                Link = LinkChuong[0].ToString().Replace(@"href='", "");
                Link = Link.Replace(@"'", "");
                Link = "https://truyentranhlh.net/" + Link;

                //Lọc lấy tên chương
                var TenChuong = Regex.Matches(DanhSachChuong2[i].ToString(), @"<b>(.*?)</b>", RegexOptions.Singleline);
                Ten = TenChuong[0].ToString().Replace(@"<b>", "");
                Ten = Ten.Replace(@"</b>", "");
                Ten = Ten.ToLower().Replace(TenTruyen.ToLower(), "").Replace("-", "").Trim();

                if (Ten.ToLower().Contains("raw") || Ten.ToLower().Contains("eng"))
                {
                    ChuongHienTai = "[Chương này có vẻ là raw hoặc eng]\n" + " " + IDTruyen + " " + Nguon + " " + Ten;
                    break;
                }

                //Loại bỏ kí tự hạn chế trong tên
                foreach (string KiTu in ListHanChe)
                {
                    try
                    {
                        ChuongHienTai = ChuongHienTai.Replace(KiTu, "");
                        Ten = Ten.Replace(KiTu, "");
                    }
                    catch { }
                }

                //Lấy danh sách ảnh có trong chương
                HttpClient httpClient2 = new HttpClient();
                httpClient2.BaseAddress = new Uri(Link);

                string htmlDanhSachAnh = httpClient2.GetStringAsync("").Result;

                string DanhSachAnhPartem1 = @"<div class=""chapter-content"">(.*?)<!-- Before images -->";
                var DanhSachAnh1 = Regex.Matches(htmlDanhSachAnh, DanhSachAnhPartem1, RegexOptions.Singleline);

                string DanhSachAnhPartem2 = @"<img class(.*?)>";
                var DanhSachAnh2 = Regex.Matches(DanhSachAnh1[0].ToString(), DanhSachAnhPartem2, RegexOptions.Singleline);

                //Nếu danh sách rỗng tức là xảy ra lỗi không tương thích
                if (DanhSachAnh2.Count == 0)
                {
                    Ten = "[Lỗi tool không còn tương thích với web nguồn]\n" + IDTruyen + " " + Nguon;
                    ChuongHienTai = Ten;
                    break;                   
                }

                DemAnh = 0;

                //Tải các ảnh có trong chương
                foreach (var anh in DanhSachAnh2)
                {

                    var LinkCuaAnh = Regex.Matches(anh.ToString(), @"src='(.*?)'", RegexOptions.Singleline);
                    LinkAnh = LinkCuaAnh[0].ToString().Replace(@"src='", "");
                    LinkAnh = LinkAnh.Replace(@"'", "");

                    try
                    {
                        if (Directory.Exists(DuongDan + "\\" + Ten) == false)
                        { Directory.CreateDirectory(DuongDan + "\\" + Ten); }

                        WebClient client = new WebClient();
                        client.Headers.Set("Referer", Link);

                        if (DemAnh < 10)
                        { client.DownloadFile(LinkAnh, DuongDan + "\\" + Ten + "\\0" + DemAnh + DinhDang(LinkAnh)); }
                        else
                        { client.DownloadFile(LinkAnh, DuongDan + "\\" + Ten + "\\" + DemAnh + DinhDang(LinkAnh)); }
                    }
                    catch
                    {
                        try
                        {
                            if (Directory.Exists(DuongDan + "\\" + Ten) == false)
                            { Directory.CreateDirectory(DuongDan + "\\" + Ten); }

                            WebClient client = new WebClient();
                            //client.Headers.Set("Referer", Link);

                            if (DemAnh < 10)
                            { client.DownloadFile(LinkAnh, DuongDan + "\\" + Ten + "\\0" + DemAnh + DinhDang(LinkAnh)); }
                            else
                            { client.DownloadFile(LinkAnh, DuongDan + "\\" + Ten + "\\" + DemAnh + DinhDang(LinkAnh)); }
                        }
                        catch
                        {
                            if (Directory.Exists(DuongDan + "\\" + Ten) == true)
                            { Directory.Delete(DuongDan + "\\" + Ten, true); }

                            i = -1; //Tải thất bại thì gán i âm để ngừng vòng lặp ngoài
                            Ten = "[Lỗi tải] " + IDTruyen + " " + Nguon + " " + Ten;

                            break;
                        }

                    }

                    DemAnh++;
                }

                ChuongHienTai = Ten; //Cập nhật lại "chương hiện tại"

                i--;
            }

            return ChuongHienTai;
        }

        public static string Beeng (string IDTruyen, string Nguon, string ChuongHienTai, int Tre, string DuongDan)
        {
            TaoDanhSachDinhDang();

            HttpClient httpClient = new HttpClient();
            httpClient.BaseAddress = new Uri(Nguon);

            string htmlDanhSachChuong = httpClient.GetStringAsync("").Result;


            //Lấy danh sách chương có trong truyện
            string DanhSachChuongPartem1 = @"<li class=""active"">(.*?)</ul>";
            var DanhSachChuong1 = Regex.Matches(htmlDanhSachChuong, DanhSachChuongPartem1, RegexOptions.Singleline);

            string DanhSachChuongPartem2 = @"<a(.*?)</a>";
            var DanhSachChuong2 = Regex.Matches(DanhSachChuong1[0].ToString(), DanhSachChuongPartem2, RegexOptions.Singleline);

            string Ten = "";
            string Link, LinkAnh = "";
            int DemAnh = 0;

            //int i = DanhSachChuong2.Count -  1 - Tre; //Giới hạn danh sách với độ trễ (giới hạn cuối tùy kiểu danh sách)
            int i = DanhSachChuong2.Count - 1;

            //Nếu danh sách rỗng tức là xảy ra lỗi không tương thích
            if (i == 0)
            {
                i = -1;
                Ten = "[Lỗi tool không còn tương thích với web nguồn]\n" + IDTruyen + " " + Nguon;
            }

            //Tìm tới vị trí chương hiện tại và bắt đầu từ chương kế sau
            string TonTai = "null";
            while (i >= 0 + Tre) //Giới hạn danh sách với độ trễ (giới hạn đầu tùy kiểu danh sách)
            {
                TonTai = "Không";

                var TenChuong = Regex.Matches(DanhSachChuong2[i].ToString(), @">(.*?)<", RegexOptions.Singleline);
                Ten = TenChuong[0].ToString().Replace(@">", "");
                Ten = Ten.Replace(@"<", "");

                i--;

                //Loại bỏ kí tự hạn chế trong tên
                foreach (string KiTu in ListHanChe)
                {
                    try
                    {
                        ChuongHienTai = ChuongHienTai.Replace(KiTu, "");
                        Ten = Ten.Replace(KiTu, "");
                    }
                    catch { }
                }

                if (Ten.ToLower().Replace("  "," ").Trim() == ChuongHienTai.ToLower().Replace("  ", " ").Trim())
                {
                    TonTai = "Có";
                    break;
                }
            }

            if (TonTai == "Không")
            {
                ChuongHienTai = "[Lỗi tên chương 2 bên không tương đồng]\n" + IDTruyen + " " + ChuongHienTai + "\n" + Nguon + " " + Ten;
            }

            //Tải anh có trong chương
            while (i >= 0 + Tre) //Giới hạn danh sách với độ trễ
            {
                //Lọc lấy link chương
                var LinkChuong = Regex.Matches(DanhSachChuong2[i].ToString(), @"href=""(.*?)""", RegexOptions.Singleline);
                Link = LinkChuong[0].ToString().Replace(@"href=""", "");
                Link = Link.Replace(@"""", "");
                Link = "https://beeng.net/" + Link;

                //Lọc lấy tên chương
                var TenChuong = Regex.Matches(DanhSachChuong2[i].ToString(), @">(.*?)<", RegexOptions.Singleline);
                Ten = TenChuong[0].ToString().Replace(@">", "");
                Ten = Ten.Replace(@"<", "");

                //Loại bỏ kí tự hạn chế trong tên
                foreach (string KiTu in ListHanChe)
                {
                    try
                    {
                        ChuongHienTai = ChuongHienTai.Replace(KiTu, "");
                        Ten = Ten.Replace(KiTu, "");
                    }
                    catch { }
                }

                //Lấy danh sách ảnh có trong chương
                HttpClient httpClient2 = new HttpClient();
                httpClient2.BaseAddress = new Uri(Link);

                string htmlDanhSachAnh = httpClient2.GetStringAsync("").Result;

                string DanhSachAnhPartem1 = @"<div id=""image-load"">(.*?)</div>";
                var DanhSachAnh1 = Regex.Matches(htmlDanhSachAnh, DanhSachAnhPartem1, RegexOptions.Singleline);

                string DanhSachAnhPartem2 = @"class="""" src=""(.*?)""";
                var DanhSachAnh2 = Regex.Matches(DanhSachAnh1[0].ToString(), DanhSachAnhPartem2, RegexOptions.Singleline);

                //Nếu danh sách rỗng tức là xảy ra lỗi không tương thích
                if (DanhSachAnh2.Count == 0)
                {
                    Ten = "[Lỗi tool không còn tương thích với web nguồn]\n" + IDTruyen + " " + Nguon;
                    ChuongHienTai = Ten;

                    break;                   
                }

                DemAnh = 0;

                //Tải các ảnh có trong chương
                foreach (var anh in DanhSachAnh2)
                {
                    LinkAnh = anh.ToString().Replace(@"class="""" src=""", "");
                    LinkAnh = LinkAnh.Replace(@"""", "");

                    try
                    {
                        if (Directory.Exists(DuongDan + "\\" + Ten) == false)
                        { Directory.CreateDirectory(DuongDan + "\\" + Ten); }

                        WebClient client = new WebClient();
                        client.Headers.Set("Referer", Link);

                        if (DemAnh < 10)
                        { client.DownloadFile(LinkAnh, DuongDan + "\\" + Ten + "\\0" + DemAnh + DinhDang(LinkAnh)); }
                        else
                        { client.DownloadFile(LinkAnh, DuongDan + "\\" + Ten + "\\" + DemAnh + DinhDang(LinkAnh)); }
                    }
                    catch
                    {
                        try
                        {
                            if (Directory.Exists(DuongDan + "\\" + Ten) == false)
                            { Directory.CreateDirectory(DuongDan + "\\" + Ten); }

                            WebClient client = new WebClient();
                            //client.Headers.Set("Referer", Link);

                            if (DemAnh < 10)
                            { client.DownloadFile(LinkAnh, DuongDan + "\\" + Ten + "\\0" + DemAnh + DinhDang(LinkAnh)); }
                            else
                            { client.DownloadFile(LinkAnh, DuongDan + "\\" + Ten + "\\" + DemAnh + DinhDang(LinkAnh)); }
                        }
                        catch
                        {
                            if (Directory.Exists(DuongDan + "\\" + Ten) == true)
                            { Directory.Delete(DuongDan + "\\" + Ten, true); }

                            i = -1; //Tải thất bại thì gán i âm để ngừng vòng lặp ngoài
                            Ten = "[Lỗi tải] " + IDTruyen + " " + Nguon + " " + Ten;

                            break;
                        }

                    }

                    DemAnh++;
                }

                ChuongHienTai = Ten; //Cập nhật lại "chương hiện tại"

                i--;
            }

            return ChuongHienTai;
        }

        public static string Ntruyen (string IDTruyen, string Nguon, string ChuongHienTai, int Tre, string DuongDan)
        {
            TaoDanhSachDinhDang();

            HttpClient httpClient = new HttpClient();
            httpClient.BaseAddress = new Uri(Nguon);

            string htmlDanhSachChuong = httpClient.GetStringAsync("").Result;

            //Lấy danh sách chương có trong truyện
            string DanhSachChuongPartem1 = @"id=""MainContent_CenterContent_detailStoryControl_listChapter"" class=""listChapter"">(.*?)<script";
            var DanhSachChuong1 = Regex.Matches(htmlDanhSachChuong, DanhSachChuongPartem1, RegexOptions.Singleline);

            string DanhSachChuongPartem2 = @"cellChapter""><a href=""(.*?)</div>";
            var DanhSachChuong2 = Regex.Matches(DanhSachChuong1[0].ToString(), DanhSachChuongPartem2, RegexOptions.Singleline);

            string Ten = "";
            string Link, LinkAnh = "";
            int DemAnh = 0;

            //int i = DanhSachChuong2.Count -  1 - Tre; //Giới hạn danh sách với độ trễ (giới hạn cuối tùy kiểu danh sách)
            int i = DanhSachChuong2.Count - 1;

            //Nếu danh sách rỗng tức là xảy ra lỗi không tương thích
            if (i == 0)
            {
                i = -1;
                Ten = "[Lỗi tool không còn tương thích với web nguồn]\n" + IDTruyen + " " + Nguon;
            }

            //Tìm tới vị trí chương hiện tại và bắt đầu từ chương kế sau
            string TonTai = "null";
            while (i >= 0 + Tre) //Giới hạn danh sách với độ trễ (giới hạn đầu tùy kiểu danh sách)
            {
                TonTai = "Không";

                var TenChuong = Regex.Matches(DanhSachChuong2[i].ToString(), @""">(.*?)<", RegexOptions.Singleline);
                Ten = TenChuong[1].ToString().Replace(@""">", "");
                Ten = Ten.Replace(@"<", "");

                i--;

                //Loại bỏ kí tự hạn chế trong tên
                foreach (string KiTu in ListHanChe)
                {
                    try
                    {
                        ChuongHienTai = ChuongHienTai.Replace(KiTu, "");
                        Ten = Ten.Replace(KiTu, "");
                    }
                    catch { }
                }

                if (Ten.ToLower().Replace("  "," ").Trim() == ChuongHienTai.ToLower().Replace("  ", " ").Trim())
                {
                    TonTai = "Có";
                    break;
                }
            }

            if (TonTai == "Không")
            {
                ChuongHienTai = "[Lỗi tên chương 2 bên không tương đồng]\n" + IDTruyen + " " + ChuongHienTai + "\n" + Nguon + " " + Ten;
            }

            //Tải anh có trong chương
            while (i >= 0 + Tre) //Giới hạn danh sách với độ trễ
            {
                //Lọc lấy link chương
                var LinkChuong = Regex.Matches(DanhSachChuong2[i].ToString(), @"href=""(.*?)""", RegexOptions.Singleline);
                Link = LinkChuong[0].ToString().Replace(@"href=""", "");
                Link = Link.Replace(@"""", "");
                Link = "https://ntruyen.info" + Link;

                //Lọc lấy tên chương
                var TenChuong = Regex.Matches(DanhSachChuong2[i].ToString(), @""">(.*?)<", RegexOptions.Singleline);
                Ten = TenChuong[1].ToString().Replace(@""">", "");
                Ten = Ten.Replace(@"<", "");

                //Loại bỏ kí tự hạn chế trong tên
                foreach (string KiTu in ListHanChe)
                {
                    try
                    {
                        ChuongHienTai = ChuongHienTai.Replace(KiTu, "");
                        Ten = Ten.Replace(KiTu, "");
                    }
                    catch { }
                }

                //Lấy danh sách ảnh có trong chương
                //HttpClient httpClient2 = new HttpClient();
                //httpClient2.BaseAddress = new Uri(Link);

                //string htmlDanhSachAnh = httpClient2.GetStringAsync("").Result;

                chromeDriver.Url = Link;
                chromeDriver.Navigate();

                string htmlDanhSachAnh = chromeDriver.PageSource;

                string DanhSachAnhPartem1 = @"<div id=""containerListPage"">(.*?)<div class=""advertTop"">";
                var DanhSachAnh1 = Regex.Matches(htmlDanhSachAnh, DanhSachAnhPartem1, RegexOptions.Singleline);

                string DanhSachAnhPartem2 = @"src=""(.*?)""";
                var DanhSachAnh2 = Regex.Matches(DanhSachAnh1[0].ToString(), DanhSachAnhPartem2, RegexOptions.Singleline);

                //Nếu danh sách rỗng tức là xảy ra lỗi không tương thích
                if (DanhSachAnh2.Count == 0)
                {
                    Ten = "[Lỗi tool không còn tương thích với web nguồn]\n" + IDTruyen + " " + Nguon;
                    ChuongHienTai = Ten;

                    break;                    
                }

                DemAnh = 0;

                //Tải các ảnh có trong chương
                foreach (var anh in DanhSachAnh2)
                {
                    LinkAnh = anh.ToString().Replace(@"src=""", "");
                    LinkAnh = LinkAnh.Replace(@"""", "");

                    try
                    {
                        if (Directory.Exists(DuongDan + "\\" + Ten) == false)
                        { Directory.CreateDirectory(DuongDan + "\\" + Ten); }

                        WebClient client = new WebClient();
                        client.Headers.Set("Referer", Link);

                        if (DemAnh < 10)
                        { client.DownloadFile(LinkAnh, DuongDan + "\\" + Ten + "\\0" + DemAnh + DinhDang(LinkAnh)); }
                        else
                        { client.DownloadFile(LinkAnh, DuongDan + "\\" + Ten + "\\" + DemAnh + DinhDang(LinkAnh)); }
                    }
                    catch
                    {
                        try
                        {
                            if (Directory.Exists(DuongDan + "\\" + Ten) == false)
                            { Directory.CreateDirectory(DuongDan + "\\" + Ten); }

                            WebClient client = new WebClient();
                            //client.Headers.Set("Referer", Link);

                            if (DemAnh < 10)
                            { client.DownloadFile(LinkAnh, DuongDan + "\\" + Ten + "\\0" + DemAnh + DinhDang(LinkAnh)); }
                            else
                            { client.DownloadFile(LinkAnh, DuongDan + "\\" + Ten + "\\" + DemAnh + DinhDang(LinkAnh)); }
                        }
                        catch
                        {
                            if (Directory.Exists(DuongDan + "\\" + Ten) == true)
                            { Directory.Delete(DuongDan + "\\" + Ten, true); }

                            i = -1; //Tải thất bại thì gán i âm để ngừng vòng lặp ngoài
                            Ten = "[Lỗi tải] " + IDTruyen + " " + Nguon + " " + Ten;

                            break;
                        }

                    }

                    DemAnh++;
                }

                ChuongHienTai = Ten; //Cập nhật lại "chương hiện tại"

                i--;
            }

            return ChuongHienTai;
        }

        public static string HocVienTruyenTranh (string IDTruyen, string Nguon, string ChuongHienTai, int Tre, string DuongDan)
        {
            TaoDanhSachDinhDang();

            HttpClient httpClient = new HttpClient();
            httpClient.BaseAddress = new Uri(Nguon);

            string htmlDanhSachChuong = httpClient.GetStringAsync("").Result;

            //Lấy danh sách chương có trong truyện
            string DanhSachChuongPartem1 = @"<tbody>(.*?)</tbody>";
            var DanhSachChuong1 = Regex.Matches(htmlDanhSachChuong, DanhSachChuongPartem1, RegexOptions.Singleline);

            string DanhSachChuongPartem2 = @"<tr>(.*?)</tr>";
            var DanhSachChuong2 = Regex.Matches(DanhSachChuong1[0].ToString(), DanhSachChuongPartem2, RegexOptions.Singleline);

            string Ten = "";
            string Link, LinkAnh = "";
            int DemAnh = 0;

            //int i = DanhSachChuong2.Count -  1 - Tre; //Giới hạn danh sách với độ trễ (giới hạn cuối tùy kiểu danh sách)
            int i = DanhSachChuong2.Count - 1;

            //Nếu danh sách rỗng tức là xảy ra lỗi không tương thích
            if (i == 0)
            {
                i = -1;
                Ten = "[Lỗi tool không còn tương thích với web nguồn]\n" + IDTruyen + " " + Nguon;
            }

            //Tìm tới vị trí chương hiện tại và bắt đầu từ chương kế sau
            string TonTai = "null";
            while (i >= 0 + Tre) //Giới hạn danh sách với độ trễ (giới hạn đầu tùy kiểu danh sách)
            {
                TonTai = "Không";

                var TenChuong = Regex.Matches(DanhSachChuong2[i].ToString(), @"title=""(.*?)""", RegexOptions.Singleline);
                Ten = TenChuong[0].ToString().Replace(@"title=""", "");
                Ten = Ten.Replace(@"""", "");

                i--;               

                //Loại bỏ kí tự hạn chế trong tên
                foreach (string KiTu in ListHanChe)
                {
                    try
                    {
                        ChuongHienTai = ChuongHienTai.Replace(KiTu, "");
                        Ten = Ten.Replace(KiTu, "");
                    }
                    catch { }
                }

                if (Ten.ToLower().Replace("  "," ").Trim() == ChuongHienTai.ToLower().Replace("  ", " ").Trim())
                {
                    TonTai = "Có";
                    break;
                }
            }

            if (TonTai == "Không")
            {
                ChuongHienTai = "[Lỗi tên chương 2 bên không tương đồng]\n" + IDTruyen + " " + ChuongHienTai + "\n" + Nguon + " " + Ten;
            }

            //Tải anh có trong chương
            while (i >= 0 + Tre) //Giới hạn danh sách với độ trễ
            {
                //Lọc lấy link chương
                var LinkChuong = Regex.Matches(DanhSachChuong2[i].ToString(), @"href=""(.*?)""", RegexOptions.Singleline);
                Link = LinkChuong[0].ToString().Replace(@"href=""", "");
                Link = Link.Replace(@"""", "");

                //Lọc lấy tên chương
                var TenChuong = Regex.Matches(DanhSachChuong2[i].ToString(), @"title=""(.*?)""", RegexOptions.Singleline);
                Ten = TenChuong[0].ToString().Replace(@"title=""", "");
                Ten = Ten.Replace(@"""", "");

                if (Ten.ToLower().Contains("raw") || Ten.ToLower().Contains("eng") || Ten.ToLower().Contains("english"))
                {
                    ChuongHienTai = "[Chương này có vẻ là raw hoặc eng]\n" + " " + IDTruyen + " " + Nguon + " " + Ten;
                    break;
                }

                //Loại bỏ kí tự hạn chế trong tên
                foreach (string KiTu in ListHanChe)
                {
                    try
                    {
                        ChuongHienTai = ChuongHienTai.Replace(KiTu, "");
                        Ten = Ten.Replace(KiTu, "");
                    }
                    catch { }
                }

                //Lấy danh sách ảnh có trong chương
                HttpClient httpClient2 = new HttpClient();
                httpClient2.BaseAddress = new Uri(Link);

                string htmlDanhSachAnh = httpClient2.GetStringAsync("").Result;

                string DanhSachAnhPartem1 = @"<div class=""manga-container"">(.*?)<div  align=""center"" class=""quang-cao-cuoi-trang"">";
                var DanhSachAnh1 = Regex.Matches(htmlDanhSachAnh, DanhSachAnhPartem1, RegexOptions.Singleline);

                string DanhSachAnhPartem2 = @"<img src=""(.*?)""";
                var DanhSachAnh2 = Regex.Matches(DanhSachAnh1[0].ToString(), DanhSachAnhPartem2, RegexOptions.Singleline);

                //Nếu danh sách rỗng tức là xảy ra lỗi không tương thích
                if (DanhSachAnh2.Count == 0)
                {
                    Ten = "[Lỗi tool không còn tương thích với web nguồn]\n" + IDTruyen + " " + Nguon;
                    ChuongHienTai = Ten;

                    break;                   
                }

                DemAnh = 0;

                //Tải các ảnh có trong chương
                foreach (var anh in DanhSachAnh2)
                {
                    LinkAnh = anh.ToString().Replace(@"<img src=""", "");
                    LinkAnh = LinkAnh.Replace(@"""", "");

                    try
                    {
                        if (Directory.Exists(DuongDan + "\\" + Ten) == false)
                        { Directory.CreateDirectory(DuongDan + "\\" + Ten); }

                        WebClient client = new WebClient();
                        client.Headers.Set("Referer", Link);

                        if (DemAnh < 10)
                        { client.DownloadFile(LinkAnh, DuongDan + "\\" + Ten + "\\0" + DemAnh + DinhDang(LinkAnh)); }
                        else
                        { client.DownloadFile(LinkAnh, DuongDan + "\\" + Ten + "\\" + DemAnh + DinhDang(LinkAnh)); }
                    }
                    catch
                    {
                        try
                        {
                            if (Directory.Exists(DuongDan + "\\" + Ten) == false)
                            { Directory.CreateDirectory(DuongDan + "\\" + Ten); }

                            WebClient client = new WebClient();
                            //client.Headers.Set("Referer", Link);

                            if (DemAnh < 10)
                            { client.DownloadFile(LinkAnh, DuongDan + "\\" + Ten + "\\0" + DemAnh + DinhDang(LinkAnh)); }
                            else
                            { client.DownloadFile(LinkAnh, DuongDan + "\\" + Ten + "\\" + DemAnh + DinhDang(LinkAnh)); }
                        }
                        catch
                        {
                            if (Directory.Exists(DuongDan + "\\" + Ten) == true)
                            { Directory.Delete(DuongDan + "\\" + Ten, true); }

                            i = -1; //Tải thất bại thì gán i âm để ngừng vòng lặp ngoài
                            Ten = "[Lỗi tải] " + IDTruyen + " " + Nguon + " " + Ten;

                            break;
                        }

                    }

                    DemAnh++;
                }

                ChuongHienTai = Ten; //Cập nhật lại "chương hiện tại"

                i--;
            }

            return ChuongHienTai;
        }      

        public static string TruyenTranhTuan (string IDTruyen, string Nguon, string ChuongHienTai, int Tre, string DuongDan)
        {
            TaoDanhSachDinhDang();

            HttpClient httpClient = new HttpClient();
            httpClient.BaseAddress = new Uri(Nguon);

            string htmlDanhSachChuong = httpClient.GetStringAsync("").Result;

            //Lấy tên truyện
            string TenTruyen = Regex.Match(htmlDanhSachChuong, @"itemprop=""name"">(.*?)</h1>", RegexOptions.Singleline).Value;
            TenTruyen = TenTruyen.Replace(@"itemprop=""name"">", "").Replace("</h1>", "");


            //Lấy danh sách chương có trong truyện
            string DanhSachChuongPartem1 = @"<div id=""manga-chapter"">(.*?)</div>";
            var DanhSachChuong1 = Regex.Matches(htmlDanhSachChuong, DanhSachChuongPartem1, RegexOptions.Singleline);

            string DanhSachChuongPartem2 = @"<a(.*?)</a>";
            var DanhSachChuong2 = Regex.Matches(DanhSachChuong1[0].ToString(), DanhSachChuongPartem2, RegexOptions.Singleline);

            string Ten = "";
            string Link, LinkAnh = "";
            int DemAnh = 0;

            //int i = DanhSachChuong2.Count -  1 - Tre; //Giới hạn danh sách với độ trễ (giới hạn cuối tùy kiểu danh sách)
            int i = DanhSachChuong2.Count - 1;

            //Nếu danh sách rỗng tức là xảy ra lỗi không tương thích
            if (i == 0)
            {
                i = -1;
                Ten = "[Lỗi tool không còn tương thích với web nguồn]\n" + IDTruyen + " " + Nguon;
            }

            //Tìm tới vị trí chương hiện tại và bắt đầu từ chương kế sau
            string TonTai = "null";
            while (i >= 0 + Tre) //Giới hạn danh sách với độ trễ (giới hạn đầu tùy kiểu danh sách)
            {
                TonTai = "Không";

                var TenChuong = Regex.Matches(DanhSachChuong2[i].ToString(), @">(.*?)<", RegexOptions.Singleline);
                Ten = TenChuong[0].ToString().Replace(@">", "").Replace(@"<", "");
                Ten = Ten.Replace(TenTruyen, "").Trim();

                i--;

                //Loại bỏ kí tự hạn chế trong tên
                foreach (string KiTu in ListHanChe)
                {
                    try
                    {
                        ChuongHienTai = ChuongHienTai.Replace(KiTu, "");
                        Ten = Ten.Replace(KiTu, "");
                    }
                    catch { }
                }

                if (Ten.ToLower().Replace("  "," ").Trim() == ChuongHienTai.ToLower().Replace("  ", " ").Trim())
                {
                    TonTai = "Có";
                    break;
                }
            }

            if (TonTai == "Không")
            {
                ChuongHienTai = "[Lỗi tên chương 2 bên không tương đồng]\n" + IDTruyen + " " + ChuongHienTai + "\n" + Nguon + " " + Ten;
            }

            //Tải anh có trong chương
            while (i >= 0 + Tre) //Giới hạn danh sách với độ trễ
            {
                //Lọc lấy link chương
                var LinkChuong = Regex.Matches(DanhSachChuong2[i].ToString(), @"""(.*?)""", RegexOptions.Singleline);
                Link = LinkChuong[0].ToString().Replace(@"""", "");
                Link = Link.Replace(@"""", "");

                //Lọc lấy tên chương
                var TenChuong = Regex.Matches(DanhSachChuong2[i].ToString(), @">(.*?)<", RegexOptions.Singleline);
                Ten = TenChuong[0].ToString().Replace(@">", "").Replace(@"<", "");
                Ten = Ten.Replace(TenTruyen, "").Trim();

                //Loại bỏ kí tự hạn chế trong tên
                foreach (string KiTu in ListHanChe)
                {
                    try
                    {
                        ChuongHienTai = ChuongHienTai.Replace(KiTu, "");
                        Ten = Ten.Replace(KiTu, "");
                    }
                    catch { }
                }

                //Lấy danh sách ảnh có trong chương
                chromeDriver.Url = Link;
                chromeDriver.Navigate();

                string htmlDanhSachAnh = chromeDriver.PageSource;

                string DanhSachAnhPartem1 = @"<div id=""viewer"">(.*?)</div>";
                var DanhSachAnh1 = Regex.Matches(htmlDanhSachAnh, DanhSachAnhPartem1, RegexOptions.Singleline);

                string DanhSachAnhPartem2 = @"src=""(.*?)""";
                var DanhSachAnh2 = Regex.Matches(DanhSachAnh1[0].ToString(), DanhSachAnhPartem2, RegexOptions.Singleline);

                //Nếu danh sách rỗng tức là xảy ra lỗi không tương thích
                if (DanhSachAnh2.Count == 0)
                {
                    Ten = "[Lỗi tool không còn tương thích với web nguồn]\n" + IDTruyen + " " + Nguon;
                    ChuongHienTai = Ten;

                    break;                   
                }

                DemAnh = 0;

                //Tải các ảnh có trong chương
                foreach (var anh in DanhSachAnh2)
                {
                    LinkAnh = anh.ToString().Replace(@"src=""", "");
                    LinkAnh = LinkAnh.Replace(@"""", "");

                    try
                    {
                        if (Directory.Exists(DuongDan + "\\" + Ten) == false)
                        { Directory.CreateDirectory(DuongDan + "\\" + Ten); }

                        WebClient client = new WebClient();
                        client.Headers.Set("Referer", Link);

                        if (DemAnh < 10)
                        { client.DownloadFile(LinkAnh, DuongDan + "\\" + Ten + "\\0" + DemAnh + DinhDang(LinkAnh)); }
                        else
                        { client.DownloadFile(LinkAnh, DuongDan + "\\" + Ten + "\\" + DemAnh + DinhDang(LinkAnh)); }
                    }
                    catch
                    {
                        try
                        {
                            if (Directory.Exists(DuongDan + "\\" + Ten) == false)
                            { Directory.CreateDirectory(DuongDan + "\\" + Ten); }

                            WebClient client = new WebClient();
                            //client.Headers.Set("Referer", Link);

                            if (DemAnh < 10)
                            { client.DownloadFile(LinkAnh, DuongDan + "\\" + Ten + "\\0" + DemAnh + DinhDang(LinkAnh)); }
                            else
                            { client.DownloadFile(LinkAnh, DuongDan + "\\" + Ten + "\\" + DemAnh + DinhDang(LinkAnh)); }
                        }
                        catch
                        {
                            if (Directory.Exists(DuongDan + "\\" + Ten) == true)
                            { Directory.Delete(DuongDan + "\\" + Ten, true); }

                            i = -1; //Tải thất bại thì gán i âm để ngừng vòng lặp ngoài
                            Ten = "[Lỗi tải] " + IDTruyen + " " + Nguon + " " + Ten;

                            break;
                        }

                    }

                    DemAnh++;
                }

                ChuongHienTai = Ten; //Cập nhật lại "chương hiện tại"

                i--;
            }

            return ChuongHienTai;
        }

        public static string HamTruyen (string IDTruyen, string Nguon, string ChuongHienTai, int Tre, string DuongDan)
        {
            TaoDanhSachDinhDang();

            HttpClient httpClient = new HttpClient();
            httpClient.BaseAddress = new Uri(Nguon);

            string htmlDanhSachChuong = httpClient.GetStringAsync("").Result;

            //Lấy danh sách chương có trong truyện
            string DanhSachChuongPartem1 = @"<div class=""content"">(.*?)<div id=""top_qc_banner""";
            var DanhSachChuong1 = Regex.Matches(htmlDanhSachChuong, DanhSachChuongPartem1, RegexOptions.Singleline);

            string DanhSachChuongPartem2 = @"<a(.*?)</a>";
            var DanhSachChuong2 = Regex.Matches(DanhSachChuong1[0].ToString(), DanhSachChuongPartem2, RegexOptions.Singleline);

            string Ten = "";
            string Link, LinkAnh = "";
            int DemAnh = 0;

            //int i = DanhSachChuong2.Count -  1 - Tre; //Giới hạn danh sách với độ trễ (giới hạn cuối tùy kiểu danh sách)
            int i = DanhSachChuong2.Count - 1;

            //Nếu danh sách rỗng tức là xảy ra lỗi không tương thích
            if (i == 0)
            {
                i = -1;
                Ten = "[Lỗi tool không còn tương thích với web nguồn]\n" + IDTruyen + " " + Nguon;
            }

            //Tìm tới vị trí chương hiện tại và bắt đầu từ chương kế sau
            string TonTai = "null";
            while (i >= 0 + Tre) //Giới hạn danh sách với độ trễ (giới hạn đầu tùy kiểu danh sách)
            {
                TonTai = "Không";

                var TenChuong = Regex.Matches(DanhSachChuong2[i].ToString(), @">(.*?)<", RegexOptions.Singleline);
                Ten = TenChuong[0].ToString().Replace(@">", "");
                Ten = Ten.Replace(@"<", "");

                i--;

                //Loại bỏ kí tự hạn chế trong tên
                foreach (string KiTu in ListHanChe)
                {
                    try
                    {
                        ChuongHienTai = ChuongHienTai.Replace(KiTu, "");
                        Ten = Ten.Replace(KiTu, "");
                    }
                    catch { }
                }

                if (Ten.ToLower().Replace("  "," ").Trim() == ChuongHienTai.ToLower().Replace("  ", " ").Trim())
                {
                    TonTai = "Có";
                    break;
                }
            }

            if (TonTai == "Không")
            {
                ChuongHienTai = "[Lỗi tên chương 2 bên không tương đồng]\n" + IDTruyen + " " + ChuongHienTai + "\n" + Nguon + " " + Ten;
            }

            //Tải anh có trong chương
            while (i >= 0 + Tre) //Giới hạn danh sách với độ trễ
            {
                //Lọc lấy link chương
                var LinkChuong = Regex.Matches(DanhSachChuong2[i].ToString(), @"href=""(.*?)""", RegexOptions.Singleline);
                Link = LinkChuong[0].ToString().Replace(@"href=""", "");
                Link = Link.Replace(@"""", "");
                Link = "https://hamtruyen.com" + Link;

                //Lọc lấy tên chương
                var TenChuong = Regex.Matches(DanhSachChuong2[i].ToString(), @">(.*?)<", RegexOptions.Singleline);
                Ten = TenChuong[0].ToString().Replace(@">", "");
                Ten = Ten.Replace(@"<", "");

                if(Ten.ToLower().Contains("video"))
                {
                    break;
                }

                //Loại bỏ kí tự hạn chế trong tên
                foreach (string KiTu in ListHanChe)
                {
                    try
                    {
                        ChuongHienTai = ChuongHienTai.Replace(KiTu, "");
                        Ten = Ten.Replace(KiTu, "");
                    }
                    catch { }
                }

                //Lấy danh sách ảnh có trong chương
                HttpClient httpClient2 = new HttpClient();
                httpClient2.BaseAddress = new Uri(Link);

                string htmlDanhSachAnh = httpClient2.GetStringAsync("").Result;

                string DanhSachAnhPartem1 = @"<div class=""content_chap"" id=""content_chap"">(.*?)<script>";
                var DanhSachAnh1 = Regex.Matches(htmlDanhSachAnh, DanhSachAnhPartem1, RegexOptions.Singleline);

                string DanhSachAnhPartem2 = @"<img src=(.*?) ";
                var DanhSachAnh2 = Regex.Matches(DanhSachAnh1[0].ToString(), DanhSachAnhPartem2, RegexOptions.Singleline);

                    //Nếu danh sách rỗng tức là xảy ra lỗi không tương thích
                if (DanhSachAnh2.Count == 0)
                {
                    Ten = "[Lỗi tool không còn tương thích với web nguồn]\n" + IDTruyen + " " + Nguon;
                    ChuongHienTai = Ten;

                    break;
                }

                DemAnh = 0;

                //Tải các ảnh có trong chương
                foreach (var anh in DanhSachAnh2)
                {
                    LinkAnh = anh.ToString().Replace(@"<img src=""", "").Replace(@"<img src='", "");
                    LinkAnh = LinkAnh.Replace(@"""", "").Replace("'", "").Trim();

                    try
                    {
                        if (Directory.Exists(DuongDan + "\\" + Ten) == false)
                        { Directory.CreateDirectory(DuongDan + "\\" + Ten); }

                        WebClient client = new WebClient();
                        client.Headers.Set("Referer", Link);

                        if (DemAnh < 10)
                        { client.DownloadFile(LinkAnh, DuongDan + "\\" + Ten + "\\0" + DemAnh + DinhDang(LinkAnh)); }
                        else
                        { client.DownloadFile(LinkAnh, DuongDan + "\\" + Ten + "\\" + DemAnh + DinhDang(LinkAnh)); }
                    }
                    catch
                    {
                        try
                        {
                            if (Directory.Exists(DuongDan + "\\" + Ten) == false)
                            { Directory.CreateDirectory(DuongDan + "\\" + Ten); }

                            WebClient client = new WebClient();
                            //client.Headers.Set("Referer", Link);

                            if (DemAnh < 10)
                            { client.DownloadFile(LinkAnh, DuongDan + "\\" + Ten + "\\0" + DemAnh + DinhDang(LinkAnh)); }
                            else
                            { client.DownloadFile(LinkAnh, DuongDan + "\\" + Ten + "\\" + DemAnh + DinhDang(LinkAnh)); }
                        }
                        catch
                        {
                            if (Directory.Exists(DuongDan + "\\" + Ten) == true)
                            { Directory.Delete(DuongDan + "\\" + Ten, true); }

                            i = -1; //Tải thất bại thì gán i âm để ngừng vòng lặp ngoài
                            Ten = "[Lỗi tải] " + IDTruyen + " " + Nguon + " " + Ten;

                            break;
                        }

                    }

                    DemAnh++;
                }

                ChuongHienTai = Ten; //Cập nhật lại "chương hiện tại"

                i--;
            }

            return ChuongHienTai;
        }

        public static string TruyenSieuHay (string IDTruyen, string Nguon, string ChuongHienTai, int Tre, string DuongDan)
        {
            TaoDanhSachDinhDang();

            HttpClient httpClient = new HttpClient();
            httpClient.BaseAddress = new Uri(Nguon);

            string htmlDanhSachChuong = httpClient.GetStringAsync("").Result;

            //Lấy danh sách chương có trong truyện
            string DanhSachChuongPartem1 = @"<ul>(.*?)</ul>";
            var DanhSachChuong1 = Regex.Matches(htmlDanhSachChuong, DanhSachChuongPartem1, RegexOptions.Singleline);

            string DanhSachChuongPartem2 = @"<a(.*?)</a>";
            var DanhSachChuong2 = Regex.Matches(DanhSachChuong1[0].ToString(), DanhSachChuongPartem2, RegexOptions.Singleline);

            string Ten = "";
            string Link, LinkAnh = "";
            int DemAnh = 0;

            #region Lấy các chương có ảnh và không bị khóa (xử lí riêng cho TruyenSieuHay)

            List<string> ListChuong = new List<string>();

            string TrangThai = "Bỏ qua";

            foreach(var info in DanhSachChuong2)
            {
                if(info.ToString().Contains("lock") == false && info.ToString().Contains("video") == false)
                {
                    TrangThai = "Lấy";                   
                }

                if(TrangThai == "Lấy")
                {
                    ListChuong.Add(info.ToString().Replace("video","").Trim());
                }
               
            }

            #endregion

            //int i = DanhSachChuong2.Count -  1 - Tre; //Giới hạn danh sách với độ trễ (giới hạn cuối tùy kiểu danh sách)
            int i = ListChuong.Count - 1;

            //Nếu danh sách rỗng tức là xảy ra lỗi không tương thích
            if (i == 0)
            {
                i = -1;
                Ten = "[Lỗi tool không còn tương thích với web nguồn]\n" + IDTruyen + " " + Nguon;
            }

            //Tìm tới vị trí chương hiện tại và bắt đầu từ chương kế sau
            string TonTai = "null";
            while (i >= 0 + Tre) //Giới hạn danh sách với độ trễ (giới hạn đầu tùy kiểu danh sách)
            {
                TonTai = "Không";

                var TenChuong = Regex.Matches(ListChuong[i].ToString(), @">(.*?)<", RegexOptions.Singleline);
                Ten = TenChuong[0].ToString().Replace(@">", "");
                Ten = Ten.Replace(@"<", "");

                i--;

                //Loại bỏ kí tự hạn chế trong tên
                foreach (string KiTu in ListHanChe)
                {
                    try
                    {
                        ChuongHienTai = ChuongHienTai.Replace(KiTu, "");
                        Ten = Ten.Replace(KiTu, "");
                    }
                    catch { }
                }

                if (Ten.ToLower().Replace("  "," ").Trim() == ChuongHienTai.ToLower().Replace("  ", " ").Trim())
                {
                    TonTai = "Có";
                    break;
                }
            }

            if (TonTai == "Không")
            {
                ChuongHienTai = "[Lỗi tên chương 2 bên không tương đồng]\n" + IDTruyen + " " + ChuongHienTai + "\n" + Nguon + " " + Ten;
            }

            //Tải anh có trong chương
            while (i >= 0 + Tre) //Giới hạn danh sách với độ trễ
            {
                //Lọc lấy link chương
                var LinkChuong = Regex.Matches(ListChuong[i].ToString(), @"href=""(.*?)""", RegexOptions.Singleline);
                Link = LinkChuong[0].ToString().Replace(@"href=""", "");
                Link = Link.Replace(@"""", "");
                Link = "https://truyensieuhay.com" + Link;

                //Lọc lấy tên chương
                var TenChuong = Regex.Matches(ListChuong[i].ToString(), @">(.*?)<", RegexOptions.Singleline);
                Ten = TenChuong[0].ToString().Replace(@">", "");
                Ten = Ten.Replace(@"<", "");

                //Loại bỏ kí tự hạn chế trong tên
                foreach (string KiTu in ListHanChe)
                {
                    try
                    {
                        ChuongHienTai = ChuongHienTai.Replace(KiTu, "");
                        Ten = Ten.Replace(KiTu, "");
                    }
                    catch { }
                }

                //Lấy danh sách ảnh có trong chương
                chromeDriver.Url = Link;
                chromeDriver.Navigate();

                string htmlDanhSachAnh = chromeDriver.PageSource;

                string DanhSachAnhPartem1 = @"<div class=""lst_image text-center"">(.*?)<script>";
                var DanhSachAnh1 = Regex.Matches(htmlDanhSachAnh, DanhSachAnhPartem1, RegexOptions.Singleline);

                string DanhSachAnhPartem2 = @"<img src=""(.*?)""";
                var DanhSachAnh2 = Regex.Matches(DanhSachAnh1[0].ToString(), DanhSachAnhPartem2, RegexOptions.Singleline);

                if (DanhSachAnh2.Count == 0)
                {
                    DanhSachAnhPartem2 = @"com"" src=""(.*?)""";
                    DanhSachAnh2 = Regex.Matches(DanhSachAnh1[0].ToString(), DanhSachAnhPartem2, RegexOptions.Singleline);
                }

                //Nếu danh sách rỗng tức là xảy ra lỗi không tương thích
                if (DanhSachAnh2.Count == 0)
                {
                    Ten = "[Lỗi tool không còn tương thích với web nguồn]\n" + IDTruyen + " " + Nguon;
                    ChuongHienTai = Ten;

                    break;
                }

                DemAnh = 0;

                //Tải các ảnh có trong chương
                foreach (var anh in DanhSachAnh2)
                {
                    LinkAnh = anh.ToString().Replace(@"com"" src=""", "").Replace(@"<img src=""", "");
                    LinkAnh = LinkAnh.Replace(@"""", "");

                    try
                    {                       
                        if (Directory.Exists(DuongDan + "\\" + Ten) == false)
                        { Directory.CreateDirectory(DuongDan + "\\" + Ten); }

                        WebClient client = new WebClient();
                        client.Headers.Set("Referer", Link);

                        if (DemAnh < 10)
                        { client.DownloadFile(LinkAnh, DuongDan + "\\" + Ten + "\\0" + DemAnh + DinhDang(LinkAnh)); }
                        else
                        { client.DownloadFile(LinkAnh, DuongDan + "\\" + Ten + "\\" + DemAnh + DinhDang(LinkAnh)); }
                    }
                    catch
                    {
                        try
                        {
                            if (Directory.Exists(DuongDan + "\\" + Ten) == false)
                            { Directory.CreateDirectory(DuongDan + "\\" + Ten); }

                            WebClient client = new WebClient();
                            //client.Headers.Set("Referer", Link);

                            if (DemAnh < 10)
                            { client.DownloadFile(LinkAnh, DuongDan + "\\" + Ten + "\\0" + DemAnh + DinhDang(LinkAnh)); }
                            else
                            { client.DownloadFile(LinkAnh, DuongDan + "\\" + Ten + "\\" + DemAnh + DinhDang(LinkAnh)); }
                        }
                        catch
                        {
                            if (Directory.Exists(DuongDan + "\\" + Ten) == true)
                            { Directory.Delete(DuongDan + "\\" + Ten, true); }

                            i = -1; //Tải thất bại thì gán i âm để ngừng vòng lặp ngoài
                            Ten = "[Lỗi tải] " + IDTruyen + " " + Nguon + " " + Ten;

                            break;
                        }
                        
                    }

                    DemAnh++;
                }

                ChuongHienTai = Ten; //Cập nhật lại "chương hiện tại"

                i--;
            }

            return ChuongHienTai;
        }

        public static string NetTruyen (string IDTruyen, string Nguon, string ChuongHienTai, int Tre, string DuongDan)
        {
            TaoDanhSachDinhDang();

            HttpClient httpClient = new HttpClient();
            httpClient.BaseAddress = new Uri(Nguon);

            string htmlDanhSachChuong = httpClient.GetStringAsync("").Result;

            //Lấy danh sách chương có trong truyện
            string DanhSachChuongPartem1 = @"<nav>(.*?)</nav>";
            var DanhSachChuong1 = Regex.Matches(htmlDanhSachChuong, DanhSachChuongPartem1, RegexOptions.Singleline);

            string DanhSachChuongPartem2 = @"<div class=""col-xs-5 chapter"">(.*?)</li>";
            var DanhSachChuong2 = Regex.Matches(DanhSachChuong1[0].ToString(), DanhSachChuongPartem2, RegexOptions.Singleline);

            string Ten = "";
            string Link, LinkAnh = "";
            int DemAnh = 0;

            //int i = DanhSachChuong2.Count -  1 - Tre; //Giới hạn danh sách với độ trễ (giới hạn cuối tùy kiểu danh sách)
            int i = DanhSachChuong2.Count - 1;

            //Nếu danh sách rỗng tức là xảy ra lỗi không tương thích
            if (i == 0)
            {
                i = -1;
                Ten = "[Lỗi tool không còn tương thích với web nguồn]\n" + IDTruyen + " " + Nguon;
            }

            //Tìm tới vị trí chương hiện tại và bắt đầu từ chương kế sau
            string TonTai = "null";
            while (i >= 0 + Tre) //Giới hạn danh sách với độ trễ (giới hạn đầu tùy kiểu danh sách)
            {
                TonTai = "Không";

                var TenChuong = Regex.Matches(DanhSachChuong2[i].ToString(), @">(.*?)<", RegexOptions.Singleline);
                Ten = TenChuong[1].ToString().Replace(@">", "");
                Ten = Ten.Replace(@"<", "");

                i--;

                //Loại bỏ kí tự hạn chế trong tên
                foreach (string KiTu in ListHanChe)
                {
                    try
                    {
                        ChuongHienTai = ChuongHienTai.Replace(KiTu, "");
                        Ten = Ten.Replace(KiTu, "");
                    }
                    catch { }
                }

                if (Ten.ToLower().Replace("  "," ").Trim() == ChuongHienTai.ToLower().Replace("  ", " ").Trim())
                {
                    TonTai = "Có";
                    break;
                }
            }

            if (TonTai == "Không")
            {
                ChuongHienTai = "[Lỗi tên chương 2 bên không tương đồng]\n" + IDTruyen + " " + ChuongHienTai + "\n" + Nguon + " " + Ten;
            }

            //Tải anh có trong chương
            while (i >= 0 + Tre) //Giới hạn danh sách với độ trễ
            {
                //Lọc lấy link chương
                var LinkChuong = Regex.Matches(DanhSachChuong2[i].ToString(), @"href=""(.*?)""", RegexOptions.Singleline);
                Link = LinkChuong[0].ToString().Replace(@"href=""", "");
                Link = Link.Replace(@"""", "");

                //Lọc lấy tên chương
                var TenChuong = Regex.Matches(DanhSachChuong2[i].ToString(), @">(.*?)<", RegexOptions.Singleline);
                Ten = TenChuong[1].ToString().Replace(@">", "");
                Ten = Ten.Replace(@"<", "");

                if (Ten.ToLower().Contains("raw") || Ten.ToLower().Contains("eng"))
                {
                    ChuongHienTai = "[Chương này có vẻ là raw hoặc eng]\n" + " " + IDTruyen + " " + Nguon + " " + Ten;
                    break;
                }

                //Loại bỏ kí tự hạn chế trong tên
                foreach (string KiTu in ListHanChe)
                {
                    try
                    {
                        ChuongHienTai = ChuongHienTai.Replace(KiTu, "");
                        Ten = Ten.Replace(KiTu, "");
                    }
                    catch { }
                }

                //Lấy danh sách ảnh có trong chương
                chromeDriver.Url = Link;
                chromeDriver.Navigate();

                string htmlDanhSachAnh = chromeDriver.PageSource;

                string DanhSachAnhPartem1 = @"<div class=""reading-detail box_doc"">(.*?)<script src=""http://";
                var DanhSachAnh1 = Regex.Matches(htmlDanhSachAnh, DanhSachAnhPartem1, RegexOptions.Singleline);

                string DanhSachAnhPartem2 = @"<div id=""page(.*?)"" data-original";
                var DanhSachAnh2 = Regex.Matches(DanhSachAnh1[0].ToString(), DanhSachAnhPartem2, RegexOptions.Singleline);

                //Nếu danh sách rỗng tức là xảy ra lỗi không tương thích
                if (DanhSachAnh2.Count == 0)
                {
                    Ten = "[Lỗi tool không còn tương thích với web nguồn]\n" + IDTruyen + " " + Nguon;
                    ChuongHienTai = Ten;

                    break;
                }

                DemAnh = 0;

                //Tải các ảnh có trong chương
                foreach (var anh in DanhSachAnh2)
                {
                    LinkAnh = Regex.Match(anh.ToString(), @"src=""(.*?)""", RegexOptions.Singleline).Value;
                    LinkAnh = LinkAnh.Replace(@"src=""", "");
                    LinkAnh = LinkAnh.Replace(@"""", "");

                    try
                    {
                        if (Directory.Exists(DuongDan + "\\" + Ten) == false)
                        { Directory.CreateDirectory(DuongDan + "\\" + Ten); }

                        WebClient client = new WebClient();
                        client.Headers.Set("Referer", Link);

                        if (DemAnh < 10)
                        { client.DownloadFile(LinkAnh, DuongDan + "\\" + Ten + "\\0" + DemAnh + DinhDang(LinkAnh)); }
                        else
                        { client.DownloadFile(LinkAnh, DuongDan + "\\" + Ten + "\\" + DemAnh + DinhDang(LinkAnh)); }
                    }
                    catch
                    {
                        try
                        {
                            if (Directory.Exists(DuongDan + "\\" + Ten) == false)
                            { Directory.CreateDirectory(DuongDan + "\\" + Ten); }

                            WebClient client = new WebClient();
                            //client.Headers.Set("Referer", Link);

                            if (DemAnh < 10)
                            { client.DownloadFile(LinkAnh, DuongDan + "\\" + Ten + "\\0" + DemAnh + DinhDang(LinkAnh)); }
                            else
                            { client.DownloadFile(LinkAnh, DuongDan + "\\" + Ten + "\\" + DemAnh + DinhDang(LinkAnh)); }
                        }
                        catch
                        {
                            if (Directory.Exists(DuongDan + "\\" + Ten) == true)
                            { Directory.Delete(DuongDan + "\\" + Ten, true); }

                            i = -1; //Tải thất bại thì gán i âm để ngừng vòng lặp ngoài
                            Ten = "[Lỗi tải] " + IDTruyen + " " + Nguon + " " + Ten;

                            break;
                        }

                    }

                    DemAnh++;
                }

                ChuongHienTai = Ten; //Cập nhật lại "chương hiện tại"

                i--;
            }

            return ChuongHienTai;
        }

        public static string TruyenChon (string IDTruyen, string Nguon, string ChuongHienTai, int Tre, string DuongDan)
        {
            TaoDanhSachDinhDang();

            HttpClient httpClient = new HttpClient();
            httpClient.BaseAddress = new Uri(Nguon);

            string htmlDanhSachChuong = httpClient.GetStringAsync("").Result;

            //Lấy danh sách chương có trong truyện
            string DanhSachChuongPartem1 = @"<ul>(.*?)</nav>";
            var DanhSachChuong1 = Regex.Matches(htmlDanhSachChuong, DanhSachChuongPartem1, RegexOptions.Singleline);

            string DanhSachChuongPartem2 = @"<a href(.*?)</a>";
            var DanhSachChuong2 = Regex.Matches(DanhSachChuong1[0].ToString(), DanhSachChuongPartem2, RegexOptions.Singleline);

            string Ten = "";
            string Link, LinkAnh = "";
            int DemAnh = 0;

            //int i = DanhSachChuong2.Count -  1 - Tre; //Giới hạn danh sách với độ trễ (giới hạn cuối tùy kiểu danh sách)
            int i = DanhSachChuong2.Count - 1;

            //Nếu danh sách rỗng tức là xảy ra lỗi không tương thích
            if (i == 0)
            {
                i = -1;
                Ten = "[Lỗi tool không còn tương thích với web nguồn]\n" + IDTruyen + " " + Nguon;
            }

            //Tìm tới vị trí chương hiện tại và bắt đầu từ chương kế sau
            string TonTai = "null";
            while (i >= 0 + Tre) //Giới hạn danh sách với độ trễ (giới hạn đầu tùy kiểu danh sách)
            {
                TonTai = "Không";

                var TenChuong = Regex.Matches(DanhSachChuong2[i].ToString(), @">(.*?)<", RegexOptions.Singleline);
                Ten = TenChuong[0].ToString().Replace(@">", "");
                Ten = Ten.Replace(@"<", "");

                i--;

                //Loại bỏ kí tự hạn chế trong tên
                foreach (string KiTu in ListHanChe)
                {
                    try
                    {
                        ChuongHienTai = ChuongHienTai.Replace(KiTu, "");
                        Ten = Ten.Replace(KiTu, "");
                    }
                    catch { }
                }

                if (Ten.ToLower().Replace("  "," ").Trim() == ChuongHienTai.ToLower().Replace("  ", " ").Trim())
                {
                    TonTai = "Có";
                    break;
                }
            }

            if (TonTai == "Không")
            {
                ChuongHienTai = "[Lỗi tên chương 2 bên không tương đồng]\n" + IDTruyen + " " + ChuongHienTai + "\n" + Nguon + " " + Ten;
            }

            //Tải anh có trong chương
            while (i >= 0 + Tre) //Giới hạn danh sách với độ trễ
            {
                //Lọc lấy link chương
                var LinkChuong = Regex.Matches(DanhSachChuong2[i].ToString(), @"href=""(.*?)""", RegexOptions.Singleline);
                Link = LinkChuong[0].ToString().Replace(@"href=""", "");
                Link = Link.Replace(@"""", "");

                //Lọc lấy tên chương
                var TenChuong = Regex.Matches(DanhSachChuong2[i].ToString(), @">(.*?)<", RegexOptions.Singleline);
                Ten = TenChuong[0].ToString().Replace(@">", "");
                Ten = Ten.Replace(@"<", "");

                if (Ten.ToLower().Contains("raw") || Ten.ToLower().Contains("eng"))
                {
                    ChuongHienTai = "[Chương này có vẻ là raw hoặc eng]\n" + " " + IDTruyen + " " + Nguon + " " + Ten;
                    break;
                }

                //Loại bỏ kí tự hạn chế trong tên
                foreach (string KiTu in ListHanChe)
                {
                    try
                    {
                        ChuongHienTai = ChuongHienTai.Replace(KiTu, "");
                        Ten = Ten.Replace(KiTu, "");
                    }
                    catch { }
                }

                //Lấy danh sách ảnh có trong chương
                chromeDriver.Url = Link;
                chromeDriver.Navigate();

                string htmlDanhSachAnh = chromeDriver.PageSource;

                string DanhSachAnhPartem1 = @"<div class=""reading-detail box_doc"">(.*?)<div class=""container mrt5"">";
                var DanhSachAnh1 = Regex.Matches(htmlDanhSachAnh, DanhSachAnhPartem1, RegexOptions.Singleline);

                string DanhSachAnhPartem2 = @"<div id=""page(.*?)"" data-original";
                var DanhSachAnh2 = Regex.Matches(DanhSachAnh1[0].ToString(), DanhSachAnhPartem2, RegexOptions.Singleline);

                //Nếu danh sách rỗng tức là xảy ra lỗi không tương thích
                if (DanhSachAnh2.Count == 0)
                {
                    Ten = "[Lỗi tool không còn tương thích với web nguồn]\n" + IDTruyen + " " + Nguon;
                    ChuongHienTai = Ten;

                    break;
                }

                DemAnh = 0;

                //Tải các ảnh có trong chương
                foreach (var anh in DanhSachAnh2)
                {
                    LinkAnh = Regex.Match(anh.ToString(), @"src=""(.*?)""", RegexOptions.Singleline).Value;
                    LinkAnh = LinkAnh.Replace(@"src=""", "");
                    LinkAnh = LinkAnh.Replace(@"""", "");

                    try
                    {
                        if (Directory.Exists(DuongDan + "\\" + Ten) == false)
                        { Directory.CreateDirectory(DuongDan + "\\" + Ten); }

                        WebClient client = new WebClient();
                        client.Headers.Set("Referer", Link);

                        if (DemAnh < 10)
                        { client.DownloadFile(LinkAnh, DuongDan + "\\" + Ten + "\\0" + DemAnh + DinhDang(LinkAnh)); }
                        else
                        { client.DownloadFile(LinkAnh, DuongDan + "\\" + Ten + "\\" + DemAnh + DinhDang(LinkAnh)); }
                    }
                    catch
                    {
                        try
                        {
                            if (Directory.Exists(DuongDan + "\\" + Ten) == false)
                            { Directory.CreateDirectory(DuongDan + "\\" + Ten); }

                            WebClient client = new WebClient();
                            //client.Headers.Set("Referer", Link);

                            if (DemAnh < 10)
                            { client.DownloadFile(LinkAnh, DuongDan + "\\" + Ten + "\\0" + DemAnh + DinhDang(LinkAnh)); }
                            else
                            { client.DownloadFile(LinkAnh, DuongDan + "\\" + Ten + "\\" + DemAnh + DinhDang(LinkAnh)); }
                        }
                        catch
                        {
                            if (Directory.Exists(DuongDan + "\\" + Ten) == true)
                            { Directory.Delete(DuongDan + "\\" + Ten, true); }

                            i = -1; //Tải thất bại thì gán i âm để ngừng vòng lặp ngoài
                            Ten = "[Lỗi tải] " + IDTruyen + " " + Nguon + " " + Ten;

                            break;
                        }

                    }

                    DemAnh++;
                }

                ChuongHienTai = Ten; //Cập nhật lại "chương hiện tại"

                i--;
            }

            return ChuongHienTai;
        }

        public static string OCuMeo (string IDTruyen, string Nguon, string ChuongHienTai, int Tre, string DuongDan)
        {
            TaoDanhSachDinhDang();

            chromeDriver.Url = Nguon;
            chromeDriver.Navigate();

            string htmlDanhSachChuong = chromeDriver.PageSource;

            //Lấy tên truyện
            string Tentruyen = Regex.Match(htmlDanhSachChuong, @"<h2 class=""info-title""(.*?)</h2>", RegexOptions.Singleline).Value;
            var MangaName = Regex.Matches(Tentruyen, @">(.*?)</", RegexOptions.Singleline);
            Tentruyen = MangaName[0].ToString().Replace(">", "").Replace("<", "").Replace("/", "");

            //Lấy danh sách chương có trong truyện
            string DanhSachChuongPartem1 = @"<tbody>(.*?)</tbody>";
            var DanhSachChuong1 = Regex.Matches(htmlDanhSachChuong, DanhSachChuongPartem1, RegexOptions.Singleline);

            string DanhSachChuongPartem2 = @"<a(.*?)</a>";
            var DanhSachChuong2 = Regex.Matches(DanhSachChuong1[0].ToString(), DanhSachChuongPartem2, RegexOptions.Singleline);

            string Ten = "";
            string Link, LinkAnh = "";
            int DemAnh = 0;

            //int i = DanhSachChuong2.Count -  1 - Tre; //Giới hạn danh sách với độ trễ (giới hạn cuối tùy kiểu danh sách)
            int i = DanhSachChuong2.Count - 1;

            //Nếu danh sách rỗng tức là xảy ra lỗi không tương thích
            if (i == 0)
            {
                i = -1;
                Ten = "[Lỗi tool không còn tương thích với web nguồn]\n" + IDTruyen + " " + Nguon;
            }

            //Tìm tới vị trí chương hiện tại và bắt đầu từ chương kế sau
            string TonTai = "null";
            while (i >= 0 + Tre) //Giới hạn danh sách với độ trễ (giới hạn đầu tùy kiểu danh sách)
            {
                TonTai = "Không";

                var TenChuong = Regex.Matches(DanhSachChuong2[i].ToString(), @">(.*?)<", RegexOptions.Singleline);
                Ten = TenChuong[0].ToString().Replace(@">", "");
                Ten = Ten.Replace(@"<", "").Replace(Tentruyen, "").Replace("–", "").Trim();

                i--;

                //Loại bỏ kí tự hạn chế trong tên
                foreach (string KiTu in ListHanChe)
                {
                    try
                    {
                        ChuongHienTai = ChuongHienTai.Replace(KiTu, "");
                        Ten = Ten.Replace(KiTu, "");
                    }
                    catch { }
                }

                if (Ten.ToLower().Replace("  "," ").Trim() == ChuongHienTai.ToLower().Replace("  ", " ").Trim())
                {
                    TonTai = "Có";
                    break;
                }
            }

            if (TonTai == "Không")
            {
                ChuongHienTai = "[Lỗi tên chương 2 bên không tương đồng]\n" + IDTruyen + " " + ChuongHienTai + "\n" + Nguon + " " + Ten;
            }

            //Tải anh có trong chương
            while (i >= 0 + Tre) //Giới hạn danh sách với độ trễ
            {
                //Lọc lấy link chương
                var LinkChuong = Regex.Matches(DanhSachChuong2[i].ToString(), @"href=""(.*?)""", RegexOptions.Singleline);
                Link = LinkChuong[0].ToString().Replace(@"href=""", "");
                Link = Link.Replace(@"""", "");

                //Lọc lấy tên chương
                var TenChuong = Regex.Matches(DanhSachChuong2[i].ToString(), @">(.*?)<", RegexOptions.Singleline);
                Ten = TenChuong[0].ToString().Replace(@">", "");
                Ten = Ten.Replace(@"<", "").Replace(Tentruyen, "").Replace("–", "").Trim();

                //Loại bỏ kí tự hạn chế trong tên
                foreach (string KiTu in ListHanChe)
                {
                    try
                    {
                        ChuongHienTai = ChuongHienTai.Replace(KiTu, "");
                        Ten = Ten.Replace(KiTu, "");
                    }
                    catch { }
                }

                //Lấy danh sách ảnh có trong chương
                chromeDriver.Url = Link;
                chromeDriver.Navigate();

                string htmlDanhSachAnh = chromeDriver.PageSource;

                string DanhSachAnhPartem1 = @"<section id=""view-chapter(.*?)</section>";
                var DanhSachAnh1 = Regex.Matches(htmlDanhSachAnh, DanhSachAnhPartem1, RegexOptions.Singleline);

                string DanhSachAnhPartem2 = @"<img src=""(.*?)""";
                var DanhSachAnh2 = Regex.Matches(DanhSachAnh1[0].ToString(), DanhSachAnhPartem2, RegexOptions.Singleline);

                //Nếu danh sách rỗng tức là xảy ra lỗi không tương thích
                if (DanhSachAnh2.Count == 0)
                {
                    Ten = "[Lỗi tool không còn tương thích với web nguồn]\n" + IDTruyen + " " + Nguon;
                    ChuongHienTai = Ten;

                    break;
                }

                DemAnh = 0;

                //Tải các ảnh có trong chương
                foreach (var anh in DanhSachAnh2)
                {
                    LinkAnh = anh.ToString().Replace(@"<img src=""", "");
                    LinkAnh = LinkAnh.Replace(@"""", "");

                    try
                    {
                        if (Directory.Exists(DuongDan + "\\" + Ten) == false)
                        { Directory.CreateDirectory(DuongDan + "\\" + Ten); }

                        WebClient client = new WebClient();
                        client.Headers.Set("Referer", Link);

                        if (DemAnh < 10)
                        { client.DownloadFile(LinkAnh, DuongDan + "\\" + Ten + "\\0" + DemAnh + DinhDang(LinkAnh)); }
                        else
                        { client.DownloadFile(LinkAnh, DuongDan + "\\" + Ten + "\\" + DemAnh + DinhDang(LinkAnh)); }
                    }
                    catch
                    {
                        try
                        {
                            if (Directory.Exists(DuongDan + "\\" + Ten) == false)
                            { Directory.CreateDirectory(DuongDan + "\\" + Ten); }

                            WebClient client = new WebClient();
                            //client.Headers.Set("Referer", Link);

                            if (DemAnh < 10)
                            { client.DownloadFile(LinkAnh, DuongDan + "\\" + Ten + "\\0" + DemAnh + DinhDang(LinkAnh)); }
                            else
                            { client.DownloadFile(LinkAnh, DuongDan + "\\" + Ten + "\\" + DemAnh + DinhDang(LinkAnh)); }
                        }
                        catch
                        {
                            if (Directory.Exists(DuongDan + "\\" + Ten) == true)
                            { Directory.Delete(DuongDan + "\\" + Ten, true); }

                            i = -1; //Tải thất bại thì gán i âm để ngừng vòng lặp ngoài
                            Ten = "[Lỗi tải] " + IDTruyen + " " + Nguon + " " + Ten;

                            break;
                        }

                    }

                    DemAnh++;
                }

                ChuongHienTai = Ten; //Cập nhật lại "chương hiện tại"

                i--;
            }

            return ChuongHienTai;
        }

        public static string Truyen1 (string IDTruyen, string Nguon, string ChuongHienTai, int Tre, string DuongDan)
        {
            TaoDanhSachDinhDang();

            HttpClient httpClient = new HttpClient();
            httpClient.BaseAddress = new Uri(Nguon);

            string htmlDanhSachChuong = httpClient.GetStringAsync("").Result;

            //Lấy danh sách chương có trong truyện
            string DanhSachChuongPartem1 = @"<div id=""MainContent_CenterContent_detailStoryControl_listChapter"" class=""listChapter"">(.*?)<script";
            var DanhSachChuong1 = Regex.Matches(htmlDanhSachChuong, DanhSachChuongPartem1, RegexOptions.Singleline);

            string DanhSachChuongPartem2 = @"cellChapter""><a(.*?)</a>";
            var DanhSachChuong2 = Regex.Matches(DanhSachChuong1[0].ToString(), DanhSachChuongPartem2, RegexOptions.Singleline);

            string Ten = "";
            string Link, LinkAnh = "";
            int DemAnh = 0;

            //int i = DanhSachChuong2.Count -  1 - Tre; //Giới hạn danh sách với độ trễ (giới hạn cuối tùy kiểu danh sách)
            int i = DanhSachChuong2.Count - 1;

            //Nếu danh sách rỗng tức là xảy ra lỗi không tương thích
            if (i == 0)
            {
                i = -1;
                Ten = "[Lỗi tool không còn tương thích với web nguồn]\n" + IDTruyen + " " + Nguon;
            }

            //Tìm tới vị trí chương hiện tại và bắt đầu từ chương kế sau
            string TonTai = "null";
            while (i >= 0 + Tre) //Giới hạn danh sách với độ trễ (giới hạn đầu tùy kiểu danh sách)
            {
                TonTai = "Không";

                var TenChuong = Regex.Matches(DanhSachChuong2[i].ToString(), @">(.*?)<", RegexOptions.Singleline);
                Ten = TenChuong[1].ToString().Replace(@">", "");
                Ten = Ten.Replace(@"<", "");

                i--;

                //Loại bỏ kí tự hạn chế trong tên
                foreach (string KiTu in ListHanChe)
                {
                    try
                    {
                        ChuongHienTai = ChuongHienTai.Replace(KiTu, "");
                        Ten = Ten.Replace(KiTu, "");
                    }
                    catch { }
                }

                if (Ten.ToLower().Replace("  "," ").Trim() == ChuongHienTai.ToLower().Replace("  ", " ").Trim())
                {
                    TonTai = "Có";
                    break;
                }
            }

            if (TonTai == "Không")
            {
                ChuongHienTai = "[Lỗi tên chương 2 bên không tương đồng]\n" + IDTruyen + " " + ChuongHienTai + "\n" + Nguon + " " + Ten;
            }

            //Tải anh có trong chương
            while (i >= 0 + Tre) //Giới hạn danh sách với độ trễ
            {
                //Lọc lấy link chương
                var LinkChuong = Regex.Matches(DanhSachChuong2[i].ToString(), @"href=""(.*?)""", RegexOptions.Singleline);
                Link = LinkChuong[0].ToString().Replace(@"href=""", "");
                Link = Link.Replace(@"""", "");
                Link = "http://truyen1.net" + Link;

                //Lọc lấy tên chương
                var TenChuong = Regex.Matches(DanhSachChuong2[i].ToString(), @">(.*?)<", RegexOptions.Singleline);
                Ten = TenChuong[1].ToString().Replace(@">", "");
                Ten = Ten.Replace(@"<", "");

                //Loại bỏ kí tự hạn chế trong tên
                foreach (string KiTu in ListHanChe)
                {
                    try
                    {
                        ChuongHienTai = ChuongHienTai.Replace(KiTu, "");
                        Ten = Ten.Replace(KiTu, "");
                    }
                    catch { }
                }

                //Lấy danh sách ảnh có trong chương
                chromeDriver.Url = Link;
                chromeDriver.Navigate();

                string htmlDanhSachAnh = chromeDriver.PageSource;

                string DanhSachAnhPartem1 = @"<div id=""containerListPage"">(.*?)id=""adBannerBottom";
                var DanhSachAnh1 = Regex.Matches(htmlDanhSachAnh, DanhSachAnhPartem1, RegexOptions.Singleline);

                string DanhSachAnhPartem2 = @"<img src=""(.*?)""";
                var DanhSachAnh2 = Regex.Matches(DanhSachAnh1[0].ToString(), DanhSachAnhPartem2, RegexOptions.Singleline);

                //Nếu danh sách rỗng tức là xảy ra lỗi không tương thích
                if (DanhSachAnh2.Count == 0)
                {
                    Ten = "[Lỗi tool không còn tương thích với web nguồn]\n" + IDTruyen + " " + Nguon;
                    ChuongHienTai = Ten;

                    break;
                }

                DemAnh = 0;

                //Tải các ảnh có trong chương
                foreach (var anh in DanhSachAnh2)
                {
                    LinkAnh = anh.ToString().Replace(@"<img src=""", "");
                    LinkAnh = LinkAnh.Replace(@"""", "");

                    try
                    {
                        if (Directory.Exists(DuongDan + "\\" + Ten) == false)
                        { Directory.CreateDirectory(DuongDan + "\\" + Ten); }

                        WebClient client = new WebClient();
                        client.Headers.Set("Referer", Link);

                        if (DemAnh < 10)
                        { client.DownloadFile(LinkAnh, DuongDan + "\\" + Ten + "\\0" + DemAnh + DinhDang(LinkAnh)); }
                        else
                        { client.DownloadFile(LinkAnh, DuongDan + "\\" + Ten + "\\" + DemAnh + DinhDang(LinkAnh)); }
                    }
                    catch
                    {
                        try
                        {
                            if (Directory.Exists(DuongDan + "\\" + Ten) == false)
                            { Directory.CreateDirectory(DuongDan + "\\" + Ten); }

                            WebClient client = new WebClient();
                            //client.Headers.Set("Referer", Link);

                            if (DemAnh < 10)
                            { client.DownloadFile(LinkAnh, DuongDan + "\\" + Ten + "\\0" + DemAnh + DinhDang(LinkAnh)); }
                            else
                            { client.DownloadFile(LinkAnh, DuongDan + "\\" + Ten + "\\" + DemAnh + DinhDang(LinkAnh)); }
                        }
                        catch
                        {
                            if (Directory.Exists(DuongDan + "\\" + Ten) == true)
                            { Directory.Delete(DuongDan + "\\" + Ten, true); }

                            i = -1; //Tải thất bại thì gán i âm để ngừng vòng lặp ngoài
                            Ten = "[Lỗi tải] " + IDTruyen + " " + Nguon + " " + Ten;

                            break;
                        }

                    }

                    DemAnh++;
                }

                ChuongHienTai = Ten; //Cập nhật lại "chương hiện tại"

                i--;
            }

            return ChuongHienTai;
        }

        public static string A3Manga (string IDTruyen, string Nguon, string ChuongHienTai, int Tre, string DuongDan)
        {
            TaoDanhSachDinhDang();

            chromeDriver.Url = Nguon;
            chromeDriver.Navigate();

            string htmlDanhSachChuong = chromeDriver.PageSource;

            //Lấy tên truyện
            string TenTruyen = Regex.Match(htmlDanhSachChuong, @"<h2 class=""info-title"">(.*?)</h2>", RegexOptions.Singleline).Value;
            TenTruyen = TenTruyen.Replace(@"<h2 class=""info-title"">", "").Replace(@"</h2>", "");

            //Lấy danh sách chương có trong truyện
            string DanhSachChuongPartem1 = @"<tbody>(.*?)</tbody>";
            var DanhSachChuong1 = Regex.Matches(htmlDanhSachChuong, DanhSachChuongPartem1, RegexOptions.Singleline);

            string DanhSachChuongPartem2 = @"<a(.*?)</a>";
            var DanhSachChuong2 = Regex.Matches(DanhSachChuong1[0].ToString(), DanhSachChuongPartem2, RegexOptions.Singleline);

            string Ten = "";
            string Link, LinkAnh = "";
            int DemAnh = 0;

            //int i = DanhSachChuong2.Count -  1 - Tre; //Giới hạn danh sách với độ trễ (giới hạn cuối tùy kiểu danh sách)
            int i = DanhSachChuong2.Count - 1;

            //Nếu danh sách rỗng tức là xảy ra lỗi không tương thích
            if (i == 0)
            {
                i = -1;
                Ten = "[Lỗi tool không còn tương thích với web nguồn]\n" + IDTruyen + " " + Nguon;
            }

            //Tìm tới vị trí chương hiện tại và bắt đầu từ chương kế sau
            string TonTai = "null";
            while (i >= 0 + Tre) //Giới hạn danh sách với độ trễ (giới hạn đầu tùy kiểu danh sách)
            {
                TonTai = "Không";

                var TenChuong = Regex.Matches(DanhSachChuong2[i].ToString(), @">(.*?)<", RegexOptions.Singleline);
                Ten = TenChuong[0].ToString().Replace(@">", "");
                Ten = Ten.Replace(@"<", "");
                Ten = Ten.Replace(TenTruyen, "").Replace("–", "").Trim();

                i--;
                
                //Loại bỏ kí tự hạn chế trong tên
                foreach (string KiTu in ListHanChe)
                {
                    try
                    {
                        ChuongHienTai = ChuongHienTai.Replace(KiTu, "");
                        Ten = Ten.Replace(KiTu, "");
                    }
                    catch { }
                }

                if (Ten.ToLower().Replace("  "," ").Trim() == ChuongHienTai.ToLower().Replace("  ", " ").Trim())
                {
                    TonTai = "Có";
                    break;
                }
            }

            if (TonTai == "Không")
            {
                ChuongHienTai = "[Lỗi tên chương 2 bên không tương đồng]\n" + IDTruyen + " " + ChuongHienTai + "\n" + Nguon + " " + Ten;
            }

            //Tải anh có trong chương
            while (i >= 0 + Tre) //Giới hạn danh sách với độ trễ
            {
                //Lọc lấy link chương
                var LinkChuong = Regex.Matches(DanhSachChuong2[i].ToString(), @"href=""(.*?)""", RegexOptions.Singleline);
                Link = LinkChuong[0].ToString().Replace(@"href=""", "");
                Link = Link.Replace(@"""", "");

                //Lọc lấy tên chương
                var TenChuong = Regex.Matches(DanhSachChuong2[i].ToString(), @">(.*?)<", RegexOptions.Singleline);
                Ten = TenChuong[0].ToString().Replace(@">", "");
                Ten = Ten.Replace(@"<", "");
                Ten = Ten.Replace(TenTruyen, "").Replace("–", "").Trim();

                if (Ten.ToLower().Contains("raw") || Ten.ToLower().Contains("eng") || Ten.ToLower().Contains("english"))
                {
                    ChuongHienTai = "[Chương này có vẻ là raw hoặc eng]\n" + " " + IDTruyen + " " + Nguon + " " + Ten;
                    break;
                }

                //Loại bỏ kí tự hạn chế trong tên
                foreach (string KiTu in ListHanChe)
                {
                    try
                    {
                        ChuongHienTai = ChuongHienTai.Replace(KiTu, "");
                        Ten = Ten.Replace(KiTu, "");
                    }
                    catch { }
                }

                //Lấy danh sách ảnh có trong chương
                chromeDriver.Url = Link;
                chromeDriver.Navigate();

                string htmlDanhSachAnh = chromeDriver.PageSource;

                string DanhSachAnhPartem1 = @"id=""chapter-content""(.*?)</section>";
                var DanhSachAnh1 = Regex.Matches(htmlDanhSachAnh, DanhSachAnhPartem1, RegexOptions.Singleline);

                string DanhSachAnhPartem2 = @"><img class=""chapter-img"" src=""(.*?)""";
                var DanhSachAnh2 = Regex.Matches(DanhSachAnh1[0].ToString(), DanhSachAnhPartem2, RegexOptions.Singleline);

                //Nếu danh sách rỗng tức là xảy ra lỗi không tương thích
                if (DanhSachAnh2.Count == 0)
                {
                    Ten = "[Lỗi tool không còn tương thích với web nguồn]\n" + IDTruyen + " " + Nguon;
                    ChuongHienTai = Ten;

                    break;
                }

                DemAnh = 0;

                //Tải các ảnh có trong chương
                foreach (var anh in DanhSachAnh2)
                {
                    LinkAnh = anh.ToString().Replace(@"><img class=""chapter-img"" src=""", "");
                    LinkAnh = LinkAnh.Replace(@"""", "");

                    try
                    {
                        if (Directory.Exists(DuongDan + "\\" + Ten) == false)
                        { Directory.CreateDirectory(DuongDan + "\\" + Ten); }

                        WebClient client = new WebClient();
                        client.Headers.Set("Referer", Link);

                        if (DemAnh < 10)
                        { client.DownloadFile(LinkAnh, DuongDan + "\\" + Ten + "\\0" + DemAnh + DinhDang(LinkAnh)); }
                        else
                        { client.DownloadFile(LinkAnh, DuongDan + "\\" + Ten + "\\" + DemAnh + DinhDang(LinkAnh)); }
                    }
                    catch
                    {
                        try
                        {
                            if (Directory.Exists(DuongDan + "\\" + Ten) == false)
                            { Directory.CreateDirectory(DuongDan + "\\" + Ten); }

                            WebClient client = new WebClient();
                            //client.Headers.Set("Referer", Link);

                            if (DemAnh < 10)
                            { client.DownloadFile(LinkAnh, DuongDan + "\\" + Ten + "\\0" + DemAnh + DinhDang(LinkAnh)); }
                            else
                            { client.DownloadFile(LinkAnh, DuongDan + "\\" + Ten + "\\" + DemAnh + DinhDang(LinkAnh)); }
                        }
                        catch
                        {
                            if (Directory.Exists(DuongDan + "\\" + Ten) == true)
                            { Directory.Delete(DuongDan + "\\" + Ten, true); }

                            i = -1; //Tải thất bại thì gán i âm để ngừng vòng lặp ngoài
                            Ten = "[Lỗi tải] " + IDTruyen + " " + Nguon + " " + Ten;

                            break;
                        }

                    }

                    DemAnh++;
                }

                ChuongHienTai = Ten; //Cập nhật lại "chương hiện tại"

                i--;
            }

            return ChuongHienTai;
        }

        public static string NgonPhongComic (string IDTruyen, string Nguon, string ChuongHienTai, int Tre, string DuongDan)
        {
            TaoDanhSachDinhDang();

            chromeDriver.Url = Nguon;
            chromeDriver.Navigate();

            string htmlDanhSachChuong = chromeDriver.PageSource;

            //Lấy danh sách chương có trong truyện
            string DanhSachChuongPartem1 = @"<tbody>(.*?)</tbody>";
            var DanhSachChuong1 = Regex.Matches(htmlDanhSachChuong, DanhSachChuongPartem1, RegexOptions.Singleline);

            string DanhSachChuongPartem2 = @"<a(.*?)</a>";
            var DanhSachChuong2 = Regex.Matches(DanhSachChuong1[0].ToString(), DanhSachChuongPartem2, RegexOptions.Singleline);

            string Ten = "";
            string Link, LinkAnh = "";
            int DemAnh = 0;

            //int i = DanhSachChuong2.Count -  1 - Tre; //Giới hạn danh sách với độ trễ (giới hạn cuối tùy kiểu danh sách)
            int i = DanhSachChuong2.Count - 1;

            //Nếu danh sách rỗng tức là xảy ra lỗi không tương thích
            if (i == 0)
            {
                i = -1;
                Ten = "[Lỗi tool không còn tương thích với web nguồn]\n" + IDTruyen + " " + Nguon;
            }

            //Tìm tới vị trí chương hiện tại và bắt đầu từ chương kế sau
            string TonTai = "null";
            while (i >= 0 + Tre) //Giới hạn danh sách với độ trễ (giới hạn đầu tùy kiểu danh sách)
            {
                TonTai = "Không";

                var TenChuong = Regex.Matches(DanhSachChuong2[i].ToString(), @">(.*?)<", RegexOptions.Singleline);
                Ten = TenChuong[0].ToString().Replace(@">", "");
                Ten = Ten.Replace(@"<", "");

                i--;

                //Loại bỏ kí tự hạn chế trong tên
                foreach (string KiTu in ListHanChe)
                {
                    try
                    {
                        ChuongHienTai = ChuongHienTai.Replace(KiTu, "");
                        Ten = Ten.Replace(KiTu, "");
                    }
                    catch { }
                }

                if (Ten.ToLower().Replace("  "," ").Trim() == ChuongHienTai.ToLower().Replace("  ", " ").Trim())
                {
                    TonTai = "Có";
                    break;
                }
            }

            if (TonTai == "Không")
            {
                ChuongHienTai = "[Lỗi tên chương 2 bên không tương đồng]\n" + IDTruyen + " " + ChuongHienTai + "\n" + Nguon + " " + Ten;
            }

            //Tải anh có trong chương
            while (i >= 0 + Tre) //Giới hạn danh sách với độ trễ
            {
                //Lọc lấy link chương
                var LinkChuong = Regex.Matches(DanhSachChuong2[i].ToString(), @"href=""(.*?)""", RegexOptions.Singleline);
                Link = LinkChuong[0].ToString().Replace(@"href=""", "");
                Link = Link.Replace(@"""", "");

                //Lọc lấy tên chương
                var TenChuong = Regex.Matches(DanhSachChuong2[i].ToString(), @">(.*?)<", RegexOptions.Singleline);
                Ten = TenChuong[0].ToString().Replace(@">", "");
                Ten = Ten.Replace(@"<", "");

                //Loại bỏ kí tự hạn chế trong tên
                foreach (string KiTu in ListHanChe)
                {
                    try
                    {
                        ChuongHienTai = ChuongHienTai.Replace(KiTu, "");
                        Ten = Ten.Replace(KiTu, "");
                    }
                    catch { }
                }

                //Lấy danh sách ảnh có trong chương
                chromeDriver.Url = Link;
                chromeDriver.Navigate();

                string htmlDanhSachAnh = chromeDriver.PageSource;

                string DanhSachAnhPartem1 = @"id=""chapter-content""(.*?)</section>";
                var DanhSachAnh1 = Regex.Matches(htmlDanhSachAnh, DanhSachAnhPartem1, RegexOptions.Singleline);

                string DanhSachAnhPartem2 = @"><img class=""chapter-img"" src=""(.*?)""";
                var DanhSachAnh2 = Regex.Matches(DanhSachAnh1[0].ToString(), DanhSachAnhPartem2, RegexOptions.Singleline);

                //Nếu danh sách rỗng tức là xảy ra lỗi không tương thích
                if (DanhSachAnh2.Count == 0)
                {
                    Ten = "[Lỗi tool không còn tương thích với web nguồn]\n" + IDTruyen + " " + Nguon;
                    ChuongHienTai = Ten;

                    break;
                }

                DemAnh = 0;

                //Tải các ảnh có trong chương
                foreach (var anh in DanhSachAnh2)
                {
                    LinkAnh = anh.ToString().Replace(@"><img class=""chapter-img"" src=""", "");
                    LinkAnh = LinkAnh.Replace(@"""", "");

                    try
                    {
                        if (Directory.Exists(DuongDan + "\\" + Ten) == false)
                        { Directory.CreateDirectory(DuongDan + "\\" + Ten); }

                        WebClient client = new WebClient();
                        client.Headers.Set("Referer", Link);

                        if (DemAnh < 10)
                        { client.DownloadFile(LinkAnh, DuongDan + "\\" + Ten + "\\0" + DemAnh + DinhDang(LinkAnh)); }
                        else
                        { client.DownloadFile(LinkAnh, DuongDan + "\\" + Ten + "\\" + DemAnh + DinhDang(LinkAnh)); }
                    }
                    catch
                    {
                        try
                        {
                            if (Directory.Exists(DuongDan + "\\" + Ten) == false)
                            { Directory.CreateDirectory(DuongDan + "\\" + Ten); }

                            WebClient client = new WebClient();
                            //client.Headers.Set("Referer", Link);

                            if (DemAnh < 10)
                            { client.DownloadFile(LinkAnh, DuongDan + "\\" + Ten + "\\0" + DemAnh + DinhDang(LinkAnh)); }
                            else
                            { client.DownloadFile(LinkAnh, DuongDan + "\\" + Ten + "\\" + DemAnh + DinhDang(LinkAnh)); }
                        }
                        catch
                        {
                            if (Directory.Exists(DuongDan + "\\" + Ten) == true)
                            { Directory.Delete(DuongDan + "\\" + Ten, true); }

                            i = -1; //Tải thất bại thì gán i âm để ngừng vòng lặp ngoài
                            Ten = "[Lỗi tải] " + IDTruyen + " " + Nguon + " " + Ten;

                            break;
                        }

                    }

                    DemAnh++;
                }

                ChuongHienTai = Ten; //Cập nhật lại "chương hiện tại"

                i--;
            }

            return ChuongHienTai;
        }

        //Coming soon
        public static string TTManga(string IDTruyen, string Nguon, string ChuongHienTai, int Tre, string DuongDan)
        {
            TaoDanhSachDinhDang();

            HttpClient httpClient = new HttpClient();
            httpClient.BaseAddress = new Uri(Nguon);

            string htmlDanhSachChuong = httpClient.GetStringAsync("").Result;

            //Lấy tên truyện
            string TenTruyen= Regex.Match(htmlDanhSachChuong, @"itemprop=""name(.*?)>", RegexOptions.Singleline).Value;
            TenTruyen = TenTruyen.Replace(@"itemprop=""name"" content=""", "").Replace(@""">",""); ;

            //Lấy danh sách chương có trong truyện
            string DanhSachChuongPartem1 = @"<tbody>(.*?)</tbody>";
            var DanhSachChuong1 = Regex.Matches(htmlDanhSachChuong, DanhSachChuongPartem1, RegexOptions.Singleline);

            string DanhSachChuongPartem2 = @"<a(.*?)</a>";
            var DanhSachChuong2 = Regex.Matches(DanhSachChuong1[0].ToString(), DanhSachChuongPartem2, RegexOptions.Singleline);

            string Ten = "";
            string Link, LinkAnh = "";
            int DemAnh = 0;

            //int i = DanhSachChuong2.Count -  1 - Tre; //Giới hạn danh sách với độ trễ (giới hạn cuối tùy kiểu danh sách)
            int i = DanhSachChuong2.Count - 1;

            //Nếu danh sách rỗng tức là xảy ra lỗi không tương thích
            if (i == 0)
            {
                i = -1;
                Ten = "[Lỗi tool không còn tương thích với web nguồn]\n" + IDTruyen + " " + Nguon;
            }

            //Tìm tới vị trí chương hiện tại và bắt đầu từ chương kế sau
            string TonTai = "null";
            while (i >= 0 + Tre) //Giới hạn danh sách với độ trễ (giới hạn đầu tùy kiểu danh sách)
            {
                TonTai = "Không";

                var TenChuong = Regex.Matches(DanhSachChuong2[i].ToString(), @">(.*?)<", RegexOptions.Singleline);
                Ten = TenChuong[0].ToString().Replace(">", "");
                Ten = Ten.Replace("<", "").Replace(TenTruyen,"").Trim();

                i--;

                //Loại bỏ kí tự hạn chế trong tên
                foreach (string KiTu in ListHanChe)
                {
                    try
                    {
                        ChuongHienTai = ChuongHienTai.Replace(KiTu, "");
                        Ten = Ten.Replace(KiTu, "");
                    }
                    catch { }
                }

                if (Ten.ToLower().Replace("  "," ").Trim() == ChuongHienTai.ToLower().Replace("  ", " ").Trim())
                {
                    TonTai = "Có";
                    break;
                }
            }

            if (TonTai == "Không")
            {
                ChuongHienTai = "[Lỗi tên chương 2 bên không tương đồng]\n" + IDTruyen + " " + ChuongHienTai + "\n" + Nguon + " " + Ten;
            }

            //Tải anh có trong chương
            while (i >= 0 + Tre) //Giới hạn danh sách với độ trễ
            {
                //Lọc lấy link chương
                var LinkChuong = Regex.Matches(DanhSachChuong2[i].ToString(), @"href=""(.*?)""", RegexOptions.Singleline);
                Link = LinkChuong[0].ToString().Replace(@"href=""", "");
                Link = Link.Replace(@"""", "");

                //Lọc lấy tên chương
                var TenChuong = Regex.Matches(DanhSachChuong2[i].ToString(), @">(.*?)<", RegexOptions.Singleline);
                Ten = TenChuong[0].ToString().Replace(">", "");
                Ten = Ten.Replace("<", "").Replace(TenTruyen, "").Trim();

                //Loại bỏ kí tự hạn chế trong tên
                foreach (string KiTu in ListHanChe)
                {
                    try
                    {
                        ChuongHienTai = ChuongHienTai.Replace(KiTu, "");
                        Ten = Ten.Replace(KiTu, "");
                    }
                    catch { }
                }


                //Ngừng
                //Lấy danh sách ảnh có trong chương
                HttpClient httpClient2 = new HttpClient();
                httpClient2.BaseAddress = new Uri(Link);

                string htmlDanhSachAnh = httpClient2.GetStringAsync("").Result;

                string DanhSachAnhPartem1 = @"<div class=""manga-container"">(.*?)<div  align=""center"" class=""quang-cao-cuoi-trang"">";
                var DanhSachAnh1 = Regex.Matches(htmlDanhSachAnh, DanhSachAnhPartem1, RegexOptions.Singleline);

                string DanhSachAnhPartem2 = @"<img src=""(.*?)""";
                var DanhSachAnh2 = Regex.Matches(DanhSachAnh1[0].ToString(), DanhSachAnhPartem2, RegexOptions.Singleline);

                //Nếu danh sách rỗng tức là xảy ra lỗi không tương thích
                if (DanhSachAnh2.Count == 0)
                {
                    Ten = "[Lỗi tool không còn tương thích với web nguồn]\n" + IDTruyen + " " + Nguon;
                    ChuongHienTai = Ten;

                    break;
                }

                DemAnh = 0;

                //Tải các ảnh có trong chương
                foreach (var anh in DanhSachAnh2)
                {
                    LinkAnh = anh.ToString().Replace(@"<img src=""", "");
                    LinkAnh = LinkAnh.Replace(@"""", "");

                    try
                    {
                        if (Directory.Exists(DuongDan + "\\" + Ten) == false)
                        { Directory.CreateDirectory(DuongDan + "\\" + Ten); }

                        WebClient client = new WebClient();
                        client.Headers.Set("Referer", Link);

                        if (DemAnh < 10)
                        { client.DownloadFile(LinkAnh, DuongDan + "\\" + Ten + "\\0" + DemAnh + DinhDang(LinkAnh)); }
                        else
                        { client.DownloadFile(LinkAnh, DuongDan + "\\" + Ten + "\\" + DemAnh + DinhDang(LinkAnh)); }
                    }
                    catch
                    {
                        try
                        {
                            if (Directory.Exists(DuongDan + "\\" + Ten) == false)
                            { Directory.CreateDirectory(DuongDan + "\\" + Ten); }

                            WebClient client = new WebClient();
                            //client.Headers.Set("Referer", Link);

                            if (DemAnh < 10)
                            { client.DownloadFile(LinkAnh, DuongDan + "\\" + Ten + "\\0" + DemAnh + DinhDang(LinkAnh)); }
                            else
                            { client.DownloadFile(LinkAnh, DuongDan + "\\" + Ten + "\\" + DemAnh + DinhDang(LinkAnh)); }
                        }
                        catch
                        {
                            if (Directory.Exists(DuongDan + "\\" + Ten) == true)
                            { Directory.Delete(DuongDan + "\\" + Ten, true); }

                            i = -1; //Tải thất bại thì gán i âm để ngừng vòng lặp ngoài
                            Ten = "[Lỗi tải] " + IDTruyen + " " + Nguon + " " + Ten;

                            break;
                        }

                    }

                    DemAnh++;
                }

                ChuongHienTai = Ten; //Cập nhật lại "chương hiện tại"

                i--;
            }

            return ChuongHienTai;
        }

        #endregion
    }
}