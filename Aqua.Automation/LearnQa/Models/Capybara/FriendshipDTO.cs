namespace Aqua.Automation.LearnQa.Models.Capybara;

public record FriendshipResponseDto(string Message,FriendshipDto Friendship);

public record FriendshipDto(
    CapybaraShortDto Capybara,
    CapybaraShortDto Friend,
    DateTime Since
);

public record CapybaraShortDto(
    string Id,
    string Name,
    string Mood
);