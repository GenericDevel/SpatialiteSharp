using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Text;

namespace SpatialiteDemo
{
    class Program
    {

       public static void Main(string[] args)
        {
            string mod_spatialite_folderPath = @".\";

            //using relative path, cannot use absolute path, dll load will fail
            //string mod_spatialite_dllPath = @"mod_spatialite-4.3.0a-win-amd64\mod_spatialite";            
            string path = Environment.GetEnvironmentVariable("Path", EnvironmentVariableTarget.Machine) + ";" + mod_spatialite_folderPath;
            Environment.SetEnvironmentVariable("Path", path, EnvironmentVariableTarget.Process);

            string sqlitePath = "_BCC_replication.sqlite";

            //select 'geometry::STGeomFromWKB(0x' || hex(ST_AsBinary(geometry)) || ', 28356) ' as geometry2  from qpp_airport_ols;            
            string sqliteConnectionString = getSqliteV3ConnectionString(sqlitePath);

            using (var sqliteConnection = new SQLiteConnection(sqliteConnectionString))
            {
                sqliteConnection.Open();
                Console.WriteLine("Load-ing mod_spatialite");
                //string mod_spatialite_dllPath = mod_spatialite_folderPath + @"\mod_spatialite";
                sqliteConnection.LoadExtension("mod_spatialite.dll");
                Console.WriteLine("Load-ed mod_spatialite");
                //sqliteConnection.Execute("update qpp_airport_ols set lga_code = 111122 where ogc_fid = 2;");
                string sql = "SELECT ST_MINX(geometry), ST_MINY(geometry), ST_MAXX(geometry), ST_MAXY(geometry) FROM qpp_airport_ols ";

                using (SQLiteCommand command = new SQLiteCommand(sql, sqliteConnection))
                {
                    using (SQLiteDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            double minX = reader.GetDouble(0);
                            double minY = reader.GetDouble(1);
                            double maxX = reader.GetDouble(2);
                            double maxY = reader.GetDouble(3);
                            Console.WriteLine("MinX:{0},MinY:{1},MaxX:{2},MaxY{3}"
                                , minX.ToString("0.00")
                                , minY.ToString("0.00")
                                , maxX.ToString("0.00")
                                , maxY.ToString("0.00"));
                        }
                    }
                }
            }

            Console.ReadLine();
        }
    

        public static string getSqliteV3ConnectionString(string sqlitePath)
        {
            //Default Timeout is in seconds, useful for when sqlite is locked,
            //exception is returned after 1 second, default is 30 seconds.            
            return "Data source=" + sqlitePath + "; Version = 3; Default Timeout = 3;";
        }

    }
}
