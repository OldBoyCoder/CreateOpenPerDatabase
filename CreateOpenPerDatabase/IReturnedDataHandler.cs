namespace CreateOpenPerDatabase
{
    public interface IReturnedDataHandler
    {
        void CreateContainer(Dictionary<string, string> fields);
        void ProcessRow(List<string> data);

        void CreateIndex(string indexName, List<string> indexColumns);
    }
}
