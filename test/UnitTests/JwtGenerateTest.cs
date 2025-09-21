using Microsoft.Extensions.Options;
using SimpleInventory.Authorization.Classes;
using SimpleInventory.Authorization.Intefaces;
using SimpleInventory.Common.Classes;

namespace UnitTests
{
    [TestClass]
    public class JwtTests
    {
        private readonly IAuthorizationClient _authClient;

        public JwtTests()
        {
            // Setup JwtSettings
            var jwtSettings = Options.Create(new JwtSettings
            {
                Key = "ThisIsASecureSuperLongJwtKey12345"
            });

            _authClient = new AuthorizationClient(jwtSettings);
        }

        [TestMethod]
        public void GenerateToken_ShouldReturn_ValidJwt()
        {
            // Arrange
            var username = "test-user";

            // Act
            var token = _authClient.GenerateToken(username);

            // Assert
            Assert.IsFalse(string.IsNullOrWhiteSpace(token));
            StringAssert.StartsWith(token, "eyJ"); // basic check for JWT structure

            Console.WriteLine("Generated JWT: " + token);
        }
    }
}
