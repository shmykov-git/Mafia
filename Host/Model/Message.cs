using Mafia.Extensions;

namespace Host.Model;

public class Message
{
    private string _text;

    public required string Name { get; set; }
    public string Text { get => _text; set { _text = value; } }
    public string[]? TextLines { set { if (value?.Length > 0 && _text == null) _text = value.SJoin(" "); } }
}
