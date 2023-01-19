using System.Data.SQLite;
using System.Data;

namespace CreateOpenPerDatabase
{
    class SQLiteReturnedDataHandler : IReturnedDataHandler
    {
        private List<string> Columns;
        private SQLiteConnection Connection;
        private IDataTypeMapper DataTypeMapper;
        private string TableName;
        public SQLiteReturnedDataHandler(SQLiteConnection connection, string tableName, IDataTypeMapper dataTypeMapper)
        {
            Connection = connection;
            TableName = tableName;
            DataTypeMapper = dataTypeMapper;
        }
        public void CreateContainer(Dictionary<string, string> fields)
        {
            Columns = fields.Select(x => x.Key).ToList();
            var sql = GenerateCreateTable(fields);
            using var cmd = new SQLiteCommand(sql, Connection);
            cmd.ExecuteNonQuery();
        }

        public void CreateIndex(string indexName, List<string> indexColumns)
        {
            var sql = $"CREATE INDEX {TableName}_{indexName} ON {TableName} (";
            sql += String.Join(",", indexColumns);
            sql += ")";
            Console.WriteLine(sql);
            using (var cmd = new SQLiteCommand(sql, Connection))
                try
                {
                    cmd.ExecuteNonQuery();

                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error creating index on table {TableName} {ex.Message}");
                }


        }

        private string GenerateCreateTable(Dictionary<string, string> fields)
        {
            var sql = $"CREATE TABLE {TableName}(";
            sql += string.Join(", ", fields.Select(x => x.Key + " " + DataTypeMapper.ConvertDataType(x.Value)));
            sql += ") ;";
            return sql;
        }

        void IReturnedDataHandler.ProcessRow(List<string> data)
        {
            using (var cmd = new SQLiteCommand())
            {
                cmd.Connection = Connection;
                var sql = $"INSERT INTO {TableName} ( ";
                sql += String.Join(", ", Columns);
                sql += ") VALUES (";
                sql += string.Join(", ", Columns.Select(x => "@" + x));
                sql += ");";
                for (int i = 0; i < data.Count; i++)
                {
                    if (data[i] == "@NULL@")
                        cmd.Parameters.AddWithValue("@" + Columns[i], DBNull.Value);
                    else
                        cmd.Parameters.AddWithValue("@" + Columns[i], data[i]);
                }
                cmd.CommandText = sql;
                cmd.ExecuteNonQuery();
            }
        }
    }
}
