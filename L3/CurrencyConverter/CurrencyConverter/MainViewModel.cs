using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Input;
using CurrencyConverter.Data;
using CurrencyConverter.Models;

namespace CurrencyConverter
{
    public class MainViewModel : INotifyPropertyChanged
    {
        private readonly CurrencyDbContext _db;

        private Currency? _selectedCurrency;
        private string _foreignAmountText = string.Empty;
        private string _uahAmountText = string.Empty;
        private bool _isUpdating;
        private string _statusMessage = string.Empty;
        private bool _isEditing;

        public MainViewModel()
        {
            _db = new CurrencyDbContext();
            _db.Database.EnsureCreated(); // Автоматично створює БД і таблиці

            LoadCurrencies();
            LoadHistory();

            SelectedCurrency = Currencies.FirstOrDefault();

            SwapCommand = new RelayCommand(ExecuteSwap, _ => SelectedCurrency != null);
            ClearCommand = new RelayCommand(ExecuteClear);
            AddCurrencyCommand = new RelayCommand(ExecuteAddCurrency, CanExecuteAddCurrency);
            RemoveCurrencyCommand = new RelayCommand(ExecuteRemoveCurrency, _ => SelectedCurrency != null);
            SaveCurrencyCommand = new RelayCommand(ExecuteSaveCurrency, CanExecuteSaveCurrency);
            CancelEditCommand = new RelayCommand(ExecuteCancelEdit);
            ClearHistoryCommand = new RelayCommand(ExecuteClearHistory);
        }

        // ── Collections ──────────────────────────────────────────────────────
        public ObservableCollection<Currency> Currencies { get; } = new();
        public ObservableCollection<ConversionHistory> History { get; } = new();

        private void LoadCurrencies()
        {
            var fromDb = _db.Currencies.ToList();

            if (!fromDb.Any())
            {
                // Початковий seed при першому запуску
                var seed = new[]
                {
                    new Currency { Code = "USD", Name = "Долар США", Flag = "🇺🇸", Rate = 41.20, Units = 1 },
                    new Currency { Code = "EUR", Name = "Євро", Flag = "🇪🇺", Rate = 44.80, Units = 1 },
                    new Currency { Code = "GBP", Name = "Фунт стерлінгів", Flag = "🇬🇧", Rate = 52.30, Units = 1 },
                    new Currency { Code = "CHF", Name = "Швейцарський франк", Flag = "🇨🇭", Rate = 46.10, Units = 1 },
                    new Currency { Code = "PLN", Name = "Польський злотий", Flag = "🇵🇱", Rate = 9.85, Units = 1 },
                    new Currency { Code = "CZK", Name = "Чеська крона", Flag = "🇨🇿", Rate = 1.73, Units = 1 },
                    new Currency { Code = "JPY", Name = "Японська єна", Flag = "🇯🇵", Rate = 27.40, Units = 100 },
                    new Currency { Code = "CNY", Name = "Китайський юань", Flag = "🇨🇳", Rate = 5.68, Units = 1 },
                    new Currency { Code = "CAD", Name = "Канадський долар", Flag = "🇨🇦", Rate = 29.60, Units = 1 },
                    new Currency
                        { Code = "AUD", Name = "Австралійський долар", Flag = "🇦🇺", Rate = 26.30, Units = 1 },
                };
                _db.Currencies.AddRange(seed);
                _db.SaveChanges();
                fromDb = _db.Currencies.ToList();
            }

            foreach (var c in fromDb) Currencies.Add(c);
        }

        private void LoadHistory()
        {
            var records = _db.History
                .OrderByDescending(h => h.Timestamp)
                .Take(100)
                .ToList();
            foreach (var h in records) History.Add(h);
        }

        // ── Selected currency ────────────────────────────────────────────────
        public Currency? SelectedCurrency
        {
            get => _selectedCurrency;
            set
            {
                _selectedCurrency = value;
                OnPropertyChanged(nameof(SelectedCurrency));
                RecalcFromForeign();
                if (value != null && !IsEditing)
                {
                    EditCode = value.Code;
                    EditName = value.Name;
                    EditFlag = value.Flag;
                    EditRate = value.Rate.ToString("G");
                    EditUnits = value.Units.ToString();
                }
            }
        }

        // ── Conversion inputs ────────────────────────────────────────────────
        public string ForeignAmountText
        {
            get => _foreignAmountText;
            set
            {
                _foreignAmountText = value;
                OnPropertyChanged(nameof(ForeignAmountText));
                if (!_isUpdating) RecalcFromForeign();
            }
        }

