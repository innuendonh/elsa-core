using System;
using Elsa.Models;
using Elsa.Persistence.MongoDb.Options;
using MongoDB.Driver;

namespace Elsa.Persistence.MongoDb.Services
{
    public class ElsaMongoDbContext
    {
        public ElsaMongoDbContext(ElsaMongoDbOptions options)
        {
            var connectionString = options.ConnectionString;
            var mongoClient = new MongoClient(connectionString);
            var databaseName = options.DatabaseName is not null and not "" ? options.DatabaseName : MongoUrl.Create(connectionString).DatabaseName;

            if (databaseName == null)
                throw new Exception("Please specify a database name, either via the connection string or via the DatabaseName setting.");
            
            MongoDatabase = mongoClient.GetDatabase(databaseName);
        }
        
        protected IMongoDatabase MongoDatabase { get; }

        public IMongoCollection<WorkflowDefinition> WorkflowDefinitions => MongoDatabase.GetCollection<WorkflowDefinition>(CollectionNames.WorkflowDefinitions);
        public IMongoCollection<WorkflowInstance> WorkflowInstances => MongoDatabase.GetCollection<WorkflowInstance>(CollectionNames.WorkflowInstances);
        public IMongoCollection<WorkflowExecutionLogRecord> WorkflowExecutionLog => MongoDatabase.GetCollection<WorkflowExecutionLogRecord>(CollectionNames.WorkflowExecutionLog);
        public IMongoCollection<Bookmark> Bookmarks => MongoDatabase.GetCollection<Bookmark>(CollectionNames.Bookmarks);
        public IMongoCollection<Trigger> Triggers => MongoDatabase.GetCollection<Trigger>(CollectionNames.Triggers);
    }
}
