using CommunityToolkit.Mvvm.Messaging.Messages;
using MrCapitalQ.AdrenalineGamesEditor.Core.Apps;

namespace MrCapitalQ.AdrenalineGamesEditor.Apps;

internal class PickPackagedAppRequestMessage : AsyncRequestMessage<PackagedAppListing?>;
