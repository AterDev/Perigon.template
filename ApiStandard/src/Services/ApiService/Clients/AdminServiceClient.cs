namespace ApiService.Clients;

public sealed class AdminServiceClient(HttpClient httpClient)
{
    public async Task<string> GetInfoAsync(CancellationToken cancellationToken = default)
    {
        return await httpClient.GetStringAsync("api/Default/info", cancellationToken);
    }
}
