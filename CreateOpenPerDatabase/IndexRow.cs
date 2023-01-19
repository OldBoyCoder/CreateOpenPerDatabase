namespace CreateOpenPerDatabase
{
    internal class IndexRow
    {
        public string TableName;
        public string IndexName;
        public string ColumnName;
        public IndexRow(string tableName, string indexName, string columnName)
        {
            if (tableName.Contains("."))
            {
                tableName = tableName.Split(new[] { '.' })[1];
            }
            TableName = tableName;
            IndexName = indexName;
            ColumnName = columnName;
        }
    }
}
