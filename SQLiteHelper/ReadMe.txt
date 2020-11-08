1.创建数据库
  打开数据连接，如果文件不存在，则创建新库
  SQLiteHelper.Instance.Open(path);

2.创建数据表
      SQLiteTable tb = new SQLiteTable(tableName);
      SQLiteColumn field = new SQLiteColumn();
      field.ColumnName = colName;
      field.ColDataType = ColType.Text;
      tb.Columns.Add(field);
      
     SQLiteHelper.Instance.CreateTable(tb);

3.利用事务插入数据
    SQLiteHelper sh = SQLiteHelper.Instance;
    sh.BeginTransaction();

     for (int i = 0; i < 5; i++)
     {
              var dic = new Dictionary<string, object>();
              dic["name"] = "Product " + (count + i);
              dic["description"] = "Some description";
              dic["status"] = "Active";
              sh.Insert("product", dic);
       }
       sh.Commit();

4.带参数查询
  