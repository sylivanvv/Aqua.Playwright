using Aqua.Automation.LearnQa.Models.Capybara;

namespace Aqua.Automation.LearnQa.CapybaraApi;

public interface IApiClient
{
    //Task<T> GetJsonAsync<T>(string url);
    Task<CapybaraModel> CreateCapybara(CapybaraModel capybara);
    Task<FriendshipResponseDto> AddFriend(string capybaraId, string friendId);
    Task<FriendsListDto> GetFriends(string capybaraId);

    Task<CapybaraModel> UpdateCapybara(CapybaraModel capybara);
    Task DeleteCapybara(string capybaraId);

}