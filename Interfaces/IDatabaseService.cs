namespace InMemoryDB.Interfaces
{
    public interface IDatabaseService
    {
        public void PutRecord(string key, string value);
        public string GetRecord(string key);
        public void DeleteRecord(string key);

        public void CreateTransaction(string transactionId);
        public void RollbackTransaction(string transactionId);
        public void CommmitTransaction(string transactionId);

        public void PutRecordWithTransaction(string key, string value, string transactiondId);
        public string GetRecordWithTransaction(string key, string transactiondId);
        public void DeleteRecordWithTransaction(string key, string transactionId);
    }
}
