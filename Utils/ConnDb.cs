using System;
using System.Collections.Generic;
using System.Data.Odbc;
using System.Linq;
using System.Text;
using System.Data.SqlClient;

namespace Utils
{
    public static class ConnDb
    {
        public static SqlConnection GetDBConnection(string host, int port, string sid, string user, string password)
        {

            Console.WriteLine("Getting Connection ...");

            //string connString = "Data Source=(DESCRIPTION=(ADDRESS_LIST=(ADDRESS=(PROTOCOL=TCP)(HOST=" + host +
            //    ")(PORT=" + port + ")))(CONNECT_DATA=(SERVER=DEDICATED)(SERVICE_NAME=" + sid + ")));User Id=" + user +
            //    ";Password=" + password + ";";
            string connString = @"Data Source=regal3\sqlexpress;Initial Catalog=CheckPostingDB;User ID=sa; Password=mymicros";

            SqlConnection conn = new SqlConnection();
            conn.ConnectionString = connString;

            return conn;
        }
        public static SqlConnection GetDBConnection(Configuration configuration)
        {
            if (configuration != null)
                return GetDBConnection(configuration.Host, configuration.Port, configuration.Sid,
                    configuration.User, configuration.Password);
            else
                return null;

        }
    }
}
