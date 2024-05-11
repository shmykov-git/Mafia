using Mafia.Extensions;
using Mafia.Model;

namespace Host.Model;

public class InteractionResult
{
    public ActivePlayer[] Selected { get; set; } = [];
    public User[] SelectedUsers => Selected.Select(p => p.User).ToArray();
    public Player[] SelectedPlayers
    {
        get
        {
            //Selected.ForEach(p =>
            //{
            //    if (p.Player == null)
            //        throw new ArgumentNullException(nameof(p.Player));
            //});

            return Selected.Select(p => p.Player!).ToArray();
        }
    }

    public bool IsSkipped => Selected.Length == 0;
}