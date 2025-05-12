
 using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Oracle.ManagedDataAccess.Client;
using RemediEmr.Repositry;
using RemediEmr.Data.Class;

//public class TokenUpdateService : BackgroundService
//    {
//        private readonly IServiceScopeFactory _serviceScopeFactory;
//        private List<TokenInfo> _previousTokens = new List<TokenInfo>(); // Store previous results

//        public TokenUpdateService(IServiceScopeFactory serviceScopeFactory)
//        {
//            _serviceScopeFactory = serviceScopeFactory;
//        }


//    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
//    {
//        while (!stoppingToken.IsCancellationRequested)
//        {
//            try
//            {
//                using (var scope = _serviceScopeFactory.CreateScope())
//                {
//                    var tokenService = scope.ServiceProvider.GetRequiredService<TvTokenRepositry>();

//                    var connectedDevices = WebSocketHandler.GetConnectedDeviceIds(); // Get active devices

//                    foreach (var deviceId in connectedDevices)
//                    {
//                        var tokens = await tokenService.GetTokenDetailsForMonitoring(deviceId);

//                        if (HasDataChanged(_previousTokens, tokens))
//                        {
//                            _previousTokens = new List<TokenInfo>(tokens);
//                            await WebSocketHandler.NotifyClients(tokens);
//                        }
//                    }
//                }
//            }
//            catch (Exception ex)
//            {
//                Console.WriteLine($"Error checking token updates: {ex.Message}");
//            }

//            await Task.Delay(8000, stoppingToken);
//        }
//    }
//        //protected override async Task ExecuteAsync(CancellationToken stoppingToken)
//        //{
//        //    while (!stoppingToken.IsCancellationRequested)
//        //    {
//        //        try
//        //        {
//        //            using (var scope = _serviceScopeFactory.CreateScope())
//        //            {
//        //                var tokenService = scope.ServiceProvider.GetRequiredService<TvTokenRepositry>();

//        //                // Fetch latest data from the database
//        //                var tokens = await tokenService.GetTokenDetailsForMonitoring();

//        //                // Check if data has changed
//        //                if (HasDataChanged(_previousTokens, tokens))
//        //                {
//        //                    _previousTokens = new List<TokenInfo>(tokens); // Update cache
//        //                    await WebSocketHandler.NotifyClients(tokens); // Send updated data to clients
//        //                }
//        //            }
//        //        }
//        //        catch (Exception ex)
//        //        {
//        //            Console.WriteLine($"Error checking token updates: {ex.Message}");
//        //        }

//        //        await Task.Delay(5000, stoppingToken); // Check for changes every 5 seconds
//        //    }
//        //}

//        private bool HasDataChanged(List<TokenInfo> oldData, List<TokenInfo> newData)
//        {
//            return JsonConvert.SerializeObject(oldData) != JsonConvert.SerializeObject(newData);
//        }
//    }

public class TokenUpdateService : BackgroundService
{
    private readonly IServiceScopeFactory _serviceScopeFactory;
    private readonly Dictionary<string, List<TokenInfo>> _previousTokens = new(); // Store previous tokens per device

    public TokenUpdateService(IServiceScopeFactory serviceScopeFactory)
    {
        _serviceScopeFactory = serviceScopeFactory;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using (var scope = _serviceScopeFactory.CreateScope())
                {
                    var tokenService = scope.ServiceProvider.GetRequiredService<TvTokenRepositry>();
                    var connectedDevices = WebSocketHandler.GetConnectedDeviceIds(); // Get active devices

                    foreach (var deviceId in connectedDevices)
                    {
                        var tokens = await tokenService.GetTokenDetailsForMonitoring(deviceId);

                        if (!_previousTokens.TryGetValue(deviceId, out var oldTokens) || HasDataChanged(oldTokens, tokens))
                        {
                            _previousTokens[deviceId] = new List<TokenInfo>(tokens); // Update cache
                            await WebSocketHandler.NotifyClients( tokens); // Notify only relevant clients
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[TokenUpdateService] Error: {ex}");
            }

            await Task.Delay(8000, stoppingToken);
        }
    }

    private bool HasDataChanged(List<TokenInfo> oldData, List<TokenInfo> newData)
    {
        // Prevent sending empty updates
        if (newData == null || newData.Count == 0) return false;

        return JsonConvert.SerializeObject(oldData) != JsonConvert.SerializeObject(newData);
    }

}

// Custom comparer to avoid full object serialization
public class TokenInfoComparer : IEqualityComparer<TokenInfo>
{
    public bool Equals(TokenInfo x, TokenInfo y)
    {
        return x.PatientOpNo == y.PatientOpNo && x.TokenReadVoiceStatus == y.TokenReadVoiceStatus; // Compare only relevant properties
    }

    public int GetHashCode(TokenInfo obj)
    {
        return HashCode.Combine(obj.PatientOpNo, obj.TokenReadVoiceStatus);
    }
}



public class DoctorTokenInfoComparer : IEqualityComparer<DoctorTokenInFo>
{
    public bool Equals(DoctorTokenInFo x, DoctorTokenInFo y)
    {
        return x.OPNO == y.OPNO && x.TOKEN_READ_STS == y.TOKEN_READ_STS; // Compare only relevant properties
    }

    public int GetHashCode(DoctorTokenInFo obj)
    {
        return HashCode.Combine(obj.TOKEN_READ_STS, obj.TOKEN_READ_STS);
    }
}

public class DoctorTvTokenUpdateService : BackgroundService
{
    private readonly IServiceScopeFactory _serviceScopeFactory;
    private readonly Dictionary<string, List<DoctorTokenInFo>> _previousTokens = new(); // Store previous tokens per device

    public DoctorTvTokenUpdateService(IServiceScopeFactory serviceScopeFactory)
    {
        _serviceScopeFactory = serviceScopeFactory;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using (var scope = _serviceScopeFactory.CreateScope())
                {
                    var tokenService = scope.ServiceProvider.GetRequiredService<DoctorTvTokenRepositry>();
                    var connectedDevices = WebSocketHandlerDoctorToken.GetConnectedDeviceIds(); // Get active devices

                    foreach (var deviceId in connectedDevices)
                    {
                        var tokens = await tokenService.GetDoctorRoomInfoDataTableAsync(deviceId);


                        if (!_previousTokens.TryGetValue(deviceId, out var oldTokens) || HasDataChanged(oldTokens, tokens))
                        {
                            _previousTokens[deviceId] = new List<DoctorTokenInFo>(tokens); // Update cache
                            await WebSocketHandlerDoctorToken.NotifyClients(tokens); // Notify only relevant clients
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[TokenUpdateService] Error: {ex}");
            }

            await Task.Delay(8000, stoppingToken);
        }
    }

    private bool HasDataChanged(List<DoctorTokenInFo> oldData, List<DoctorTokenInFo> newData)
    {
        // Prevent sending empty updates
        if (newData == null || newData.Count == 0) return false;

        return JsonConvert.SerializeObject(oldData) != JsonConvert.SerializeObject(newData);
    }

}