        public string UahAmountText
        {
            get => _uahAmountText;
            set
            {
                _uahAmountText = value;
                OnPropertyChanged(nameof(UahAmountText));
                if (!_isUpdating) RecalcFromUah();
            }
        }

        public string StatusMessage
        {
            get => _statusMessage;
            set
            {
                _statusMessage = value;
                OnPropertyChanged(nameof(StatusMessage));
            }
        }

        // ── Edit fields ──────────────────────────────────────────────────────
        public bool IsEditing
        {
            get => _isEditing;
            set
            {
                _isEditing = value;
                OnPropertyChanged(nameof(IsEditing));
                OnPropertyChanged(nameof(IsNotEditing));
            }
        }

        public bool IsNotEditing => !IsEditing;

        private string _editCode = string.Empty;
        private string _editName = string.Empty;
        private string _editFlag = string.Empty;
        private string _editRate = string.Empty;
        private string _editUnits = "1";

        public string EditCode
        {
            get => _editCode;
            set
            {
                _editCode = value;
                OnPropertyChanged(nameof(EditCode));
            }
        }

        public string EditName
        {
            get => _editName;
            set
            {
                _editName = value;
                OnPropertyChanged(nameof(EditName));
            }
        }

        public string EditFlag
        {
            get => _editFlag;
            set
            {
                _editFlag = value;
                OnPropertyChanged(nameof(EditFlag));
            }
        }

        public string EditRate
        {
            get => _editRate;
            set
            {
                _editRate = value;
                OnPropertyChanged(nameof(EditRate));
            }
        }

        public string EditUnits
        {
            get => _editUnits;
            set
            {
                _editUnits = value;
                OnPropertyChanged(nameof(EditUnits));
            }
        }

        // ── Commands ─────────────────────────────────────────────────────────
        public ICommand SwapCommand { get; }
        public ICommand ClearCommand { get; }
        public ICommand AddCurrencyCommand { get; }
        public ICommand RemoveCurrencyCommand { get; }
        public ICommand SaveCurrencyCommand { get; }
        public ICommand CancelEditCommand { get; }
        public ICommand ClearHistoryCommand { get; }

        private void ExecuteSwap(object? _)
        {
            if (SelectedCurrency == null) return;
            (_foreignAmountText, _uahAmountText) = (_uahAmountText, _foreignAmountText);
            OnPropertyChanged(nameof(ForeignAmountText));
            OnPropertyChanged(nameof(UahAmountText));
            RecalcFromForeign();
            StatusMessage = "Суми поміняно місцями.";
        }

        private void ExecuteClear(object? _)
        {
            _isUpdating = true;
            ForeignAmountText = string.Empty;
            UahAmountText = string.Empty;
            _isUpdating = false;
            StatusMessage = "Поля очищено.";
        }

        private void ExecuteAddCurrency(object? _)
        {
            IsEditing = true;
            EditCode = string.Empty;
            EditName = string.Empty;
            EditFlag = "🏳️";
            EditRate = string.Empty;
            EditUnits = "1";
            StatusMessage = "Заповніть дані нової валюти.";
        }

        private bool CanExecuteAddCurrency(object? _) => IsNotEditing;

        private void ExecuteRemoveCurrency(object? _)
        {
            if (SelectedCurrency == null) return;
            var name = SelectedCurrency.Name;
            _db.Currencies.Remove(SelectedCurrency);
            _db.SaveChanges();
            Currencies.Remove(SelectedCurrency);
            SelectedCurrency = Currencies.FirstOrDefault();
            StatusMessage = $"Валюту «{name}» видалено.";
        }

