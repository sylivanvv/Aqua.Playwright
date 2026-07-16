namespace Aqua.Automation.AuthHelpers;

[AttributeUsage(AttributeTargets.Class)]
public class WithAuthAttribute(AuthRole role) : Attribute
{
    public AuthRole Role { get; } = role;
}

public enum AuthRole
{
    LearnQaUser,
    QaBrainsUser
}