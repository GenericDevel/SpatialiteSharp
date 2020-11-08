using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.IO;
using System.Text;

namespace SpatialiteDemo
{
    class Program
    {
        static SQLiteHelper sh = SQLiteHelper.Instance;

        public static void Main(string[] args)
        {
            DisplayMenu();
        }

        private static void DisplayMenu()
        {
            Console.Clear();
            Console.WriteLine("\n***************************************");
            Console.WriteLine("\t 1. Create Database");
            Console.WriteLine("\t 2. Init spatial metadata");
            Console.WriteLine("\t 3. Create spatial table");
            Console.WriteLine("\t 4. Show Table Info");
            Console.WriteLine("\t 5. Query");
            Console.WriteLine("\t 6. Create polylines");
            Console.WriteLine("\t 7. test intersect");
            Console.WriteLine("\t 0.Exit");
            Console.WriteLine("***************************************");
            Console.Write(">");

            string s = Console.ReadLine();
            switch (s)
            {
                case "1":
                    CreateDatabase();
                    break;
                case "2":
                    InitSpatialMeta();
                    break;
                case "3":
                    CreateSpatialTable();
                    break;
                case "4":
                    ShowTableInfo();
                    break;
                case "5":
                    Query();
                    break;
                case "6":
                    CreatePolylines();
                    break;
                case "7":
                    TestIntersect();
                    break;
                case "0":
                    sh.Close();
                    System.Environment.Exit(0);
                    break;
            }
        }

        private static void CreateDatabase()
        {
            Console.Write("\n 请输入数据库名称：");
            string database = Console.ReadLine();

            try
            {
                if (!database.Contains(":\\"))
                    database = Environment.CurrentDirectory + "\\" + database;
                database = database.Replace("\\\\", "\\");

                bool isNew = false;
                if(!File.Exists(database))
                {
                    isNew = true;
                }
                sh.Open(database);
                //加载空间数据模块
                sh.LoadSpatialMod();

                if(isNew)
                    Console.WriteLine("数据库创建成功，按任意键返回.");
                else
                    Console.WriteLine("数据库打开成功，按任意键返回.");

                Console.ReadKey();

                DisplayMenu();
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("#错误：" + ex.Message);
                Console.ResetColor();

                Console.WriteLine("按任意键返回.");
                Console.ReadKey();
                DisplayMenu();
            }
        }

        /// <summary>
        /// 在创建空间数据时，初始化时必须要执行此功能
        /// </summary>
        private static void InitSpatialMeta()
        {
            try
            {
                sh.InitSpatialMeta();
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("#错误：" + ex.Message);
                Console.ResetColor();
            }
            finally { 
                Console.WriteLine("按任意键返回.");
                Console.ReadKey();
                DisplayMenu();
            }
        }

