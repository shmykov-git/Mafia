namespace Mafia.Model;

public interface IRating
{
    (string nick, int rating, RatingCase[] cases)[] GetRatings();
}
