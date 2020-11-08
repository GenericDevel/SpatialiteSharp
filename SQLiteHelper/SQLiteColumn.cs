using System;
using System.Collections.Generic;
using System.Text;

namespace System.Data.SQLite
{
    public class SQLiteColumn
    {
        public string Name = "";
        public bool PrimaryKey = false;
        public FieldType DataType = FieldType.Text;
        public bool AutoIncrement = false;
        public bool NotNull = false;
        public string DefaultValue = "";

        public SQLiteColumn()
        { }

        public SQLiteColumn(string colName)
        {
            Name = colName;
            PrimaryKey = false;
            DataType = FieldType.Text;
            AutoIncrement = false;
        }

        public SQLiteColumn(string colName, FieldType colDataType)
        {
            Name = colName;
            PrimaryKey = false;
            DataType = colDataType;
            AutoIncrement = false;
        }

        public SQLiteColumn(string colName, bool autoIncrement)
        {
            Name = colName;

            if (autoIncrement)
            {
                PrimaryKey = true;
                DataType = FieldType.Integer;
                AutoIncrement = true;
            }
            else
            {
                PrimaryKey = false;
                DataType = FieldType.Text;
                AutoIncrement = false;
            }
        }

        public SQLiteColumn(string colName, FieldType colDataType, bool primaryKey, bool autoIncrement, bool notNull, string defaultValue)
        {
            Name = colName;

            if (autoIncrement)
            {
                PrimaryKey = true;
                DataType = FieldType.Integer;
                AutoIncrement = true;
            }
            else
            {
                PrimaryKey = primaryKey;
                DataType = colDataType;
                AutoIncrement = false;
                NotNull = notNull;
                DefaultValue = defaultValue;
            }
        }
    }
}
