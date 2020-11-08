//////////////////////////////////////////////////
/// 文件：SQLiteHelper.cs
/// 说明：SQLite管理助手
/// 
/// 时间：2020/11/8/
/// 来源：http://sh.codeplex.com
/////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
using System.Globalization;

namespace System.Data.SQLite
{
    /// <summary>
    /// 字段类型
    /// </summary>
    public enum FieldType
    {
        Text,
        DateTime,
        Integer,
        Decimal,
        BLOB
    }

    /// <summary>
    /// SQLite管理助手
    /// </summary>
    public class SQLiteHelper
    {
        #region 实例及构造函数
        static SQLiteHelper instance;

        /// <summary>
        /// 实例
        /// </summary>
        public static SQLiteHelper Instance
        {
            get
            {
                if (instance == null)
                    instance = new SQLiteHelper();

                return instance;
            }
        }


        public SQLiteHelper() { }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="command"></param>
        public SQLiteHelper(SQLiteCommand command)
        {
            cmd = command;
        }

        #endregion

        #region 私有变量及属性
        private SQLiteCommand cmd = null;
        private SQLiteConnection conn;

        /// <summary>
        /// SQLite Command
        /// </summary>
        public SQLiteCommand Command { get => cmd; set => cmd = value; }

        /// <summary>
        /// SQLite Connection
        /// </summary>
        public SQLiteConnection Connection { get => conn; set => conn = value; }
        #endregion

