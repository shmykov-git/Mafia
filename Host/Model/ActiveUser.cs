﻿using System.Windows.Input;
using Host.ViewModel;
using Mafia.Model;

namespace Host.Model;

public class ActiveUser : NotifyPropertyChanged
{
    private readonly Action<string, ActiveUser> onChange;

    public ActiveUser(User user, Action<string, ActiveUser> onChange, string propertyName)
    {
        User = user;
        this.onChange = onChange;
        Subscribe((name, obj) => onChange(name, (ActiveUser)obj), propertyName);
    }

    public ActiveUser Self => this;

    public bool IsButtonsVisibleSilent;
    public bool IsButtonsVisible { get => IsButtonsVisibleSilent; set { IsButtonsVisibleSilent = value; Changed(); } }

    public void RefreshButtons(bool isVisible) 
    {
        IsButtonsVisible = isVisible;
    }

    public ICommand CommandUp => new Command(arg => onChange("Up", (ActiveUser)arg));
    public ICommand CommandDown => new Command(arg => onChange("Down", (ActiveUser)arg));

    public Color NickColorSilent { get; set; } = Colors.Black;
    public Color NickColor { get => NickColorSilent; set { NickColorSilent = value; Changed(); } }

    public User User { get; }

    public string Nick { get => User.Nick; set { User.Nick = value; Changed(); } }

    public bool IsSelectedSilent { get => User.IsSelected; set => User.IsSelected = value; }
    public bool IsSelected { get => IsSelectedSilent; set => IfChanged(v => IsSelectedSilent = v, IsSelectedSilent, value); }
    
}
