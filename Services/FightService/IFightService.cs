using dotnet_rpg.Dtos.Character;
using dotnet_rpg.Dtos.Fight;

namespace dotnet_rpg.Services.FightService
{
    public interface IFightService
    {
        Task<ServiceResponse<FightResultDto>> Fight(FightRequestDto request);
        List<string> DoFight(ref List<Fighter> fighters, FightSettingsDto settings);
        Task<ServiceResponse<List<GetCharacterDto>>> ClearFightResults(List<int> characterIds);
    }
}
