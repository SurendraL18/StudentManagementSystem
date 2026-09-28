using System.Net;
using Microsoft.AspNetCore.Mvc;

namespace StudentManagement.IntegrationTests.API
{
    public class UsersApiTests : IClassFixture<CustomWebApplicationFactory>
    {
        private readonly HttpClient _client;

        // Explicit concrete constructor method structure
        public UsersApiTests(CustomWebApplicationFactory factory)
        {
            _client = factory.CreateClient();
        }

        [Fact]
        public async Task CreateUser_ShouldReturn409ConflictWithProblemDetails_WhenEmailIsDuplicate()
        {
            // Arrange
            var duplicateEmail = $"api-duplicate.{Guid.NewGuid()}@school.com";
            var requestPayload = new
            {
                Email = duplicateEmail,
                Password = "SecurePassword123!",
                FirstName = "API",
                LastName = "Test"
            };

            // Act 1: Initial creation
            var firstResponse = await _client.PostAsJsonAsync("/api/Users", requestPayload);
            firstResponse.EnsureSuccessStatusCode();

            // Act 2: Force duplicate error response pipeline conversion
            var secondResponse = await _client.PostAsJsonAsync("/api/Users", requestPayload);

            // Assert
            Assert.Equal(HttpStatusCode.Conflict, secondResponse.StatusCode);
            Assert.Equal("application/problem+json", secondResponse.Content.Headers.ContentType?.MediaType);

            var problem = await secondResponse.Content.ReadFromJsonAsync<ProblemDetails>();

            Assert.NotNull(problem);
            Assert.Equal("/problems/duplicate-user-email", problem.Type);
            Assert.Equal((int)HttpStatusCode.Conflict, problem.Status);
        }
    }
}
