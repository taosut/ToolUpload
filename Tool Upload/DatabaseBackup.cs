using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tool_Upload
{
    public class DatabaseBackup
    {
        public SQLiteConnection MyConnection;

        public DatabaseBackup()
        {
            MyConnection = new SQLiteConnection("Data source=DatabaseBackup.db");
        }

        public void OpenConnection()
        {
            MyConnection.Open();
        }

        public void CloseConnection()
        {
            MyConnection.Close();          
        }

        ~DatabaseBackup()
        { }
    }
}
