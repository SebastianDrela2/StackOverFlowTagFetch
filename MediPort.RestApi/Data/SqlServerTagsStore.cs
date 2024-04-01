﻿using MediPort.Api.SqlCommands;
using MediPortApi.HttpProcessing;
using MediPortApi.SqlCommands;
using MediPortApi.TagProcessing;
using Microsoft.Data.SqlClient;

namespace MediPort.RestApi.Data
{
    public class SqlServerTagsStore : ITagsStore
    {
        private readonly string _connectionString;

        public SqlServerTagsStore()
        {
            // lord save me for storing credintials here

            var server = "localhost";
            var database = "master";
            var port = "1433";
            var user = "SA";
            var password = "Password1234";

            var connectionString = $"Server={server},{port};Initial Catalog={database};User ID={user};Password={password};Trust Server Certificate=True";

            if (connectionString == null)
            {
                throw new ArgumentNullException(nameof(connectionString));
            }

            using var connection = new SqlConnection(connectionString);
            connection.Open();

            var tableExistsCommand = new TagTableExistsCommand(connection);
            var tableExists = tableExistsCommand.Execute();

            if (!tableExists)
            {
                var createTableCommand = new CreateTagsTableCommand(connection);
                createTableCommand.Execute();
            }

            _connectionString = connectionString;
        }

        public async Task RefreshAllTags(string apiKey)
        {
            await using var connection = new SqlConnection(_connectionString);
            connection.Open();

            var stackOverflowService = new StackOverflowService(connection, apiKey, 1000, default);
            await stackOverflowService.ResetTagsAsync();         
        }

        public async Task<IEnumerable<SimplifiedTag>> GetAllTagsSorted(SortOption sortOption)
        {
            await using var connection = new SqlConnection(_connectionString);
            connection.Open();

            var selectTagsCommand = new SelectTagsFromTableCommand(connection);

            var tags = await Task.Run(() => selectTagsCommand.Execute());
            var simplifiedTagCalculator = new SimplifiedTagCalculator(tags);

            return await Task.Run(() => simplifiedTagCalculator.GetSortedSimplifiedTags(sortOption));

        }

        public async Task<IEnumerable<SimplifiedTag>> GetAllTags()
        {
            await using var connection = new SqlConnection(_connectionString);
            connection.Open();

            var selectTagsCommand = new SelectTagsFromTableCommand(connection);
            
            var tags = await Task.Run(() => selectTagsCommand.Execute());

            connection.Close();

            var simplifiedTagCalculator = new SimplifiedTagCalculator(tags);

            return await Task.Run(simplifiedTagCalculator.GetSimplifiedTags);
        }

        public async Task<SimplifiedTag> GetTag(int id)
        {
            await using var connection = new SqlConnection(_connectionString);
            connection.Open();

            var selectTagsCommand = new SelectTagsFromTableCommand(connection);
            var tags = await Task.Run(() => selectTagsCommand.Execute(id));

            connection.Close();

            var simplifiedTagCalculator = new SimplifiedTagCalculator(tags);

            return await Task.Run(() => simplifiedTagCalculator.GetSimplifiedTags().FirstOrDefault());
        }

        public async Task<SimplifiedTag> CreateTag(Tag tag)
        {
            await using var connection = new SqlConnection(_connectionString);
            connection.Open();

            var insertTagsCommand = new InsertIntoTagTableCommand(connection);

            await Task.Run(() => insertTagsCommand.Execute(tag));

            var simplifiedTagCalculator = new SimplifiedTagCalculator();

            return await Task.Run(() => simplifiedTagCalculator.GetSimplifiedTag(tag));
        }

        public async Task UpdateTag(int id, Tag tag)
        {
            await using var connection = new SqlConnection(_connectionString);
            connection.Open();

            var updateTagCommand = new UpdateTagInTableCommand(connection);
            updateTagCommand.Execute(id, tag);
        }

        public async Task DeleteTag(int id)
        {
            await using var connection = new SqlConnection(_connectionString);
            connection.Open();

            var deleteTagsCommand = new DeleteTagsTableCommand(connection);
            deleteTagsCommand.Execute(id);

        }    
    }
}
