using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Remora.Discord.API.Abstractions.Gateway.Events;
using Remora.Discord.API.Abstractions.Rest;
using Remora.Discord.Gateway.Responders;
using Remora.Rest.Core;
using Remora.Results;
using static AcegikmoDiscordBot.Program;

namespace AcegikmoDiscordBot
{
    internal class PronounCommand: IResponder<IMessageCreate>
    {
        private static readonly ulong SheHer = 920793126266880001;
        private static readonly ulong HeHim = 920793126266880001;
        private static readonly ulong TheyThem = 920793126266880001;
        private static readonly ulong HimHerThey = 920793126266880001;
        private static readonly ulong FaeFaer = 920793126266880001;

        private readonly IDiscordRestChannelAPI _channelApi;
        private readonly IDiscordRestGuildAPI _guildApi;
        private readonly GamesCommand _gamesCommand;

        public PronounCommand(IDiscordRestChannelAPI channelApi, IDiscordRestGuildAPI guildApi, GamesCommand gamesCommand) {
            _channelApi = channelApi;
            _guildApi = guildApi;
            _gamesCommand = gamesCommand;
        }

        public async Task<Result> RespondAsync(IMessageCreate message, CancellationToken ct = new())
        {
            if (message.Content == "!pronoun" && message.GuildID.IsAcegikmo()) {
                await _channelApi.CreateMessageAsync(message.ChannelID,
                    "Available pronouns: she/her, he/him, they/them, him/her/they, fae/faer. Example:\n!pronoun she/her");
            }
            if (message.Content.StartsWith("!pronoun ") && message.GuildID.IsAcegikmo())
            {
                var role = message.Content["!pronoun ".Length..].ToLower();
                ulong? idNull = role switch
                {
                    "she/her" => SheHer,
                    "he/him" => HeHim,
                    "they/them" => TheyThem,
                    "him/her/they" => HimHerThey,
                    "fae/faer" => FaeFaer,
                    _ => null,
                };
                if (idNull != null) {
                    var member = await _guildApi.GetGuildMemberAsync(message.GuildID.Value, message.Author.ID);
                    var id = idNull.Value;
                    if (member.Entity.Roles.Contains(new Snowflake(id)))
                    {
                        await _guildApi.RemoveGuildMemberRoleAsync(message.GuildID.Value, message.Author.ID, new Snowflake(id));
                        await _gamesCommand.CrossReact(message);
                    }
                    else
                    {
                        await _guildApi.AddGuildMemberRoleAsync(message.GuildID.Value, message.Author.ID, new Snowflake(id));
                        await _gamesCommand.Checkmark(message);
                    }
                }
            }
            return Result.FromSuccess();
        }
    }
}
