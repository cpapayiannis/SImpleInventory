namespace SimpleInventory.Authorization.Intefaces
{
    public interface IAuthorizationClient
    {
        string GenerateToken(string username);
    }
}
