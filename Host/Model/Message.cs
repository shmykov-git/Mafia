using Mafia.Extensions;

namespace Host.Model;

public class Message
{
    private string _text;
    private string[] _textLines;

    public required string Name { get; set; }
    public string Text { get => _text; set { _text = value; } }
    public string[] TextLines { get => _textLines; set { _textLines = value; if(_textLines.Length > 0 && !_text.HasText()) _text = value.SJoin(" "); } }
}
