namespace Gamestore.Services.Constants;

public static class NotificationConstants
{
    public const string SMS = "sms";
    public const string PUSH = "push";
    public const string EMAIL = "email";

    public static readonly List<string> AvailableMethods = new()
    {
        SMS,
        PUSH,
        EMAIL,
    };
}