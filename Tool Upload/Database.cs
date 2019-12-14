using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Tool_Upload
{
    public class Database
    {
        public SQLiteConnection MyConnection;

        public Database()
        {
            MyConnection = new SQLiteConnection("Data source=Database.db");

            if (!File.Exists("./Database.db"))
            {
                SQLiteConnection.CreateFile("Database.db");

                MyConnection.Open();

                string query = "CREATE TABLE Truyen (IDTruyen TEXT PRIMARY KEY,  Nguon TEXT, Tre int, ChuongHienTai TEXT, TrangThai TEXT, LenDauTrang TEXT, NgayCapNhat TEXT);";
                SQLiteCommand MyCommand = new SQLiteCommand(query, MyConnection);
                int result1 = MyCommand.ExecuteNonQuery();
            

                //string query3 = "INSERT INTO Truyen (IDTruyen, Nguon, Tre, ChuongHienTai, TrangThai, NgayCapNhat) VALUES ('1670', 'https://truyenqq.com/truyen-tranh/the-promised-neverland-2547.html', 0, '155', 'Đang tiến hành', '')";
                //SQLiteCommand MyCommand3 = new SQLiteCommand(query3, MyConnection);
                //int result3 = MyCommand3.ExecuteNonQuery();

                MyConnection.Close();

                //MessageBox.Show("Chào mừng tới với ứng dụng Comic Viewer được viết bởi NBT", "Xin chào");
            }

        }

        public void OpenConnection()
        {
            MyConnection.Open();
        }

        public void CloseConnection()
        {
            MyConnection.Close();
        }
    }
}
