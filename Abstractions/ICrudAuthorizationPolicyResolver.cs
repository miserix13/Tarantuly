namespace Tarantuly.Abstractions;

public interface ICrudAuthorizationPolicyResolver
{
    string GetListPolicy(Type entityType);

    string GetCreatePolicy(Type entityType);

    string GetEditPolicy(Type entityType);

    string GetDeletePolicy(Type entityType);

    string GetRestorePolicy(Type entityType);
}
