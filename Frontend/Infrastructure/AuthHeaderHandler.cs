using System.Net.Http.Headers;

namespace Frontend.Infrastructure
{
    public class AuthHeaderHandler : DelegatingHandler
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public AuthHeaderHandler(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        protected override async Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            var session = _httpContextAccessor.HttpContext?.Session;

            var token =
                session?.GetString("JwtToken") ??
                session?.GetString("AccessToken") ??
                session?.GetString("Token") ??
                session?.GetString("AuthToken") ??
                session?.GetString("BearerToken");

            if (!string.IsNullOrWhiteSpace(token))
            {
                if (token.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
                {
                    token = token["Bearer ".Length..];
                }

                request.Headers.Authorization =
                    new AuthenticationHeaderValue("Bearer", token);
            }

            return await base.SendAsync(request, cancellationToken);
        }
    }
}