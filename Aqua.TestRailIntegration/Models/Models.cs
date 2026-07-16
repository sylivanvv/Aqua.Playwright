using System.Text.Json.Serialization;

namespace Aqua.TestRailIntegration.Models;

public record AddResultRequest(
    [property: JsonPropertyName("status_id")] int StatusId, 
    [property: JsonPropertyName("comment")] string Comment);

public record TestRailStep(
    [property: JsonPropertyName("content")] string Content, 
    [property: JsonPropertyName("expected")] string Expected);

public record UpdateCaseRequest(
    [property: JsonPropertyName("title")] string Title, 
    [property: JsonPropertyName("custom_steps_separated")] List<TestRailStep> CustomStepsSeparated);