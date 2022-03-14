using Victoria;
using Victoria.EventArgs;
using System.Collections.Concurrent;
using Discord;
using Victoria.Enums;
using Discord.Addons.Hosting;
using Discord.WebSocket;
using Discord.Addons.Hosting.Util;

namespace Marabis.Services
{
    public sealed class AudioService : DiscordClientService
    {
        private readonly LavaNode _lavaNode;
        private readonly ILogger _logger;
        public readonly HashSet<ulong> VoteQueue;
        private readonly ConcurrentDictionary<ulong, CancellationTokenSource> _disconnectTokens;

        public AudioService(DiscordSocketClient client, ILogger<AudioService> logger, LavaNode lavaNode, ILoggerFactory loggerFactory) : base(client, logger)
        {
            _lavaNode = lavaNode;
            _logger = loggerFactory.CreateLogger<LavaNode>();
            _disconnectTokens = new ConcurrentDictionary<ulong, CancellationTokenSource>();

            _lavaNode.OnLog += arg =>
            {
                _logger.Log(FromSeverityToLevel(arg), arg.Exception, arg.Message);
                return Task.CompletedTask;
            };

            _lavaNode.OnPlayerUpdated += OnPlayerUpdated;
            _lavaNode.OnStatsReceived += OnStatsReceived;
            _lavaNode.OnTrackEnded += OnTrackEnded;
            _lavaNode.OnTrackStarted += OnTrackStarted;
            _lavaNode.OnTrackException += OnTrackException;
            _lavaNode.OnTrackStuck += OnTrackStuck;
            _lavaNode.OnWebSocketClosed += OnWebSocketClosed;

            VoteQueue = new HashSet<ulong>();
        }

        public static LogLevel FromSeverityToLevel(LogMessage arg)
        {
            string logText = $": {arg.Exception?.ToString() ?? arg.Message}";
            LogLevel logLevel = LogLevel.None;
            switch (arg.Severity.ToString())
            {
                case "Critical":
                    {
                        logLevel = LogLevel.Critical;
                        break;
                    }
                case "Warning":
                    {
                        logLevel = LogLevel.Warning;
                        break;
                    }
                case "Info":
                    {
                        logLevel = LogLevel.Information;
                        break;
                    }
                case "Verbose":
                    {
                        logLevel = LogLevel.Information;
                        break;
                    }
                case "Debug":
                    {
                        logLevel = LogLevel.Debug;
                        break;
                    }
                case "Error":
                    {
                        logLevel = LogLevel.Error;
                        break;
                    }
            }

            return logLevel;
        }

        private Task OnPlayerUpdated(PlayerUpdateEventArgs arg)
        {
            //_logger.LogInformation($"Track updated : {arg.Track.Title} : {arg.Position}");
            return Task.CompletedTask;
        }

        private Task OnStatsReceived(StatsEventArgs arg)
        {
            _logger.LogInformation($"Music has been up for : {arg.Uptime}");
            return Task.CompletedTask;
        }

        private async Task OnTrackStarted(TrackStartEventArgs arg)
        {
            if (!_disconnectTokens.TryGetValue(arg.Player.VoiceChannel.Id, out var value))
            {
                return;
            }

            if (value.IsCancellationRequested)
            {
                return;
            }

            value.Cancel(true);
            await arg.Player.TextChannel.SendMessageAsync("Auto disconnect has been cancelled!");
        }

        private async Task OnTrackEnded(TrackEndedEventArgs args)
        {
            if (args.Reason == TrackEndReason.Finished)
            {
                return;
            }

            var player = args.Player;
            if (!player.Queue.TryDequeue(out var queueable))
            {
                await player.TextChannel.SendMessageAsync("Queue completed! Please add more tracks!");
                _ = InitiateDisconnectAsync(args.Player, TimeSpan.FromSeconds(10));
                return;
            }

            if (queueable is not LavaTrack track)
            {
                await player.TextChannel.SendMessageAsync("Next item in queue is not a track.");
                return;
            }

            await args.Player.PlayAsync(track);
            await args.Player.TextChannel.SendMessageAsync($"{args.Reason}: {args.Track.Title}\nNow playing: {track.Title}");
        }

        private async Task InitiateDisconnectAsync(LavaPlayer player, TimeSpan timeSpan)
        {
            if (!_disconnectTokens.TryGetValue(player.VoiceChannel.Id, out var value))
            {
                value = new CancellationTokenSource();
                _disconnectTokens.TryAdd(player.VoiceChannel.Id, value);
            }
            else if (value.IsCancellationRequested)
            {
                _disconnectTokens.TryUpdate(player.VoiceChannel.Id, new CancellationTokenSource(), value);
                value = _disconnectTokens[player.VoiceChannel.Id];
            }

            await player.TextChannel.SendMessageAsync($"Auto disconnect initiated! Disconnecting in {timeSpan}...");
            var isCancelled = SpinWait.SpinUntil(() => value.IsCancellationRequested, timeSpan);
            if (isCancelled)
            {
                return;
            }

            await _lavaNode.LeaveAsync(player.VoiceChannel);
            await player.TextChannel.SendMessageAsync("Bye, see ya next time.");
        }

        private async Task OnTrackException(TrackExceptionEventArgs arg)
        {
            _logger.LogError($"Track {arg.Track.Title} threw an exception. Please check Lavalink console/logs.");
            arg.Player.Queue.Enqueue(arg.Track);
            await arg.Player.TextChannel.SendMessageAsync($"{arg.Track.Title} has been re-added to queue after throwing an exception.");
        }

        private async Task OnTrackStuck(TrackStuckEventArgs arg)
        {
            _logger.LogError(
                $"Track {arg.Track.Title} got stuck for {arg.Threshold}ms. Please check Lavalink console/logs.");
            arg.Player.Queue.Enqueue(arg.Track);
            await arg.Player.TextChannel.SendMessageAsync($"{arg.Track.Title} has been re-added to queue after getting stuck.");
        }

        private Task OnWebSocketClosed(WebSocketClosedEventArgs arg)
        {
            _logger.LogCritical($"Discord WebSocket connection closed with following reason: {arg.Reason}");
            return Task.CompletedTask;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            
            await Client.WaitForReadyAsync(stoppingToken);
            Client.Ready += OnReadyAsync;
        }

        private async Task OnReadyAsync()
        {
            // Avoid calling ConnectAsync again if it's already connected 
            // (It throws InvalidOperationException if it's already connected).
            if (!_lavaNode.IsConnected)
            {
                _lavaNode.ConnectAsync();
            }

            // Other ready related stuff
        }
    }
}
