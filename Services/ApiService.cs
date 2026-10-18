using System.Net.Http.Json;
using CarnetDepenses.Models;

namespace CarnetDepenses.Services;

public interface IApiService
{
    Task<TauxReponse> GetTauxAsync();
}

public class ApiService : IApiService
{
    private readonly HttpClient _http;
    public ApiService(HttpClient http) => _http = http;
    public async Task<TauxReponse> GetTauxAsync() => await _http.GetFromJsonAsync<TauxReponse>("https://api.exemple.cm/taux")
        ?? throw new InvalidOperationException("Reponse vide");
}
