using System.Net.Http.Headers;

namespace FileAccessApp.Services
{
    public class AuthService
    {
        private readonly HttpClient _http;
        public string? Token { get; private set; }

        public event Func<Task>? OnChange;

        public AuthService(HttpClient http)
        {
            _http = http;
        }

        public async Task SetToken(string token)
        {
            if (await ValidToken(token))
            {
                Token = token;
                await NotifyStateChanged();
            }
        }

        public async Task ClearToken()
        {
            Token = null;
            await NotifyStateChanged();
        }

        private async Task NotifyStateChanged()
        {
            if (OnChange != null)
                await OnChange.Invoke();
        }

        public async Task<bool> ValidToken(string token)
        {
            if (string.IsNullOrEmpty(token))
                return false;

            var request = new HttpRequestMessage(HttpMethod.Get, "api/auth/secure");
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

            try
            {
                var response = await _http.SendAsync(request);
                return response.IsSuccessStatusCode;
            }
            catch
            {
                return false;
            }
        }
    }
}
