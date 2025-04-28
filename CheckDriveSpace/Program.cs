using System;
using System.Net;
using System.IO;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Discord;
using Discord.WebSocket;

class Program
{
    private static DiscordSocketClient _client;
    private static BotConfig _config;

    static async Task Main(string[] args)
    {
        // 設定ファイルの読み込み
        LoadConfig();

        // Discordクライアントの初期化
        _client = new DiscordSocketClient();
        _client.Log += LogAsync;

        await _client.LoginAsync(TokenType.Bot, _config.Token);
        await _client.StartAsync();

        // 定期的にドライブ容量をチェック
        while (true)
        {
            CheckDriveSpace();
            await Task.Delay(TimeSpan.FromMinutes(_config.CheckMin)); // 10分ごとにチェック
        }
    }

    static void LoadConfig()
    {
        string json = File.ReadAllText("00-config.json");
        _config = JsonConvert.DeserializeObject<BotConfig>(json);
    }

    static void CheckDriveSpace()
    {
        DriveInfo drive = new DriveInfo(_config.DriveLetter);
        long freeSpaceGB = drive.AvailableFreeSpace / (1024 * 1024 * 1024);

        if (freeSpaceGB < _config.ThresholdGB)
        {
            SendDiscordNotification($"<@{_config.NumberUID}> 警告: __**{Dns.GetHostName()}**__に搭載されている__**{_config.DriveLetter}ドライブ**__の空き容量が __**{freeSpaceGB}GB**__ です！");
        }
    }

    static async void SendDiscordNotification(string message)
    {
        var channel = _client.GetChannel(_config.ChannelId) as IMessageChannel;
        if (channel != null)
        {
            await channel.SendMessageAsync(message);
        }
    }

    static Task LogAsync(LogMessage log)
    {
        Console.WriteLine(log);
        return Task.CompletedTask;
    }
}

class BotConfig
{
    public string Token { get; set; }
    public ulong ChannelId { get; set; }
    public ulong NumberUID { get; set; }
    public string DriveLetter { get; set; }
    public long ThresholdGB { get; set; }
    public double CheckMin { get; set; }
}
