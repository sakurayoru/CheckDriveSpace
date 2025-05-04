using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

class Program
{
    private static BotConfig _config;
    private static readonly HttpClient _httpClient = new HttpClient();

    static async Task Main(string[] args)
    {
        // 設定ファイルの読み込み
        LoadConfig();

        // 定期的にドライブ容量をチェック
        while (true)
        {
            CheckDriveSpace();
            await Task.Delay(TimeSpan.FromMinutes(_config.CheckMin)); // config指定時間分ごとにチェック
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
            SendDiscordWebhook($"<@{_config.NumberUID}> 警告: __**{Dns.GetHostName()}**__に搭載されている__**{_config.DriveLetter}ドライブ**__の空き容量が __**{freeSpaceGB}GB**__ です！");
        }
    }

    static async void SendDiscordWebhook(string message)
    {
        var payload = new
        {
            content = message
        };

        string jsonPayload = JsonConvert.SerializeObject(payload);
        var content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");

        try
        {
            await _httpClient.PostAsync(_config.WebhookUrl, content);
            Console.WriteLine($"{DateTime.Now} 通知を送ります");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Webhook送信エラー: {ex.Message}");
        }
    }
}

class BotConfig
{
    public string WebhookUrl { get; set; }
    public ulong NumberUID { get; set; }
    public string DriveLetter { get; set; }
    public long ThresholdGB { get; set; }
    public double CheckMin { get; set; }
}
