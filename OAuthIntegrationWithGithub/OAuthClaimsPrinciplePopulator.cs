using System.Net.Http.Headers;
using System.Text.Json;
using Microsoft.AspNetCore.Authentication.OAuth;

namespace OAuthIntegrationWithGithub;

public static class OAuthClaimsPrinciplePopulator

{
    public static async Task OnCreatingTicket(
        OAuthCreatingTicketContext context)
    {
        var request = new HttpRequestMessage(HttpMethod.Get, context.Options.UserInformationEndpoint);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", context.AccessToken);
        using var response = await context.Backchannel.SendAsync(request, CancellationToken.None);
        
        var user = await response.Content.ReadFromJsonAsync<JsonElement>();
        
        context.RunClaimActions(user);
    }
}