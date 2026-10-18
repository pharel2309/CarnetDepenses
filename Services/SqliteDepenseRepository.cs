using SQLite;

using CarnetDepenses.Models;

namespace CarnetDepenses.Services;

public class SqliteDepenseRepository : IDepenseRepository
{
    private SQLite.SQLiteAsyncConnection? _db;

    private async Task InitAsync()
    {
        if (_db != null) return;
        _db = new SQLite.SQLiteAsyncConnection(Path.Combine(FileSystem.AppDataDirectory, "depenses.db3"));
        await _db.CreateTableAsync<Depense>(); // si absente
    }

    public async Task<List<Depense>> ListerAsync()
    {
        await InitAsync();
        return await _db!.Table<Depense>().ToListAsync();
    }

    public async Task AjouterAsync(Depense d)
    {
        await InitAsync();
        await _db!.InsertAsync(d); // Id renseigne automatiquement
    }

    public async Task ModifierAsync(Depense d)
    {
        await InitAsync();
        await _db!.UpdateAsync(d);
    }

    public async Task SupprimerAsync(Depense d)
    {
        await InitAsync();
        await _db!.DeleteAsync(d);
    }
}
