using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using InMemoryDB.Interfaces;

namespace InMemoryDB.Services
{
    //This will be a registered singleton service
    public class DatabaseService : IDatabaseService
    {
        //Initialize in memory key value store and transaction store
        private readonly Dictionary<string, Record> keyValueStore = new();
        private readonly Dictionary<string, TransactionRecords> transactionStore = new();

        public void PutRecord(string key, string value)
        {
            putRecordHelper(keyValueStore, key, value);
        }

        public string GetRecord(string key)
        {
            return keyValueStore.GetValueOrDefault(key)?.Value;
        }

        public void DeleteRecord(string key)
        {
            keyValueStore.Remove(key);
        }

        public void CreateTransaction(string transactionId)
        {
            if (transactionStore.ContainsKey(transactionId))
                throw new Exception("Dublicate Transaction Id");
            transactionStore.Add(transactionId, new());
        }

        public void RollbackTransaction(string transactionId)
        {
            if (!transactionStore.ContainsKey(transactionId))
                throw new Exception("Invalid Transaction Id");
            transactionStore.Remove(transactionId);
        }

        public void CommmitTransaction(string transactionId)
        {
            if (!transactionStore.ContainsKey(transactionId))
                throw new Exception("Invalid Transaction Id");
            //Get TransactionRecords for the transaction and commit data to keyValueStore
            TransactionRecords tranRecords = transactionStore.GetValueOrDefault(transactionId);

            //First check if any keys are mutated since transaction was created
            foreach(var item in tranRecords.Records)
            {
                var originalRecord = keyValueStore.GetValueOrDefault(item.Key);
                if (originalRecord?.TimeStamp > tranRecords.TransactionStartTime)
                {
                    transactionStore.Remove(transactionId);
                    throw new Exception("Transaction conflict - key mutation has occured since this transaction started");
                }
            }

            //Now commit the transaction and update/create records as necessary
            foreach(var item in tranRecords.Records)
            {
                putRecordHelper(keyValueStore, item.Key, item.Value?.Value);
            }

            transactionStore.Remove(transactionId);
        }

        public void PutRecordWithTransaction(string key, string value, string transactionId)
        {
            if (!transactionStore.ContainsKey(transactionId))
                throw new Exception("Invalid Transaction Id");

            //Get original value for potential use during a more complex rollback (not implemented)
            var originalValue = GetRecord(key);

            //Get TransactionRecords and add/update new record accordingly
            TransactionRecord tranRecord = new TransactionRecord(value, originalValue);
            TransactionRecords tranRecords = transactionStore.GetValueOrDefault(transactionId);
            putTransactionRecordHelper(tranRecords.Records, key, tranRecord);
            transactionStore[key] = tranRecords;
        }

        public string GetRecordWithTransaction(string key, string transactionId)
        {
            if (!transactionStore.ContainsKey(transactionId))
                throw new Exception("Invalid Transaction Id");
            //Get TransactionRecords for the transaction and get value for the key
            TransactionRecords tranRecords = transactionStore.GetValueOrDefault(transactionId);
            if (tranRecords.Records?.GetValueOrDefault(key) != null)
                return tranRecords.Records?.GetValueOrDefault(key)?.Value;
            else
                return keyValueStore.GetValueOrDefault(key)?.Value;
        }

        public void DeleteRecordWithTransaction(string key, string transactionId)
        {
            if (!transactionStore.ContainsKey(transactionId))
                throw new Exception("Invalid Transaction Id");
            ///Get TransactionRecords for the transaction and delete record accordingly.
            TransactionRecords tranRecords = transactionStore.GetValueOrDefault(transactionId);
            tranRecords.Records?.Remove(key);
        }

        internal void putRecordHelper(Dictionary<string, Record> dictionary, string key, string value)
        {
            if (dictionary.ContainsKey(key))
                dictionary[key] = new Record(value);
            else
                dictionary.Add(key, new Record(value));
        }

        internal void putTransactionRecordHelper(Dictionary<string, TransactionRecord> dictionary, string key, TransactionRecord record)
        {
            if (dictionary.ContainsKey(key))
                dictionary[key] = record;
            else
                dictionary.Add(key, record);
        }

        internal class Record
        {
            public Record(string value)
            {
                Value = value;
            }

            public string Value { get; set; }
            public DateTime TimeStamp { get; set; } = DateTime.Now;
        }

        internal class TransactionRecord
        {
            public TransactionRecord(string value, string originalValue)
            {
                Value = value;
                OriginalValue = originalValue;
            }

            public string Value { get; set; }
            public string OriginalValue { get; set; }
        }

        internal class TransactionRecords
        {
            public DateTime TransactionStartTime { get; set; } = DateTime.Now;
            public Dictionary<string, TransactionRecord> Records { get; set; } = new();
        }
    }
}
