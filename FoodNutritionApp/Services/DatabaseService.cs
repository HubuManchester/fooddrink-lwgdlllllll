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
    private bool _initialized;

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
        await MigrateSchemaAsync(_connection);
        return _connection;
    }

    private static async Task MigrateSchemaAsync(SQLiteAsyncConnection db)
    {
        try
        {
            var columns = await db.QueryAsync<TableInfo>("PRAGMA table_info(HistoryRecords)");
            var hasCategory = columns.Any(c =>
                string.Equals(c.Name, "Category", StringComparison.OrdinalIgnoreCase));

            if (!hasCategory)
            {
                await db.ExecuteAsync("ALTER TABLE HistoryRecords ADD COLUMN Category TEXT DEFAULT 'Other'");
            }
        }
        catch (SQLiteException)
        {
            // Table may not exist yet on first run; CreateTableAsync handles new databases.
        }
    }

    private sealed class TableInfo
    {
        public string Name { get; set; } = string.Empty;
    }

    public async Task InitializeAsync(LocalFoodDataService localFoodData)
    {
        if (_initialized)
        {
            return;
        }

        await GetConnectionAsync();
        var db = _connection!;
        var count = await db.Table<HistoryRecord>().CountAsync();
        if (count == 0)
        {
            var seedFoods = await localFoodData.GetAllFoodsAsync();
            foreach (var food in seedFoods.Take(6))
            {
                await db.InsertAsync(HistoryRecord.FromFoodItem(food));
            }
        }

        _initialized = true;
    }

    public async Task SaveRecordAsync(FoodItem item)
    {
        var db = await GetConnectionAsync();
        await db.InsertAsync(HistoryRecord.FromFoodItem(item));
    }

    public async Task UpdateRecordAsync(HistoryRecord record)
    {
        var db = await GetConnectionAsync();
        await db.UpdateAsync(record);
    }

    public async Task<HistoryRecord?> GetRecordByIdAsync(int id)
    {
        var db = await GetConnectionAsync();
        return await db.Table<HistoryRecord>().FirstOrDefaultAsync(r => r.Id == id);
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
