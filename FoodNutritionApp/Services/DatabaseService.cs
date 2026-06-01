using FoodNutritionApp.Models;
using SQLite;

namespace FoodNutritionApp.Services;

/// <summary>
/// SQLite persistence for scan and search history records.
/// </summary>
public class DatabaseService
{
    private SQLiteAsyncConnection? _connection;
    private readonly string _databasePath;

    public DatabaseService()
    {
        _databasePath = Path.Combine(FileSystem.AppDataDirectory, "foodnutrition.db3");
    }

    private async Task<SQLiteAsyncConnection> GetConnectionAsync()
    {
        if (_connection != null)
        {
            return _connection;
        }

        _connection = new SQLiteAsyncConnection(_databasePath);
        await _connection.CreateTableAsync<HistoryRecord>();
        return _connection;
    }

    public async Task SaveRecordAsync(FoodItem item)
    {
        var db = await GetConnectionAsync();
        await db.InsertAsync(HistoryRecord.FromFoodItem(item));
    }

    public async Task<List<HistoryRecord>> GetAllRecordsAsync()
    {
        var db = await GetConnectionAsync();
        return await db.Table<HistoryRecord>().OrderByDescending(r => r.SavedAt).ToListAsync();
    }

    public async Task DeleteRecordAsync(HistoryRecord record)
    {
        var db = await GetConnectionAsync();
        await db.DeleteAsync(record);
    }

    public async Task ClearAllAsync()
    {
        var db = await GetConnectionAsync();
        await db.DeleteAllAsync<HistoryRecord>();
    }
}
