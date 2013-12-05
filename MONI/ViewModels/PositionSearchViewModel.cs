﻿using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Input;
using MONI.Util;

namespace MONI.ViewModels {
  public class PositionSearchViewModel : ViewModelBase {
    private bool showPnSearch;
    private string searchText;
    private ICommand cancelCommand;

    public PositionSearchViewModel(string projectNumberFile) {
      this.Results = new QuickFillObservableCollection<PositionNumber>();
      this.ProjectNumbers = new List<PositionNumber>();
      Task.Factory.StartNew(() => this.ReadPNFile(projectNumberFile));
    }

    private void ReadPNFile(string pnFilePath) {
      if (!string.IsNullOrWhiteSpace(pnFilePath) && File.Exists(pnFilePath)) {
        var allPnLines = File.ReadAllLines(pnFilePath, Encoding.Default);
        foreach (string line in allPnLines.Skip(1)) {
          var pn = new PositionNumber();
          pn.Number = line.Substring(0, 5);
          pn.Description = line.Substring(14);
          this.ProjectNumbers.Add(pn);
        }
      }
    }

    protected List<PositionNumber> ProjectNumbers { get; set; }

    public bool ShowPNSearch {
      get { return this.showPnSearch; }
      set {
        this.showPnSearch = value;
        this.OnPropertyChanged(() => this.ShowPNSearch);
      }
    }

    public string SearchText {
      get { return this.searchText; }
      set {
        this.searchText = value;
        this.Search();
      }
    }

    private void Search() {
      this.Results.Clear();
      var s = this.searchText;
      if (!string.IsNullOrWhiteSpace(s)) {
        var res = this.ProjectNumbers.Where(pn => Regex.IsMatch(pn.Number, s, RegexOptions.IgnoreCase) || Regex.IsMatch(pn.Description, s, RegexOptions.IgnoreCase));
        this.Results.Fill(res);
      }
    }

    public QuickFillObservableCollection<PositionNumber> Results { get; private set; }

    public ICommand CancelCommand {
      get { return this.cancelCommand ?? (this.cancelCommand = new DelegateCommand(() => this.ShowPNSearch = false, () => true)); }
    }

    public string GetDescriptionForPositionNumber(string key) {
      //TODO
      return key;
    }
  }

  public class PositionNumber {
    public string Number { get; set; }
    public string Description { get; set; }
  }
}