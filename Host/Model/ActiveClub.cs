namespace Host.Model;

public class ActiveClub : NotifyPropertyChanged
{
    private bool _isChecked;

    public string Name { get; set; }
    public bool IsChecked { get => _isChecked; set { IfChanged(ref _isChecked, value); } }
}