        private static void CreateSpatialTable()
        {
            try
            {
                #region 创建点层
                SQLiteTable table = new SQLiteTable();
                table.Name = "PointT";
                SQLiteColumn col = new SQLiteColumn();
                col.AutoIncrement = true;
                col.DataType = FieldType.Integer;
                col.Name = "PK_UID";
                col.NotNull = true;
                table.Columns.Add(col);

                col = new SQLiteColumn();
                col.DataType = FieldType.Text;
                col.Name = "name";
                table.Columns.Add(col);
                sh.CreateTable(table);
                string sql = "CREATE TABLE PointT (PK_UID INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,name TEXT)";
                //sh.Execute(sql);

                /*  添加图形列 AddGeometryColumn('你的表名', '你的几何列名',4326, 'POINT','XY')  XY为二维数据*/
                sql = "SELECT AddGeometryColumn('PointT', 'GEOMETRY',2346, 'POINT','XY')"; //2346 为EPSG码
                sh.Execute(sql);

                /* 添加空间索引 */
                sql = "SELECT CreateSpatialIndex('PointT', 'GEOMETRY')";
                sh.Execute(sql);

                /*  通过OGC标准的WTK 文件描述格式插入一个点记录*/
                sql = "INSERT INTO PointT(PK_UID, name, GEOMETRY) "
                    + "VALUES (1, 'first point',GeomFromText('POINT(636000 5209340)',2346))";
                sh.Execute(sql);
                /*  通过OGC标准的WTK 文件描述格式插入一个点记录*/
                sql = "INSERT INTO PointT(GEOMETRY, name, PK_UID) "
                    + "VALUES (GeomFromText('POINT(636100 5209340)',2346),'eleventh point', 2)";
                sh.Execute(sql);

                #endregion


                #region 创建线层

                sql = "CREATE TABLE PolylineT(PK_UID INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,name TEXT NOT NULL)";
                sh.Execute(sql);

                //bRet = ExecuteSql(sql, conn);
                /*  添加图形列 */
                sql = "SELECT AddGeometryColumn('PolylineT', 'GEOMETRY',2346, 'LINESTRING', 2)"; //2346 为EPSG码
                sh.Execute(sql);

                /* 添加空间索引 */
                sql = "SELECT CreateSpatialIndex('PolylineT', 'GEOMETRY')";
                sh.Execute(sql);

                /*  通过OGC标准的WTK 文件描述格式插入一个点记录*/
                sql = "INSERT INTO PolylineT(PK_UID, name, GEOMETRY) "
                    + "VALUES (1, 'line1', GeomFromText('LINESTRING(636000 5209340,633950 5212200,634400 5207800)',2346))";
                sh.Execute(sql);

                /*  通过OGC标准的WTK 文件描述格式插入一个点记录*/

                //sql = "INSERT INTO Xian(GEOMETRY, meas_value, name, PK_UID) VALUES (GeomFromText('POINT(636100 5209340)',2346),11.123456789, 'eleventh point', 2)";
                //cmd.CommandText = sql;
                //cmd.ExecuteNonQuery();

                #endregion

                #region 创建面层

                sql = "CREATE TABLE PolygonT (PK_UID INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,name TEXT NOT NULL)";
                sh.Execute(sql);

                //bRet = ExecuteSql(sql, conn);
                /*  添加图形列 */
                sql = "SELECT AddGeometryColumn('PolygonT', 'GEOMETRY',2346, 'POLYGON', 2)"; //2346 为EPSG码
                sh.Execute(sql);

                /* 添加空间索引 */
                sql = "SELECT CreateSpatialIndex('PolygonT', 'GEOMETRY')";
                sh.Execute(sql);

                /*  通过OGC标准的WTK 文件描述格式插入一个点记录*/
                sql = "INSERT INTO PolygonT(PK_UID, name, GEOMETRY) "
                    + "VALUES (1, 'Poly1',GeomFromText('POLYGON((636000 5209340,633950 5212200,634400 5207800,632409 5209760,636000 5209340))',2346))";
                sh.Execute(sql);

                /*  通过OGC标准的WTK 文件描述格式插入一个点记录*/

                //sql = "INSERT INTO Xian(GEOMETRY, meas_value, name, PK_UID) VALUES (GeomFromText('POINT(636100 5209340)',2346),11.123456789, 'eleventh point', 2)";
                //cmd.CommandText = sql;
                //cmd.ExecuteNonQuery();

                Console.WriteLine("按任意键返回.");
                Console.ReadKey();
                DisplayMenu();

                #endregion
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("#错误：" + ex.Message);
                Console.ResetColor();

                Console.WriteLine("按任意键返回.");
                Console.ReadKey();
                DisplayMenu();
            }
        }

        private static void ShowTableInfo()
        {
            try
            {
                DataTable dt = sh.GetTableList();
                Console.WriteLine("\n 数据表：--------------------");
                Console.WriteLine("序号\t数据表名称");
                int i = 1;

                foreach (DataRow row in dt.Rows)
                {
                    Console.WriteLine("{0}\t{1}", i.ToString(), row[0].ToString());
                }

                Console.WriteLine("按任意键返回");
                Console.ReadKey();
                DisplayMenu();
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("#错误：" + ex.Message);
                Console.ResetColor();

                Console.WriteLine("按任意键返回.");
                Console.ReadKey();
                DisplayMenu();
            }
        }


        private static void Query()
        {
            try
            {
                Console.Write("输入要执行的SQL语句：");
                string sql = Console.ReadLine();
                DataTable dt = sh.Select(sql);

                Console.WriteLine("----------查询结果---------------");
                foreach (DataColumn col in dt.Columns)
                {
                    Console.Write(col.ColumnName+"\t");
                }
                Console.Write("\n");

                foreach (DataRow row in dt.Rows)
                {
                    string data = "";
                    for (int i = 0; i < dt.Columns.Count; i++)
                    {
                        data += row[i].ToString() + "\t";
                    }
                    Console.WriteLine(data);                    
                }
                Console.WriteLine("---------------------------------------");
                
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("#错误：" + ex.Message);
                Console.ResetColor();
            }
            finally
            {
                Console.WriteLine("按任意键返回.");
                Console.ReadKey();
                DisplayMenu();
            }

        }

