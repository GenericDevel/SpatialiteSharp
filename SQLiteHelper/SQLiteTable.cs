using System;
using System.Collections.Generic;
using System.Text;

namespace System.Data.SQLite
{
    public class SQLiteTable
    {
        public string Name = "";
        public SQLiteColumnList Columns = new SQLiteColumnList();

        public SQLiteTable()
        { }

        public SQLiteTable(string name)
        {
            Name = name;
        }
    }
}