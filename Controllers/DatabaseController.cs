using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using InMemoryDB.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace InMemoryDB.Controllers
{
    [ApiController]
    [Route("database")]
    public class DatabaseController : Controller
    {

        private readonly IDatabaseService _dbService;

        public DatabaseController(IDatabaseService dbService)
        {
            _dbService = dbService;
        }

        [HttpGet("put-record", Name = "PutRecord")]
        public void PutRecord([FromQueryAttribute, BindRequired] string key, [BindRequired] string value, string transactionId)
        {
            if (transactionId != null)
                _dbService.PutRecordWithTransaction(key, value, transactionId);
            else
                _dbService.PutRecord(key, value);
        }

        [HttpGet("get-record", Name = "GetRecord")]
        public string GetRecord([FromQueryAttribute, BindRequired] string key, string transactionId)
        {
            if (transactionId != null)
                return _dbService.GetRecordWithTransaction(key, transactionId);
            return _dbService.GetRecord(key);
        }

        [HttpGet("delete-record", Name = "DeleteRecord")]
        public void DeleteRecord([FromQueryAttribute, BindRequired] string key, string transactionId)
        {
            if (transactionId != null)
                _dbService.DeleteRecordWithTransaction(key, transactionId);
            else
                _dbService.DeleteRecord(key);
        }

        [HttpGet("create-transaction", Name = "CreateTransaction")]
        public void CreateTransaction([FromQueryAttribute, BindRequired] string transactionId)
        {
            _dbService.CreateTransaction(transactionId);
        }

        [HttpGet("rollback-transaction", Name = "RollbackTransaction")]
        public void RollbackTransaction([FromQueryAttribute, BindRequired] string transactionId)
        {
            _dbService.RollbackTransaction(transactionId);
        }

        [HttpGet("commit-transaction", Name = "CommmitTransaction")]
        public void CommmitTransaction([FromQueryAttribute, BindRequired] string transactionId)
        {
            _dbService.CommmitTransaction(transactionId);
        }
    }
}
