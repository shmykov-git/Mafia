namespace Mafia.Model;

public class DefaultCityFactory : ICity
{
    public City City { get; }

    public DefaultCityFactory(City city)
    {
        City = city;
    }
}