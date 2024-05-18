using Mafia.Model;

namespace Mafia.Executions;

public delegate Task<DailyNews> CityOperation(State state, CityAction action);

public static class CityOperations
{
    public static async Task<DailyNews> CityKill(State state, CityAction action)
    {
        var dailyNews = new DailyNews
        {
            Selects = [new Select { Operation = nameof(CityKill), Who = null!, UserWhom = await state.Host.AskCityToSelect(state, action, nameof(CityKill)) }]
        };

        dailyNews.DoKnowAllWhom(state);

        return dailyNews;
    }

    public static async Task<DailyNews> CityImmunity(State state, CityAction action)
    {
        var dailyNews = new DailyNews
        {
            Selects = [new Select { Operation = nameof(CityImmunity), Who = null!, UserWhom = await state.Host.AskCityToSelect(state, action, nameof(CityImmunity)) }]
        };

        dailyNews.DoKnowAllWhom(state);

        return dailyNews;
    }
    
}
