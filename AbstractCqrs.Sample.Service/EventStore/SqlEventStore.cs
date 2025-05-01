using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Threading.Tasks;

using AbstractCqrs.Core.Domain;
using AbstractCqrs.Core.EventStore;
using AbstractCqrs.Core.EventStore.Impl;
using AbstractCqrs.Core.Resolve;

using Newtonsoft.Json;

namespace AbstractCqrs.Sample.Service.EventStore
{
    [ServiceRegistration(ServiceRegistrationType.Scope)]
    public class SqlEventStore : BaseEventStore
    {
        static SqlEventStore()
        {
            var createTable =
                @"IF NOT EXISTS (SELECT * FROM SYSOBJECTS WHERE NAME='EventStore' AND XTYPE='U')
                                CREATE TABLE [EventStore](
		                            [Id] [bigint] IDENTITY(1,1) NOT NULL,
		                            [RootId] uniqueidentifier NOT NULL,
		                            [EventTimestamp] [datetime] NOT NULL,
		                            [DATA] [nvarchar](max) NOT NULL
		                            PRIMARY KEY ([Id] ASC)
	                            ) ON [PRIMARY]";

            using (var con = new SqlConnection(Config.ConnectionString))
            {
                con.OpenAsync().Wait();

                var cmd = con.CreateCommand();
                cmd.CommandText = createTable;
                cmd.ExecuteNonQuery();
            }
        }

        protected override IEventRecord Create()
        {
            return new EventRecord();
        }

        protected override string Serialize<TRoot>(IEvent<TRoot> evt)
        {
            var settings =
                new JsonSerializerSettings
                {
                    TypeNameHandling =
                        TypeNameHandling.All
                };
            return JsonConvert.SerializeObject(evt, settings);
        }

        protected override IEvent<TRoot> DeSerialize<TRoot>(string data)
        {
            var settings =
                new JsonSerializerSettings
                {
                    TypeNameHandling =
                        TypeNameHandling.All
                };
            return JsonConvert.DeserializeObject<IEvent<TRoot>>(data, settings);
        }

        protected override async Task Save(IList<IEventRecord> records)
        {
            using (var con = new SqlConnection(Config.ConnectionString))
            {
                await con.OpenAsync();
                using (var tx = con.BeginTransaction())
                {
                    var cmd = con.CreateCommand();
                    cmd.Transaction = tx;
                    cmd.CommandText = @"INSERT INTO [dbo].[EventStore]
                                           ([RootId]
		                                   ,[EventTimestamp]
                                           ,[DATA])
                                     VALUES
                                           (@rootId
		                                   ,@eventTimeStamp
		                                   ,@data)";
                    cmd.Prepare();

                    foreach (var record in records)
                    {
                        cmd.Parameters.AddWithValue("@rootId", record.RootId);
                        cmd.Parameters.AddWithValue("@eventTimeStamp",
                            record.EventTimestamp);
                        cmd.Parameters.AddWithValue("@data", record.Data);
                        await cmd.ExecuteNonQueryAsync();
                    }

                    tx.Commit();
                }
            }
        }

        protected override async Task<IList<IEventRecord>> Load(Guid rootId)
        {
            IList<IEventRecord> records = new List<IEventRecord>();
            using (var con = new SqlConnection(Config.ConnectionString))
            {
                await con.OpenAsync();

                var cmd = con.CreateCommand();
                cmd.CommandText = @"SELECT [RootId],[EventTimestamp],[Data] 
                                    FROM [dbo].[EventStore] WHERE [RootId] = @rootId
                                    ORDER BY [EventTimestamp]";
                cmd.Parameters.AddWithValue("rootId", rootId);

                var resultSet = await cmd.ExecuteReaderAsync();
                while (await resultSet.ReadAsync())
                {
                    var record = Create();

                    record.RootId = await resultSet.GetFieldValueAsync<Guid>(0);
                    record.EventTimestamp =
                        await resultSet.GetFieldValueAsync<DateTime>(1);
                    record.Data = await resultSet.GetFieldValueAsync<string>(2);
                    records.Add(record);
                }
            }
            return records;
        }

        public override async Task<Type> GetRootType(Guid rootId)
        {
            var data = string.Empty;
            using (var con = new SqlConnection(Config.ConnectionString))
            {
                await con.OpenAsync();

                var cmd = con.CreateCommand();
                cmd.CommandText = @"SELECT TOP(1) [Data] 
                                    FROM [dbo].[EventStore] WHERE [RootId] = @rootId
                                    ORDER BY [EventTimestamp]";
                cmd.Parameters.AddWithValue("rootId", rootId);

                var resultSet = await cmd.ExecuteReaderAsync();
                if (await resultSet.ReadAsync())
                    data = await resultSet.GetFieldValueAsync<string>(0);
            }

            var settings =
                new JsonSerializerSettings
                {
                    TypeNameHandling =
                        TypeNameHandling.All
                };

            var type = JsonConvert.DeserializeObject(data, settings);
            return type?.GetType().BaseType.GenericTypeArguments[0];
        }
    }
}