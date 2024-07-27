using System.Text.Json.Serialization;

namespace MrCapitalQ.AdrenalineGamesEditor.Core.Adrenaline;

public class AdrenalineGameDataModel
{
    [JsonPropertyName("games")]
    public IEnumerable<AdrenalineGameModel> Games { get; init; } = [];
}
