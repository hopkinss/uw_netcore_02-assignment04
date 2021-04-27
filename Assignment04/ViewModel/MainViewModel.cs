using Assignment04.Data;
using Assignment04.Model;
using System.Collections.Generic;
using System.Threading.Tasks;
using static Assignment04.Model.ZipCode;
using Prism.Commands;
using System.Windows.Input;
using System;
using System.Text.RegularExpressions;
using System.Collections.ObjectModel;
using System.Linq;

namespace Assignment04.ViewModel
{
    public class MainViewModel : NotifyDataErrorInfoBase
    {
        private IZipDataService _zipDataService;
        private string zip;
        private string state;
        private string city;
        private bool isWorking;
        private string notification;
        private bool isValidZip;
        private Country countryCode;       
        private ObservableCollection<ZipCode> zipCodes;
        private ObservableCollection<ZipCode> selectedStates;
        private ObservableCollection<ZipCode> selectedCities;

        public MainViewModel(IZipDataService zipDataService)
        {
            _zipDataService = zipDataService;
            SubmitCommand = new DelegateCommand(OnSubmitExecute, CanSubmitExecute);
            SelectedCities = new ObservableCollection<ZipCode>();
            SelectedStates = new ObservableCollection<ZipCode>();
            State = "Unknown";
            City = "Unknown";
            AddError("Zip", "Zip code is required");
        }

        public bool IsWorking
        {
            get { return isWorking; }
            set
            {
                isWorking = value;
                OnPropertyChanged();
            }
        }
        public string Notification
        {
            get { return notification; }
            set
            {
                notification = value;
                OnPropertyChanged();
            }
        }

        public ObservableCollection<ZipCode> ZipCodes
        {
            get => zipCodes;
            set
            {
                zipCodes = value;
                OnPropertyChanged();
            }
        }
        public ObservableCollection<ZipCode> SelectedStates
        {
            get { return selectedStates; }
            set
            {
                selectedStates = value;
                OnPropertyChanged();
            }
        }
        public bool IsValidZip
        {
            get { return isValidZip; }
            set
            {
                isValidZip = value;
                OnPropertyChanged();
            }
        }
        public ObservableCollection<ZipCode> SelectedCities
        {
            get { return selectedCities; }
            set
            {
                selectedCities = value;
                OnPropertyChanged();
            }
        }


        public Country CountryCode
        {
            get { return countryCode; }
            set
            {
                countryCode = value;
                OnPropertyChanged();
            }
        }
        public string City
        {
            get { return city; }
            set
            {
                city = value;
                OnPropertyChanged();
            }
        }
        public string State
        {
            get { return state; }
            set
            {
                state = value;
                OnPropertyChanged();
            }
        }
        public string Zip
        {
            get { return zip; }
            set
            {
                zip = value;
                OnPropertyChanged();
                TryGetLocation();
                ValidateProperty("Zip");
                ((DelegateCommand)SubmitCommand).RaiseCanExecuteChanged();
            }
        }

        public ICommand SubmitCommand { get; }


        private void OnSubmitExecute()
        {
            Notification = $"Message sent to {City}, {State} in {CountryCode}" ;
        }

        private bool CanSubmitExecute()
        {
            return !HasErrors;
        }

        public async Task LoadZipAsync()
        {
            var zips = await _zipDataService.GetAllZipAsync();
            ZipCodes = new ObservableCollection<ZipCode>(zips);
        }

        public void ValidateProperty(string propertyName)
        {
            ClearErrors(propertyName);
            switch (propertyName)
            {
                case nameof(Zip):
                    if (string.IsNullOrEmpty(Zip))
                    {
                        AddError(propertyName, "Zip is required");
                        return;
                    }

                    if (CountryCode > 0)
                    {
                        if (CountryCode == Country.Canada)
                        {
                            if (Zip.Length < 7)
                            {
                                AddError(propertyName, $"Canadian zip codes must have 7 characters in the format 'A1A 1A1'");
                            }
                            else
                            {
                                if (!Regex.IsMatch(Zip, @"^([ABCEGHJKLMNPRSTVXY]\d[ABCEGHJKLMNPRSTVWXYZ])\ {0,1}(\d[ABCEGHJKLMNPRSTVWXYZ]\d)$"))
                                {
                                    AddError(propertyName, $"Invalid format. Specify Canadian zip codes in format: 'A1A 1A1'");
                                }
                            }

                        }
                        else if (CountryCode == Country.US)
                        {
                            if (Zip.Length < 5)
                            {
                                AddError(propertyName, $"US zip codes must have least 5 digits");
                            }
                            else
                            {
                                if (!Regex.IsMatch(Zip, @"^\d{5}(?:[-\s]\d{4})?$"))
                                {
                                    AddError(propertyName, $"Invalid format for US zip code");
                                }
                            }
                        }
                    }
                    break;
            }
        }

        private async void TryGetLocation()
        {
            IsValidZip = false;
            // Set the value of country
            if (Regex.IsMatch(Zip, "^[A-Z]", RegexOptions.IgnoreCase))
            {
                CountryCode = Country.Canada;
            }
            else if (Regex.IsMatch(Zip, "^[0-9]"))
            {
                CountryCode = Country.US;
            }
            else
                CountryCode = Country.Unknown;

            if (CountryCode > 0)
            {
                // Lookup State/Provice on Zip changed
                int keyLen = CountryCode == Country.US ? 5 : 6;
                var key = Zip.Length <= keyLen ? Zip : Zip.Substring(0, keyLen);

                if (CountryCode == Country.US || (CountryCode == Country.Canada && key.Length >= 3))
                {
                    IsWorking = true;
                    var matches = await GetMatchesAsync(key);
                    SelectedStates = await GetZipByStateAsync(State, matches);

                    // If there only 1 matching state display possible cities
                    if (matches.Count() == 1)
                    {
                        State = matches[0].State;
                        SelectedCities = await GetZipByCityAsync(City, matches);

                        if (matches[0].Cities.Count() == 1)
                        {
                            City = matches[0].Cities[0];
                            IsValidZip = true;
                        }
                        else
                        {
                            City = "Unknown";
                        }
                    }
                    else if (matches.Count() > 1)
                    {
                        SelectedCities.Clear();
                    }
                    else
                    {
                        State = "Unknown";
                        City = "Unknown";
                        SelectedCities.Clear();
                    }
                    IsWorking = false;
                }
                else
                {
                    SelectedCities.Clear();
                    SelectedStates.Clear();
                }
            }
            else
            {
                SelectedCities.Clear();
                SelectedStates.Clear();
                State = "Unknown";
            }
        }

        // Group cities within each state and province for selected country
        private async Task<List<ZipCode>> GetMatchesAsync(string key)
        {
            return await Task.Run(() =>
            {
                return ZipCodes.Where(x => x.Zip.StartsWith(key))
                        .GroupBy(x => x.State, x => x.City, (key, g) => new ZipCode { State = key, Cities = g.Distinct().ToList() }).ToList();
            });
        }

        private async Task<ObservableCollection<ZipCode>> GetZipByStateAsync(string state, List<ZipCode> matches)
        {
            return await Task.Run(() =>
            {
                return new ObservableCollection<ZipCode>(matches.Select(x => new ZipCode
                {
                    State = x.State
                }));
            });
        }
        private async Task<ObservableCollection<ZipCode>> GetZipByCityAsync(string city, List<ZipCode> matches)
        {
            return await Task.Run(() =>
            {
                return new ObservableCollection<ZipCode>(matches[0].Cities.Select(x => new ZipCode
                {
                    City = x,
                }));
            });
        }
    }
}
