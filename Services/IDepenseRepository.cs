using CarnetDepenses.Models;

namespace CarnetDepenses.Services;

public interface IDepenseRepository
{
    Task<List<Depense>> ListerAsync();
    Task AjouterAsync(Depense d);
    Task ModifierAsync(Depense d);
    Task SupprimerAsync(Depense d);
}
