using Newtonsoft.Json;

namespace bb_ne_budet;

public class MatchInfo
{
    public int MatchId { get; set; }
    public string Team1 { get; set; }
    public string Team2 { get; set; }
    public string League { get; set; }
    public DateTime MatchTime { get; set; }
    public string MatchUrl { get; set; }
    public Dictionary<string, double> Coefficients { get; set; }  
    public string ToJson() => JsonConvert.SerializeObject(this);
}
