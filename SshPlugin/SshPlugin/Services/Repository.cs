using SQLite;
using SshPlugin.Models;

namespace SshPlugin.Services;

internal class Repository
{
    private SQLiteAsyncConnection _database;
    private Guid ConnectionId { get; }

    internal Repository(string dataPath, Guid connectionId)
    {
        ConnectionId = connectionId;
        _database = new SQLiteAsyncConnection(Path.Join(dataPath, "Database.db"), SQLiteOpenFlags.ReadWrite | SQLiteOpenFlags.Create);
        _database.CreateTableAsync<FileEntity>();
    }

    public async Task<FileEntity> GetFile(Guid id)
    {
        return await _database.GetAsync<FileEntity>(e => e.ConnectionId == ConnectionId && e.Id == id);
    }

    public async Task<FileEntity> GetFileByLocalPath(string localPath)
    {
        return await _database.GetAsync<FileEntity>(e => e.ConnectionId == ConnectionId && e.LocalPath == localPath);
    }

    public async Task UpdateFile(FileEntity entity)
    {
        await _database.UpdateAsync(entity);
    }

    public async Task InsertFile(FileEntity entity)
    {
        await _database.InsertAsync(entity);
    }
}