using System.Data.SQLite;
using System.Data.OleDb;
using System.Data;

namespace CreateOpenPerDatabase
{

    internal class AccessDatabaseTransfer
    {
        static List<string> GetAccessTables(OleDbConnection conn)
        {
            var tableList = new List<string>();
            var tables = conn.GetOleDbSchemaTable(System.Data.OleDb.OleDbSchemaGuid.Tables, restrictions: new object[] { null, null, null, "TABLE" });
            if (tables != null)
            foreach (DataRow table in tables.Rows)
            {
                tableList.Add(table["TABLE_NAME"].ToString());
            }
            return tableList;
        }
        public static void ProcessAccessTables(string dbFile)
        {
            var password = "\u0001\u0007\u0014\u0007\u0001\u00f3\u001b\n\n\u00d2\u001e\u00da\u00b1";
            OleDbConnectionStringBuilder builder = new OleDbConnectionStringBuilder();
            builder.ConnectionString = $"Data Source=.\\Data\\SP.DB.04210.FCTLR";
            //builder.ConnectionString = $"Data Source={ePerPath}\\SP.DB.00900.FCTLR";
            builder.Add("Provider", "Microsoft.ACE.OLEDB.12.0");// "Microsoft.Jet.Oledb.4.0");
            builder.Add("Jet OLEDB:Database Password", password);

            using (var sQLiteConnection = new SQLiteConnection($"Data Source={dbFile}"))
            {
                sQLiteConnection.Open();
                using (var accessConnection = new OleDbConnection(builder.ConnectionString))
                {
                    accessConnection.Open();
                    var tables = GetAccessTables(accessConnection);
                    foreach (var table in tables)
                    {
                        Console.WriteLine(table);
                        ExtractAccessTableToTable(table, sQLiteConnection, accessConnection);
                    }
                    BuildIndices(accessConnection, sQLiteConnection, tables);
                }
            }
        }
        static void ExtractAccessTableToTable(string tableName, SQLiteConnection SQLiteconnection, OleDbConnection accessConnection)
        {
            var sql = $"SELECT * FROM {tableName}";
            IReturnedDataHandler handler = new SQLiteReturnedDataHandler(SQLiteconnection, tableName, new AccessToSQLiteTypeConverter());
            using (var txn = SQLiteconnection.BeginTransaction())
            {
                using (var cmd = new OleDbCommand(sql, accessConnection))
                {
                    var reader = cmd.ExecuteReader();
                    var first = true;
                    while (reader.Read())
                    {
                        if (first)
                        {
                            first = false;
                            var fields = new Dictionary<string, string>();
                            for (int i = 0; i < reader.FieldCount; i++)
                            {
                                var dataType = reader.GetDataTypeName(i);
                                fields.Add(reader.GetName(i), dataType);
                                //if (!dataTypes.Contains(dataType))
                                //    dataTypes.Add(dataType);
                            }
                            handler.CreateContainer(fields);
                        }
                        var data = new List<string>();
                        for (int i = 0; i < reader.FieldCount; i++)
                        {
                            if (reader.IsDBNull(i))
                                data.Add("@NULL@");
                            else
                                data.Add(reader.GetValue(i).ToString());
                        }
                        handler.ProcessRow(data);
                    }
                }
                txn.Commit();
            }
        }
        static void BuildIndices(OleDbConnection accessConn, SQLiteConnection sqlConn, List<string> tables)
        {
            var AllIndexData = new List<IndexRow>();
            var indices = accessConn.GetSchema("Indexes", null);


            foreach (DataRow dataRow in indices.Rows)
            {
                if (dataRow[2] != null && !dataRow[2].ToString().StartsWith("MSys"))
                    AllIndexData.Add(new IndexRow(dataRow[2].ToString(), dataRow[5].ToString(), dataRow[17].ToString()));
            }
            foreach (var index in AllIndexData.Select(x => x.IndexName).Distinct())
            {
                Console.WriteLine(index);
                var tableName = AllIndexData.First(x => x.IndexName == index).TableName;
                if (tables.Contains(tableName))
                {
                    var sql = $"CREATE INDEX {index} ON {tableName} (";
                    sql += String.Join(",", AllIndexData.Where(x => x.TableName == tableName && x.IndexName == index).Select(x => x.ColumnName));
                    sql += ")";
                    using (var cmd = new SQLiteCommand(sql, sqlConn))
                        try
                        {
                            cmd.ExecuteNonQuery();

                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"Error creating index on table {tableName} {ex.Message}");
                        }
                }
            }

        }
    }
}
