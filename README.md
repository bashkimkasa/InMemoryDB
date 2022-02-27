# InMemoryDB
DotNet simple in-memory db example.

## Overview
This project is a very simple in-memory db implemented with DotNet5.0. It supports basic transactions.  
It uses dictionaries for the in-memory repos.  
It provides a swagger interface to test with at (default port 5000): http://localhost:5000/swagger/index.html  
For simplicity all the APIs are exposed as HTTP GET Requests (with required and optional query parameters - documented in swagger).  
It exposes the following APIs:
  1. /database/put-record (takes in required query params: ['key', 'value'], and optional params: ['transactionId'])
  2. /database/get-record (takes in required query params: ['key'] and optional param: ['transactionId'])
  3. /database/delete-record (takes in required query params: ['key'] and optional param: ['transactionId'])
  4. /database/create-transaction (takes in required query params: ['transactionId'])
  5. /database/rollback-transaction (takes in required query params: ['transactionId'])
  6. /database/commit-transaction (takes in required query params: ['transactionId'])

## Getting Started
1. Clone this project locally
2. Ensure that you have .NET 5.0 or later
3. From the root folder of your local repository, run `dotnet build`. Resolve any build failures.
4. After a successful build, locate and execute \<your-local-folder>\InMemoryDB\bin\Release\net5.0\InMemoryDB.exe
5. As mentioned above you can access the swagger page at this location: http://localhost:5000/swagger/index.html

## Usage and Notes
Any of the put-record, get-record, or delete-record operations without a transactionId (optional) is committed immidiately.  
In order to use transactionId with these operations a transaction has to be created first using create-transaction operation.  
When ready to commit a transaction use commit-transaction operation. Whenever a transaction needs to be cancelled use rollback-transaction operation.  
  
Transaction isolation closely follows the read committed isolation level. Hence, the read operations do not need to be stored in the transaction repo (as read-locks are released as soon as select statements complete in this isolation level). In addition, in this implementation 'write' transactions do not block other transactions (either read or write).
  
When committing or rolling back a transaction the transactionId is invalidated (and deleted) accordingly.  
Also, when committing a transaction, it will fail and error out accordingly if there is a conflict (meaning the transaction attempts to change a value for a key that was mutated after the transaction was created).

Error handling is very simple and all errors are returned as HTTP 500 with the appropriate message detail for the particular error.

Here is an example request sequence without transactions:
```console
http://localhost:5000/database/put-record?key=example&value=foo   -> returns null
http://localhost:5000/database/get-record?key=example             -> returns "foo"
http://localhost:5000/database/delete-record?key=example          -> returns null
http://localhost:5000/database/get-record?key=example             -> returns null
http://localhost:5000/database/delete-record?key=example          -> returns null
```
  
Here is an example request sequence with transactions:
```console
http://localhost:5000/database/create-transaction?transactionId=abc           -> returns null
http://localhost:5000/database/put-record?key=a&value=foo&transactionId=abc   -> returns null
http://localhost:5000/database/get-record?key=a&transactionId=abc             -> returns "foo"
http://localhost:5000/database/get-record?key=a                               -> returns null

http://localhost:5000/database/create-transaction?transactionId=xyz           -> returns null
http://localhost:5000/database/put-record?key=a&value=bar&transactionId=xyz   -> returns null
http://localhost:5000/database/get-record?key=a&transactionId=xyz             -> returns "bar"
http://localhost:5000/database/commit-transaction?transactionId=xyz           -> returns null
http://localhost:5000/database/get-record?key=a                               -> returns "bar"

http://localhost:5000/database/commit-transaction?transactionId=abc           -> returns error (conflict - mutated key)

http://localhost:5000/database/get-record?key=a                               -> returns "bar"

http://localhost:5000/database/create-transaction?transactionId=abc           -> returns null
http://localhost:5000/database/put-record?key=a&value=foo&transactionId=abc   -> returns null
http://localhost:5000/database/get-record?key=a                               -> returns "bar"
http://localhost:5000/database/rollback-transaction?transactionId=abc         -> returns null
http://localhost:5000/database/put-record?key=a&value=foo&transactionId=abc   -> returns error (invalid transactionId)
http://localhost:5000/database/get-record?key=a                               -> returns "bar"

http://localhost:5000/database/create-transaction?transactionId=def           -> returns null
http://localhost:5000/database/put-record?key=b&value=foo&transactionId=def   -> returns null
http://localhost:5000/database/get-record?key=a&transactionId=def             -> returns "bar"
http://localhost:5000/database/get-record?key=b&transactionId=def             -> returns "foo"
http://localhost:5000/database/rollback-transaction?transactionId=def         -> returns null
http://localhost:5000/database/get-record?key=b                               -> returns null
```