        private void ExecuteSaveCurrency(object? _)
        {
            if (!double.TryParse(EditRate.Replace(',', '.'),
                    System.Globalization.NumberStyles.Any,
                    System.Globalization.CultureInfo.InvariantCulture,
                    out double rate) || rate <= 0)
            {
                StatusMessage = "⚠ Невірний курс. Введіть позитивне число.";
                return;
            }

            if (!int.TryParse(EditUnits, out int units) || units <= 0) units = 1;

            if (SelectedCurrency != null &&
                string.Equals(SelectedCurrency.Code, EditCode, StringComparison.OrdinalIgnoreCase))
            {
                SelectedCurrency.Name = EditName;
                SelectedCurrency.Flag = EditFlag;
                SelectedCurrency.Rate = rate;
                SelectedCurrency.Units = units;
                _db.Currencies.Update(SelectedCurrency); // ← EF Update
                _db.SaveChanges();
                StatusMessage = $"Курс {EditCode} оновлено → {rate} грн / {units} од.";
            }
            else
            {
                if (string.IsNullOrWhiteSpace(EditCode))
                {
                    StatusMessage = "⚠ Введіть код валюти.";
                    return;
                }

                if (Currencies.Any(c => string.Equals(c.Code, EditCode, StringComparison.OrdinalIgnoreCase)))
                {
                    StatusMessage = $"⚠ Валюта {EditCode} вже існує.";
                    return;
                }

                var newCur = new Currency
                {
                    Code = EditCode.ToUpper(), Name = EditName,
                    Flag = EditFlag, Rate = rate, Units = units
                };
                _db.Currencies.Add(newCur); // ← EF Insert
                _db.SaveChanges();
                Currencies.Add(newCur);
                SelectedCurrency = newCur;
                StatusMessage = $"Валюту {newCur.Code} додано та збережено в БД.";
            }

            IsEditing = false;
            RecalcFromForeign();
        }

        private bool CanExecuteSaveCurrency(object? _) => IsEditing;

        private void ExecuteCancelEdit(object? _)
        {
            IsEditing = false;
            if (SelectedCurrency != null)
            {
                EditCode = SelectedCurrency.Code;
                EditName = SelectedCurrency.Name;
                EditFlag = SelectedCurrency.Flag;
                EditRate = SelectedCurrency.Rate.ToString("G");
                EditUnits = SelectedCurrency.Units.ToString();
            }

            StatusMessage = "Редагування скасовано.";
        }

        private void ExecuteClearHistory(object? _)
        {
            _db.History.RemoveRange(_db.History);
            _db.SaveChanges();
            History.Clear();
            StatusMessage = "Журнал конвертацій очищено.";
        }

        // ── Conversion logic ─────────────────────────────────────────────────
        private void RecalcFromForeign()
        {
            if (_isUpdating || SelectedCurrency == null) return;
            _isUpdating = true;
            try
            {
                if (double.TryParse(ForeignAmountText.Replace(',', '.'),
                        System.Globalization.NumberStyles.Any,
                        System.Globalization.CultureInfo.InvariantCulture,
                        out double foreign) && foreign >= 0)
                {
                    double uah = foreign * SelectedCurrency.RatePerUnit;
                    UahAmountText = uah.ToString("F2");
                    StatusMessage = $"{foreign:F2} {SelectedCurrency.Code} = {uah:F2} грн " +
                                    $"(курс: {SelectedCurrency.Rate} грн / {SelectedCurrency.Units} {SelectedCurrency.Code})";

                    // ── Зберегти в журнал БД ──
                    if (foreign > 0)
                    {
                        var entry = new ConversionHistory
                        {
                            CurrencyCode = SelectedCurrency.Code,
                            ForeignAmount = foreign,
                            UahAmount = uah,
                            RateUsed = SelectedCurrency.RatePerUnit,
                            Timestamp = DateTime.Now
                        };
                        _db.History.Add(entry);
                        _db.SaveChanges();
                        History.Insert(0, entry); // додаємо зверху списку
                    }
                }
                else
                {
                    UahAmountText = string.Empty;
                    StatusMessage = string.Empty;
                }
            }
            finally
            {
                _isUpdating = false;
            }
        }

        private void RecalcFromUah()
        {
            if (_isUpdating || SelectedCurrency == null) return;
            _isUpdating = true;
            try
            {
                if (double.TryParse(UahAmountText.Replace(',', '.'),
                        System.Globalization.NumberStyles.Any,
                        System.Globalization.CultureInfo.InvariantCulture,
                        out double uah) && uah >= 0 && SelectedCurrency.RatePerUnit > 0)
                {
                    double foreign = uah / SelectedCurrency.RatePerUnit;
                    ForeignAmountText = foreign.ToString("F4");
                    StatusMessage = $"{uah:F2} грн = {foreign:F4} {SelectedCurrency.Code} " +
                                    $"(курс: {SelectedCurrency.Rate} грн / {SelectedCurrency.Units} {SelectedCurrency.Code})";
                }
                else
                {
                    ForeignAmountText = string.Empty;
                    StatusMessage = string.Empty;
                }
            }
            finally
            {
                _isUpdating = false;
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected void OnPropertyChanged(string name) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}