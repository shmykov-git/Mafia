namespace Host.Model;

public class ActiveLang: NotifyPropertyChanged
{
    private bool _isChecked;

    public string Name { get; set; }
    public string FullName { get; set; }
    public bool IsChecked { get => _isChecked; set { IfChanged(ref _isChecked, value); } }
}
