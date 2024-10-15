namespace Gibbon.Git.Server.Security;

public static class Roles
{
    public const string Admin = "Administrator";
    public const string Member = "Member";
}

public static class Policies {
    public const string RepositoryAdmin = "RepositoryAdminPolicy";
    public const string RepositoryPush = "RepositoryPushPolicy";
}
