using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace AutoAbiba
{
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            // Словник, де: Ключ = назва процесу гри (без .exe), Значення = ім'я файлу софту поруч
            var gamesConfig = new Dictionary<string, string>
            {
                { "cs2", "/* Ваша програма*/"},
                { "PioneerGame-e", "/* Ваша програма*/" },
                { "RustClient", "/* Ваша програма*/" }
            };

            // Запускаємо моніторинг списку ігор у фоні
            Task.Run(() => MonitorMultiGames(gamesConfig));
        }

        private void MonitorMultiGames(Dictionary<string, string> config)
        {
            // Словник для відстеження стану: чи запущено супутній софт для конкретної гри
            var runningApps = new Dictionary<string, Process>();

            // Ініціалізуємо стани для кожної гри
            foreach (var game in config.Keys) runningApps[game] = null;

            while (true)
            {
                foreach (var pair in config)
                {
                    string gameName = pair.Key;
                    string appName = pair.Value;
                    string appPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, appName);

                    bool gameOpen = Process.GetProcessesByName(gameName).Length > 0;
                    bool isAppRunning = runningApps[gameName] != null;

                    // 1. Гра відкрилася, а софт для неї ще НЕ запущений
                    if (gameOpen && !isAppRunning)
                    {
                        if (File.Exists(appPath))
                        {
                            try { runningApps[gameName] = Process.Start(appPath); } catch { }
                        }
                    }
                    // 2. Гра закрилася, а софт для неї досі активний
                    else if (!gameOpen && isAppRunning)
                    {
                        var child = runningApps[gameName];
                        try { if (child != null && !child.HasExited) child.Kill(); } catch { }
                        runningApps[gameName] = null; // Скидаємо стан
                    }
                }

                Thread.Sleep(3000); 
            }
        }
    }
}