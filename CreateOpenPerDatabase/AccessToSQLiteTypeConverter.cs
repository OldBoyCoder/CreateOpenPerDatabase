namespace CreateOpenPerDatabase
{
    internal class AccessToSQLiteTypeConverter : IDataTypeMapper
    {
        string IDataTypeMapper.ConvertDataType(string dataType)
        {
            if (dataType == "DBTYPE_BOOL") return "INTEGER";
            if (dataType == "DBTYPE_I1") return "INTEGER";
            if (dataType == "DBTYPE_UI1") return "INTEGER";
            if (dataType == "DBTYPE_I2") return "INTEGER";
            if (dataType == "DBTYPE_I4") return "INTEGER";
            if (dataType == "DBTYPE_R4") return "REAL";
            if (dataType == "DBTYPE_R8") return "REAL";
            return "TEXT";
        }
    }
}
