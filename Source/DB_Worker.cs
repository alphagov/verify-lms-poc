using System;
using System.Data;
using System.Data.Odbc;
using System.Collections.Generic;

namespace local_matching.DBODBC
{
    public class DB_Worker
    {
        private string driver;
        private string server;
        private string database;
        private string user;
        private string password;

        public DB_Worker(string dr, string se, string da, string us, string pa)
        {
            driver = dr;
            server = se;
            database = da;
            user = us;
            password = pa;
        }

        public int Go(string queryString)
        {
            OdbcCommand command = new OdbcCommand(queryString);

            string connectionString="Driver={" + driver + "};"+"Server=" + server + ";"+database + ";Uid=" + user + ";Pwd=" + password + ";";

            int cnt=0;

            using (OdbcConnection connection = new OdbcConnection(connectionString))
            {
                command.Connection = connection;
                connection.Open();
                cnt=command.ExecuteNonQuery();

                // The connection is automatically closed at 
                // the end of the Using block.
            }

            return cnt;
        }

        public string[] Read(string queryString)
        {
            OdbcCommand command = new OdbcCommand(queryString);

            string connectionString = "Driver={" + driver + "};" + "Server=" + server + ";" + database + ";Uid=" + user + ";Pwd=" + password + ";";
            //  

            List<string> mycol = new List<string>();

            using (OdbcConnection connection = new OdbcConnection(connectionString))
            {
                command.Connection = connection;
                connection.Open();

                OdbcDataReader myReader = command.ExecuteReader();


                // This needs to transfer to a list of strings
                try         
                {
                    while (myReader.Read())
                    {
                        string line="";
                        for(int i=0;i<myReader.FieldCount;i++)
                        {
                            if (myReader[i] != null)
                            {
                                line += "|" + myReader.GetValue(i).ToString();
                            }                            
                            else
                            {
                                line += "|";
                            }
                        }
                        line+="|";
#if DEBUG
                        Console.WriteLine(line);
#endif
                        mycol.Add(line);
                    }
                }
                finally
                {
                    myReader.Close();
                    connection.Close();
                }
                // The connection is automatically closed at 
                // the end of the Using block.
            }

            return mycol.ToArray();
        }


    }
}

//testDB.Go("INSERT INTO AddressLines (IDLINK,Line) Values (1,'Northwind Traders')");
//testDB.Go("Microsoft Access Driver (*.mdb, *.accdb)");
//testDB.Go("MariaDB ODBC 3.0 Driver");
