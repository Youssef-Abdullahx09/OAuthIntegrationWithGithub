using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using OAuthIntegrationWithGithub;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddAuthentication("cookie")
    .AddCookie("cookie")
    .AddOAuth(builder.Configuration.GetValue<string>("OAuth:Schema")!, options =>
    {
        options.SignInScheme = "cookie";
        
        options.ClientId = builder.Configuration.GetValue<string>("OAuth:ClientId")!;
        options.ClientSecret = builder.Configuration.GetValue<string>("OAuth:ClientSecret")!;
        options.AuthorizationEndpoint = builder.Configuration.GetValue<string>("OAuth:AuthorizationEndpoint")!;
        options.TokenEndpoint = builder.Configuration.GetValue<string>("OAuth:TokenEndpoint")!;
        options.UserInformationEndpoint = builder.Configuration.GetValue<string>("OAuth:UserInformationEndpoint")!;
        options.CallbackPath = builder.Configuration.GetValue<string>("OAuth:CallbackPath")!;
        options.SaveTokens = true;
        options.Events.OnCreatingTicket = OAuthClaimsPrinciplePopulator.OnCreatingTicket;
        options.ClaimActions.MapJsonKey("sub", "id");
        options.ClaimActions.MapJsonKey(ClaimTypes.NameIdentifier, "login");
        
    });
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthentication();

app.MapGet("/", (HttpContext ctx) => ctx.User.Claims
    .Select(x => new
    {
        x.Type,
        x.Value
    })
    .ToList());

app.MapGet("/login", () =>
    Results.Challenge(
        new AuthenticationProperties
        {
            RedirectUri = "/",
        },
        authenticationSchemes: [builder.Configuration.GetValue<string>("OAuth:Schema")!]));

await app.RunAsync(CancellationToken.None);