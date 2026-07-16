using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using ZodiacTower.Core.Battle;
using ZodiacTower.Core.Game;
using ZodiacTower.Core.Generation;
using ZodiacTower.Web;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });
builder.Services.AddSingleton<IUnitGenerator, UnitGenerator>();
builder.Services.AddSingleton<BattleService>();
builder.Services.AddSingleton<CardGameService>();

await builder.Build().RunAsync();
