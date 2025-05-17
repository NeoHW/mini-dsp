using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using DSP.Api;
using SSP.Api;
using Shared.Models;
using Shared.Utilities;

using System;
using System.Collections.Generic;


public class Program
{
    public static void Main(string[] args)
    {
        var users = UserLoader.LoadFromFile("../Shared/users.json");
        
        var campaignsAlpha = CampaignLoader.LoadFromFile("../Shared/campaign_1.json");
        var campaignsBeta = CampaignLoader.LoadFromFile("../Shared/campaign_2.json");
        var campaignsGamma = CampaignLoader.LoadFromFile("../Shared/campaign_3.json");
        
        using var host = Host.CreateDefaultBuilder(args)
            .ConfigureServices((context, services) =>
            {
                services.AddSingleton<IUserStore>(sp => new UserStore(users));
                
                services.AddSingleton<Ssp>();
                
                services.AddSingleton(sp => new Dsp("Alpha", sp.GetRequiredService<IUserStore>(), campaignsAlpha));
                services.AddSingleton(sp => new Dsp("Beta", sp.GetRequiredService<IUserStore>(), campaignsBeta));
                services.AddSingleton(sp => new Dsp("Gamma", sp.GetRequiredService<IUserStore>(), campaignsGamma));
            })
            .Build();

        var ssp = host.Services.GetRequiredService<Ssp>();
        var dsps = host.Services.GetServices<Dsp>();

        foreach (var dsp in dsps)
        {
            dsp.SubscribeTo(ssp);
        }

        Console.WriteLine("=== DSP-SSP Simulation Started ===");
        Console.WriteLine("=== Press Enter to stop simulation ===");
        Console.ReadLine();
    }
}