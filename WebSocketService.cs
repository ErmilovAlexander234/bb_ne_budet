using System.Collections.Concurrent;
using WebSocketSharp;
using WebSocketSharp.Server;

namespace bb_ne_budet;
public class WebSocketService : WebSocketBehavior
{
    private readonly ConcurrentDictionary<int, MatchInfo> _matches = new();

    protected override void OnMessage(MessageEventArgs e)
    {
        if (e.Data == "get_matches")
        {
            // отправка всех матчей 
            var allMatchesJson = _matches.Values.Select(match => match.ToJson()).ToArray();
            foreach (var matchJson in allMatchesJson)
            {
                Send(matchJson);
            }
        }
    }

    // нестатический метод для уведов
    public void NotifyAllClients(string message)
    {
        Sessions?.Sessions?.ToList().ForEach(session => session.Context.WebSocket.Send(message));
    }

    // нестатический метод для парсинга
    public async Task StartParsingAndUpdatingAsync()
    {
        var parser = new BetBoomParser();

        while (true)
        {
            try
            {
                var newMatches = await parser.ParseMatchesAsync();
                UpdateMatches(newMatches);

                foreach (var match in _matches.Values)
                {
                    var newCoefficients = await parser.ParseMatchCoefficientsAsync(match.MatchUrl);
                    if (!match.Coefficients.SequenceEqual(newCoefficients))
                    {
                        match.Coefficients = newCoefficients;
                        NotifyAllClients(match.ToJson());
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка парсинга: {ex.Message}");
            }
            await Task.Delay(20000);
        }
    }

    // нестатический метод для обновления списка матчей
    private void UpdateMatches(List<MatchInfo> newMatches)
    {
        // добавление новых матчей
        newMatches.ForEach(match =>
        {
            if (!_matches.ContainsKey(match.MatchId))
            {
                _matches[match.MatchId] = match;
                NotifyAllClients(match.ToJson());
            }
        });

        // удаление матчей
        var removedMatches = _matches.Keys
            .Where(matchId => newMatches.All(m => m.MatchId != matchId))
            .ToList();

        foreach (var matchId in removedMatches)
        {
            _matches.TryRemove(matchId, out _);
            NotifyAllClients($"{{\"event\": \"remove\", \"matchId\": {matchId}}}");
        }
    }
}