        #region 连接管理
        /// <summary>
        /// 打开连接
        /// </summary>
        /// <param name="databasePath">数据库文件路径</param>
        /// <returns></returns>
        /// <remarks>如果文件不存在则创建数据库</remarks>
        public bool Open(string databasePath)
        {
            try
            {
                string dataSource = string.Format("data source={0};Pooling=true;FailIfMissing=false", databasePath);
                this.conn = new SQLiteConnection(dataSource);
                conn.Open();

                this.cmd = new SQLiteCommand();                  
                cmd.Connection = conn;
                
                return true;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// 加载空间数据模块
        /// </summary>
        /// <param name="modPath">模块文件路径</param>
        /// <remarks>如果模块库文件与可执行文件在同一目录下，可以不带路径名称，如:mod_spatialite.dll</remarks>
        public void LoadSpatialMod(string modPath="mod_spatialite.dll")
        {
            try
            {
                this.conn.LoadExtension(modPath);               
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// 初始化空间参数
        /// </summary>
        /// <remarks>初始创建空间数据库时，必须执行初始化操作</remarks>
        public void InitSpatialMeta()
        {
            try
            {
                //
                string sql = "select InitSpatialMetaData()";
                cmd.CommandText = sql;
                cmd.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// 关闭
        /// </summary>
        public void Close()
        {
            try
            {
                conn.Close();
            }
            catch (Exception ex)
            { 
            }
        }
        #endregion

        #region DB Info

        /// <summary>
        /// 获取数据表状态
        /// </summary>
        /// <returns>DataTable</returns>
        /// <remarks>字段包括：type,name,tbl_name,rootpage,sql</remarks>
        public DataTable GetTableStatus()
        {
            return Select("SELECT * FROM sqlite_master;");
        }

        /// <summary>
        /// 获取数据表列表
        /// </summary>
        /// <returns>DataTable</returns>
        /// <remarks>字段包括：tablename</remarks>
        public DataTable GetTableList()
        {
            DataTable dt = GetTableStatus();
            DataTable dt2 = new DataTable();
            dt2.Columns.Add("tablename");
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                string t = dt.Rows[i]["name"] + "";
                if (t != "sqlite_sequence")
                    dt2.Rows.Add(t);
            }
            return dt2;
        }

        /// <summary>
        /// 获取数据表字段状态
        /// </summary>
        /// <param name="tableName"></param>
        /// <returns>DataTable</returns>
        /// <remarks>字段包括：cid,name,notnull,dflt_value,pk</remarks>
        public DataTable GetColumnStatus(string tableName)
        {
            return Select(string.Format("PRAGMA table_info(`{0}`);", tableName));
        }

        /// <summary>
        /// 显示数据库信息
        /// </summary>
        /// <returns>DataTable</returns>
        /// <remarks>字段包括：seq,name,file</remarks>
        public DataTable ShowDatabase()
        {
            return Select("PRAGMA database_list;");
        }

        #endregion

        #region 数据查询

        /// <summary>
        /// 开始事务
        /// </summary>
        public void BeginTransaction()
        {
            cmd.CommandText = "begin transaction;";
            cmd.ExecuteNonQuery();
        }

        /// <summary>
        /// 提交
        /// </summary>
        public void Commit()
        {
            cmd.CommandText = "commit;";
            cmd.ExecuteNonQuery();
        }

        /// <summary>
        /// 回滚
        /// </summary>
        public void Rollback()
        {
            cmd.CommandText = "rollback";
            cmd.ExecuteNonQuery();
        }

        /// <summary>
        /// 查询SQL
        /// </summary>
        /// <param name="sql"></param>
        /// <returns></returns>
        public DataTable Select(string sql)
        {
            return Select(sql, new List<SQLiteParameter>());
        }

        /// <summary>
        /// 带参数查询
        /// </summary>
        /// <param name="sql">SQL语句</param>
        /// <param name="dicParameters">参数列表</param>
        /// <returns>DataTable</returns>
        /// <remarks>示例：
        /// 
        /// 
        /// </remarks>
        public DataTable Select(string sql, Dictionary<string, object> dicParameters = null)
        {
            List<SQLiteParameter> lst = GetParametersList(dicParameters);
            return Select(sql, lst);
        }

        /// <summary>
        /// 带参数查询
        /// </summary>
        /// <param name="sql"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public DataTable Select(string sql, IEnumerable<SQLiteParameter> parameters = null)
        {
            cmd.CommandText = sql;
            if (parameters != null)
            {
                foreach (var param in parameters)
                {
                    cmd.Parameters.Add(param);
                }
            }
            SQLiteDataAdapter da = new SQLiteDataAdapter(cmd);
            DataTable dt = new DataTable();
            da.Fill(dt);
            return dt;
        }

        /// <summary>
        /// 执行SQL
        /// </summary>
        /// <param name="sql"></param>
        public void Execute(string sql)
        {
            Execute(sql, new List<SQLiteParameter>());
        }

        /// <summary>
        /// 执行带参数SQL
        /// </summary>
        /// <param name="sql"></param>
        /// <param name="dicParameters"></param>
        public void Execute(string sql, Dictionary<string, object> dicParameters = null)
        {
            List<SQLiteParameter> lst = GetParametersList(dicParameters);
            Execute(sql, lst);
        }

        /// <summary>
        /// 执行带参数SQL
        /// </summary>
        /// <param name="sql"></param>
        /// <param name="parameters"></param>
        public void Execute(string sql, IEnumerable<SQLiteParameter> parameters = null)
        {
            cmd.CommandText = sql;
            if (parameters != null)
            {
                foreach (var param in parameters)
                {
                    cmd.Parameters.Add(param);
                }
            }
            cmd.ExecuteNonQuery();
        }

        /// <summary>
        /// 执行SQL
        /// </summary>
        /// <param name="sql"></param>
        /// <returns></returns>
        public object ExecuteScalar(string sql)
        {
            cmd.CommandText = sql;
            return cmd.ExecuteScalar();
        }

        /// <summary>
        /// 执行带参数SQL
        /// </summary>
        /// <param name="sql"></param>
        /// <param name="dicParameters"></param>
        /// <returns></returns>
        public object ExecuteScalar(string sql, Dictionary<string, object> dicParameters = null)
        {
            List<SQLiteParameter> lst = GetParametersList(dicParameters);
            return ExecuteScalar(sql, lst);
        }

        /// <summary>
        /// 执行带参数SQL
        /// </summary>
        /// <param name="sql"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public object ExecuteScalar(string sql, IEnumerable<SQLiteParameter> parameters = null)
        {
            cmd.CommandText = sql;
            if (parameters != null)
            {
                foreach (var parameter in parameters)
                {
                    cmd.Parameters.Add(parameter);
                }
            }
            return cmd.ExecuteScalar();
        }

        public dataType ExecuteScalar<dataType>(string sql, Dictionary<string, object> dicParameters = null)
        {
            List<SQLiteParameter> lst = null;
            if (dicParameters != null)
            {
                lst = new List<SQLiteParameter>();
                foreach (KeyValuePair<string, object> kv in dicParameters)
                {
                    lst.Add(new SQLiteParameter(kv.Key, kv.Value));
                }
            }
            return ExecuteScalar<dataType>(sql, lst);
        }

        public dataType ExecuteScalar<dataType>(string sql, IEnumerable<SQLiteParameter> parameters = null)
        {
            cmd.CommandText = sql;
            if (parameters != null)
            {
                foreach (var parameter in parameters)
                {
                    cmd.Parameters.Add(parameter);
                }
            }
            return (dataType)Convert.ChangeType(cmd.ExecuteScalar(), typeof(dataType));
        }

        public dataType ExecuteScalar<dataType>(string sql)
        {
            cmd.CommandText = sql;
            return (dataType)Convert.ChangeType(cmd.ExecuteScalar(), typeof(dataType));
        }

        private List<SQLiteParameter> GetParametersList(Dictionary<string, object> dicParameters)
        {
            List<SQLiteParameter> lst = new List<SQLiteParameter>();
            if (dicParameters != null)
            {
                foreach (KeyValuePair<string, object> kv in dicParameters)
                {
                    lst.Add(new SQLiteParameter(kv.Key, kv.Value));
                }
            }
            return lst;
        }

        public string Escape(string data)
        {
            data = data.Replace("'", "''");
            data = data.Replace("\\", "\\\\");
            return data;
        }

        /// <summary>
        /// 插入数据
        /// </summary>
        /// <param name="tableName"></param>
        /// <param name="dic"></param>
        public void Insert(string tableName, Dictionary<string, object> dic)
        {
            StringBuilder sbCol = new System.Text.StringBuilder();
            StringBuilder sbVal = new System.Text.StringBuilder();

            foreach (KeyValuePair<string, object> kv in dic)
            {
                if (sbCol.Length == 0)
                {
                    sbCol.Append("insert into ");
                    sbCol.Append(tableName);
                    sbCol.Append("(");
                }
                else
                {
                    sbCol.Append(",");
                }

                sbCol.Append("`");
                sbCol.Append(kv.Key);
                sbCol.Append("`");

                if (sbVal.Length == 0)
                {
                    sbVal.Append(" values(");
                }
                else
                {
                    sbVal.Append(", ");
                }

                sbVal.Append("@v");
                sbVal.Append(kv.Key);
            }

            sbCol.Append(") ");
            sbVal.Append(");");

            cmd.CommandText = sbCol.ToString() + sbVal.ToString();

            foreach (KeyValuePair<string, object> kv in dic)
            {
                cmd.Parameters.AddWithValue("@v" + kv.Key, kv.Value);
            }

            cmd.ExecuteNonQuery();
        }

        /// <summary>
        /// 插入数据
        /// </summary>
        /// <param name="sql">要插入的SQL语句</param>
        /// <param name="dic"></param>
        public void Insert(string sql)
        {          
            cmd.CommandText =sql;
            cmd.ExecuteNonQuery();
        }

        /// <summary>
        /// 修改
        /// </summary>
        /// <param name="tableName"></param>
        /// <param name="dicData"></param>
        /// <param name="colCond"></param>
        /// <param name="varCond"></param>
        public void Update(string tableName, Dictionary<string, object> dicData, string colCond, object varCond)
        {
            Dictionary<string, object> dic = new Dictionary<string, object>();
            dic[colCond] = varCond;
            Update(tableName, dicData, dic);
        }

        /// <summary>
        /// 修改
        /// </summary>
        /// <param name="tableName"></param>
        /// <param name="dicData"></param>
        /// <param name="dicCond"></param>
        public void Update(string tableName, Dictionary<string, object> dicData, Dictionary<string, object> dicCond)
        {
            if (dicData.Count == 0)
                throw new Exception("dicData is empty.");

            StringBuilder sbData = new System.Text.StringBuilder();

            Dictionary<string, object> _dicTypeSource = new Dictionary<string, object>();

            foreach (KeyValuePair<string, object> kv1 in dicData)
            {
                _dicTypeSource[kv1.Key] = null;
            }

            foreach (KeyValuePair<string, object> kv2 in dicCond)
            {
                if (!_dicTypeSource.ContainsKey(kv2.Key))
                    _dicTypeSource[kv2.Key] = null;
            }

            sbData.Append("update `");
            sbData.Append(tableName);
            sbData.Append("` set ");

            bool firstRecord = true;

            foreach (KeyValuePair<string, object> kv in dicData)
            {
                if (firstRecord)
                    firstRecord = false;
                else
                    sbData.Append(",");

                sbData.Append("`");
                sbData.Append(kv.Key);
                sbData.Append("` = ");

                sbData.Append("@v");
                sbData.Append(kv.Key);
            }

            sbData.Append(" where ");

            firstRecord = true;

            foreach (KeyValuePair<string, object> kv in dicCond)
            {
                if (firstRecord)
                    firstRecord = false;
                else
                {
                    sbData.Append(" and ");
                }

                sbData.Append("`");
                sbData.Append(kv.Key);
                sbData.Append("` = ");

                sbData.Append("@c");
                sbData.Append(kv.Key);
            }

            sbData.Append(";");

            cmd.CommandText = sbData.ToString();

            foreach (KeyValuePair<string, object> kv in dicData)
            {
                cmd.Parameters.AddWithValue("@v" + kv.Key, kv.Value);
            }

            foreach (KeyValuePair<string, object> kv in dicCond)
            {
                cmd.Parameters.AddWithValue("@c" + kv.Key, kv.Value);
            }

            cmd.ExecuteNonQuery();
        }

        /// <summary>
        /// 获取最后插入的ID
        /// </summary>
        /// <returns></returns>
        public long LastInsertRowId()
        {
            return ExecuteScalar<long>("select last_insert_rowid();");
        }

        #endregion

        #region 数据表操作

        /// <summary>
        /// 创建数据表
        /// </summary>
        /// <param name="table"></param>
        /// <remarks>示例：
        /// SQLiteTable tb = new SQLiteTable(tableName);
        /// SQLiteColumn field = new SQLiteColumn();
        /// field.ColumnName = colName;
        /// field.ColDataType = ColType.Text;
        /// tb.Columns.Add(field);
        /// </remarks>
        public void CreateTable(SQLiteTable table)
        {
            StringBuilder sb = new Text.StringBuilder();
            sb.Append("create table if not exists `");
            sb.Append(table.Name);
            sb.AppendLine("`(");

            bool firstRecord = true;

            foreach (SQLiteColumn col in table.Columns)
            {
                if (col.Name.Trim().Length == 0)
                {
                    throw new Exception("Column name cannot be blank.");
                }

                if (firstRecord)
                    firstRecord = false;
                else
                    sb.AppendLine(",");

                sb.Append(col.Name);
                sb.Append(" ");

                if (col.AutoIncrement)
                {
                    sb.Append("integer primary key autoincrement");
                    continue;
                }

                switch (col.DataType)
                {
                    case FieldType.Text:
                        sb.Append("text"); break;
                    case FieldType.Integer:
                        sb.Append("integer"); break;
                    case FieldType.Decimal:
                        sb.Append("decimal"); break;
                    case FieldType.DateTime:
                        sb.Append("datetime"); break;
                    case FieldType.BLOB:
                        sb.Append("blob"); break;
                }

                if (col.PrimaryKey)
                    sb.Append(" primary key");
                else if (col.NotNull)
                    sb.Append(" not null");
                else if (col.DefaultValue.Length > 0)
                {
                    sb.Append(" default ");

                    if (col.DefaultValue.Contains(" ") || col.DataType == FieldType.Text || col.DataType == FieldType.DateTime)
                    {
                        sb.Append("'");
                        sb.Append(col.DefaultValue);
                        sb.Append("'");
                    }
                    else
                    {
                        sb.Append(col.DefaultValue);
                    }
                }
            }

            sb.AppendLine(");");

            cmd.CommandText = sb.ToString();
            cmd.ExecuteNonQuery();
        }

        /// <summary>
        /// 修改表名
        /// </summary>
        /// <param name="tableFrom"></param>
        /// <param name="tableTo"></param>
        public void RenameTable(string tableFrom, string tableTo)
        {
            cmd.CommandText = string.Format("alter table `{0}` rename to `{1}`;", tableFrom, tableTo);
            cmd.ExecuteNonQuery();
        }

        /// <summary>
        /// 复制表记录
        /// </summary>
        /// <param name="tableFrom"></param>
        /// <param name="tableTo"></param>
        public void CopyAllData(string tableFrom, string tableTo)
        {
            DataTable dt1 = Select(string.Format("select * from `{0}` where 1 = 2;", tableFrom));
            DataTable dt2 = Select(string.Format("select * from `{0}` where 1 = 2;", tableTo));

            Dictionary<string, bool> dic = new Dictionary<string, bool>();

            foreach (DataColumn dc in dt1.Columns)
            {
                if (dt2.Columns.Contains(dc.ColumnName))
                {
                    if (!dic.ContainsKey(dc.ColumnName))
                    {
                        dic[dc.ColumnName] = true;
                    }
                }
            }

            foreach (DataColumn dc in dt2.Columns)
            {
                if (dt1.Columns.Contains(dc.ColumnName))
                {
                    if (!dic.ContainsKey(dc.ColumnName))
                    {
                        dic[dc.ColumnName] = true;
                    }
                }
            }

            StringBuilder sb = new Text.StringBuilder();

            foreach (KeyValuePair<string, bool> kv in dic)
            {
                if (sb.Length > 0)
                    sb.Append(",");

                sb.Append("`");
                sb.Append(kv.Key);
                sb.Append("`");
            }

            StringBuilder sb2 = new Text.StringBuilder();
            sb2.Append("insert into `");
            sb2.Append(tableTo);
            sb2.Append("`(");
            sb2.Append(sb.ToString());
            sb2.Append(") select ");
            sb2.Append(sb.ToString());
            sb2.Append(" from `");
            sb2.Append(tableFrom);
            sb2.Append("`;");

            cmd.CommandText = sb2.ToString();
            cmd.ExecuteNonQuery();
        }

        /// <summary>
        /// 删除表
        /// </summary>
        /// <param name="table"></param>
        public void DropTable(string table)
        {
            cmd.CommandText = string.Format("drop table if exists `{0}`", table);
            cmd.ExecuteNonQuery();
        }

        /// <summary>
        /// 修改表结构
        /// </summary>
        /// <param name="targetTable"></param>
        /// <param name="newStructure"></param>
        public void UpdateTableStructure(string targetTable, SQLiteTable newStructure)
        {
            newStructure.Name = targetTable + "_temp";

            CreateTable(newStructure);

            CopyAllData(targetTable, newStructure.Name);

            DropTable(targetTable);

            RenameTable(newStructure.Name, targetTable);
        }


        public void AttachDatabase(string database, string alias)
        {
            Execute(string.Format("attach '{0}' as {1};", database, alias));
        }

        public void DetachDatabase(string alias)
        {
            Execute(string.Format("detach {0};", alias));
        }

        #endregion

    }
}