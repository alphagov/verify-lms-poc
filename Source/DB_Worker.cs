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
             int cnt = 0;

            try
            {
                OdbcCommand command = new OdbcCommand(queryString);

                string connectionString="Driver={" + driver + "};"+"Server=" + server + ";"+database + ";Uid=" + user + ";Pwd=" + password + ";";
                
                using (OdbcConnection connection = new OdbcConnection(connectionString))
                {
                    command.Connection = connection;
                    connection.Open();
                    cnt=command.ExecuteNonQuery();

                    // The connection is automatically closed at 
                    // the end of the Using block.
                }
            }
            catch
            {
                // DB Query failed, this will simply return 0
            }

            return cnt;
        }

        public string[] Read(string queryString)
        {
            List<string> mycol = new List<string>();

            try
            {
                OdbcCommand command = new OdbcCommand(queryString);

                string connectionString = "Driver={" + driver + "};" + "Server=" + server + ";" + database + ";Uid=" + user + ";Pwd=" + password + ";";

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
            }
            catch
            {
                // The query failed for some reason, so just send back empty array
            }
            return mycol.ToArray();
        }


    }
}
