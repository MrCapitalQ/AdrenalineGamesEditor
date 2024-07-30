using System.Text.Json.Serialization;

namespace MrCapitalQ.AdrenalineGamesEditor.Core.Adrenaline;

public class AdrenalineGamesDataModel
{
    [JsonPropertyName("games")]
    public IEnumerable<AdrenalineGameModel> Games { get; init; } = [];
}