        /// <summary>
        /// 随机生成多义线
        /// </summary>
        private static void CreatePolylines()
        {
            Console.Write("\n 请输入要生成的线数目：");
            string s = Console.ReadLine();
            int n = 0;
            if (!Int32.TryParse(s, out n))
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("提示：输入的数字无效，请输入有效的正整数! ");
                Console.ResetColor();

                Console.ReadKey();
                Console.WriteLine("按任意键返回.");
                DisplayMenu();

            }

            try
            {
                //清除数据
                sh.Execute("delete from PolylineT");

                /*  通过OGC标准的WTK 文件描述格式插入一个点记录*/
                string sql;

                int l = n / 1000;
                for (int j = 0; j <= l; j++)
                {
                    sh.BeginTransaction();
                    int t = (n < (j + 1) * 1000 ? n : (j + 1) * 1000);
                    for (int i = j*1000; i < t; i++)
                    {
                        sql = String.Format("INSERT INTO PolylineT(PK_UID, name, GEOMETRY) "
                            + "VALUES ({0}, 'line{1}', GeomFromText('LINESTRING({2})',2346))"
                                , i, i, CreatePolyline());
                        sh.Insert(sql);
                        //sh.Execute(sql);
                        OverWrite("当前进度：" + i * 100 / n + "%");
                    }
                    sh.Commit();
                }
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("#错误：" + ex.Message);
                Console.ResetColor();
            }
            finally
            {
                Console.WriteLine("按任意键返回.");
                Console.ReadKey();
                DisplayMenu();
            }

        }

        /// <summary>
        /// 随机生成一个多义线
        /// </summary>
        private static string CreatePolyline()
        {
            Random rand = new Random();
            int n = rand.Next(3, 20);

            string str = "";
            for (int i = 0; i < n; i++)
            {
                str += (string.IsNullOrEmpty(str) ? "" : ",") + rand.Next(636000, 640000) + " " + rand.Next(520000, 521000);
            }

            return str;
        }

        private static void TestIntersect()
        {
            //select asText(geometry) from point where
            //ST_Intersects(point.geometry, GeomFromText('POLYGON((322.372427 113.231539, 230.763178 -34.295044, 49.924143 -20.018278, -141.622469 0.20714, -208.247377 109.662348, 83.236597 259.568392, 86.805788 398.766859, 216.486414 426.130659, 310.475122 334.52141, 322.372427 113.231539))')) = 1

            try
            {
                string str = CreatePolyline();
                string sql = string.Format("select name from PolylineT where "
                    + "ST_Intersects(PolylineT.geometry, GeomFromText('LINESTRING({0})',2346))==1", str);
                DateTime time1 = DateTime.Now;
                DataTable dt = sh.Select(sql);
                DateTime time2 = DateTime.Now;

                TimeSpan ts1 = new TimeSpan(time1.Ticks);
                TimeSpan ts2 = new TimeSpan(time2.Ticks);
                TimeSpan ts = ts1.Subtract(ts2).Duration();
                //显示时间  
                string span = ts.Days.ToString() + "天"
                        + ts.Hours.ToString() + "小时"
                        + ts.Minutes.ToString() + "分钟"
                        + ts.Seconds.ToString() + "秒"
                        + ts.Milliseconds.ToString() + "毫秒";

                Console.WriteLine("用时：" + span);
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("#错误：" + ex.Message);
                Console.ResetColor();
            }
            finally
            {
                Console.WriteLine("按任意键返回.");
                Console.ReadKey();
                DisplayMenu();
            }

        }

        public static void Test()
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

      

        //定点清除指定的行
        public static void OverWrite(string msg,int invertedIndex = 0)
        {
            int currentLineCursor = Console.CursorTop;
            int top = Console.CursorTop - invertedIndex; top = top < 0 ? 0 : top;
            Console.SetCursorPosition(0, top);
            Console.Write(new string(' ', Console.WindowWidth - 1));
            Console.SetCursorPosition(0, currentLineCursor);
            Console.Write(msg);
        }

    }
}
