using System.Collections.ObjectModel;
using CarnetDepenses.Models;
using CarnetDepenses.Services;
using Microsoft.Maui.Storage;

namespace CarnetDepenses.ViewModels;

public class DepensesViewModel : BaseViewModel
{
    private readonly IDepenseRepository _repo;
    private string _libelle = "", _montantTexte = "";
    private readonly IApiService _api;
    private bool _isBusy;
    private string _totalConverti = "", _message = "";
    private Depense? _selection;
    private string _budgetTexte = "50000";

    public ObservableCollection<Depense> Depenses { get; } = new();
    public decimal Total => Depenses.Sum(d => d.Montant);
    public decimal TotalMois => Depenses
        .Where(d => d.Date.Month == DateTime.Today.Month && d.Date.Year == DateTime.Today.Year)
        .Sum(d => d.Montant);
    public decimal Budget => decimal.TryParse(BudgetTexte, out decimal b) ? b : 0;
    public double ProgressionBudget => Budget <= 0 ? 0 : Math.Min(1, (double)(TotalMois / Budget));
    public string TexteBudget => Budget <= 0 ? "Budget non défini"
        : $"{TotalMois:N0} / {Budget:N0} FCFA ({ProgressionBudget:P0})";
    public bool BudgetDepasse => Budget > 0 && TotalMois > Budget;

    public string Libelle
    {
        get => _libelle;
        set
        {
            if (SetField(ref _libelle, value)) AjouterCommand.Rafraichir();
        }
    }
    public string MontantTexte
    {
        get => _montantTexte;
        set
        {
            if (SetField(ref _montantTexte, value)) AjouterCommand.Rafraichir();
        }
    }

    public bool IsBusy
    {
        get => _isBusy;
        set { if (SetField(ref _isBusy, value)) ConvertirCommand.Rafraichir(); }
    }
    public string TotalConverti
    {
        get => _totalConverti;
        set => SetField(ref _totalConverti, value);
    }
    public string Message
    {
        get => _message;
        set => SetField(ref _message, value);
    }
    public Depense? Selection
    {
        get => _selection;
        set
        {
            if (SetField(ref _selection, value) && value != null)
            {
                Libelle = value.Libelle;
                MontantTexte = value.Montant.ToString();
            }
        }
    }
    public string BudgetTexte
    {
        get => _budgetTexte;
        set { if (SetField(ref _budgetTexte, value)) RafraichirBudget(); }
    }
    public bool EstEnEdition => Selection != null;
    public string TexteBoutonAjout => EstEnEdition ? "Modifier" : "Ajouter";

    public RelayCommand AjouterCommand { get; }
    public RelayCommand SupprimerCommand { get; }
    public RelayCommand ConvertirCommand { get; }
    public RelayCommand AnnulerCommand { get; }
    public DepensesViewModel(IDepenseRepository repo, IApiService api)
    {
        _repo = repo;
        _api = api;
        AjouterCommand = new RelayCommand(async p => await AjouterOuModifierAsync(),
            p => !string.IsNullOrWhiteSpace(Libelle)
                && decimal.TryParse(MontantTexte, out _));
        SupprimerCommand = new RelayCommand(async p => await SupprimerAsync((Depense)p!));
        ConvertirCommand = new RelayCommand(
            async (_) => await ConvertirAsync(), (_) => !IsBusy);
        AnnulerCommand = new RelayCommand(_ => AnnulerEdition());
    }

    public async Task ChargerAsync()
    {
        Depenses.Clear();
        foreach (var d in await _repo.ListerAsync())
            Depenses.Add(d);
        RafraichirTotaux();
    }

    private void RafraichirTotaux()
    {
        OnPropertyChanged(nameof(Total));
        RafraichirBudget();
    }

    private void RafraichirBudget()
    {
        OnPropertyChanged(nameof(TotalMois));
        OnPropertyChanged(nameof(Budget));
        OnPropertyChanged(nameof(ProgressionBudget));
        OnPropertyChanged(nameof(TexteBudget));
        OnPropertyChanged(nameof(BudgetDepasse));
    }

    private async Task AjouterOuModifierAsync()
    {
        if (!decimal.TryParse(MontantTexte, out decimal m))
            return;
        if (Selection == null)
        {
            var d = new Depense { Libelle = Libelle, Montant = m };
            await _repo.AjouterAsync(d);
            Depenses.Add(d);
        }
        else
        {
            Selection.Libelle = Libelle;
            Selection.Montant = m;
            await _repo.ModifierAsync(Selection);
            int i = Depenses.IndexOf(Selection);
            if (i >= 0) { Depenses.RemoveAt(i); Depenses.Insert(i, Selection); }
            Selection = null;
            OnPropertyChanged(nameof(EstEnEdition));
            OnPropertyChanged(nameof(TexteBoutonAjout));
        }
        RafraichirTotaux();
        Libelle = ""; MontantTexte = "";
    }

    private void AnnulerEdition()
    {
        Selection = null;
        Libelle = ""; MontantTexte = "";
        OnPropertyChanged(nameof(EstEnEdition));
        OnPropertyChanged(nameof(TexteBoutonAjout));
    }

    private async Task SupprimerAsync(Depense d)
    {
        if (Selection == d) AnnulerEdition();
        await _repo.SupprimerAsync(d);
        Depenses.Remove(d);
        RafraichirTotaux();
    }

    private async Task ConvertirAsync()
    {
        IsBusy = true; Message = "";
        try
        {
            var t = await _api.GetTauxAsync(); 
            Preferences.Set("taux_eur", (double)t.Taux["EUR"]); 
            TotalConverti = $"{Total * t.Taux["EUR"]:F2} EUR";
        }
        catch (HttpRequestException)  
        {
            double c = Preferences.Get("taux_eur", 0.0);
            if (c == 0) Message = "Hors ligne, pas de taux.";
            else
            {
                TotalConverti = $"{Total * (decimal)c:F2} EUR (cache)";
                Message = "Hors ligne : dernier taux connu.";
            }
        }
        finally { IsBusy = false; } 
    }
}
