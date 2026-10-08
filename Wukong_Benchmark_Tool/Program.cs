using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Threading;
using Microsoft.Win32;

namespace Wukong_Benchmark_Tool
{
    internal class Program
    {
        [DllImport("user32.dll")]
        static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

        [DllImport("user32.dll")]
        static extern bool IsWindowVisible(IntPtr hWnd);

        static readonly string CpuTestConfig = @"[/Script/GSGameSettings.GSGameUserSettings]
DesiredScreenWidth=640
DesiredScreenHeight=360
bNeverShowStartupUI=False
UISettingData=((""MainDisplay"", ""0""),(""PrivacyAgreement"", ""0""),(""Min"", ""0""),(""FrameRateQualityFirst"", ""0""),(""ScreenBrightness"", ""0""),(""ScreenMode"", ""1""),(""ScreenRatio"", ""0""),(""ScreenResolution"", ""2""),(""WindowFullImageQuality"", ""666666""),(""LockFrameRate"", ""1""),(""Vsync"", ""1""),(""MotionBlur"", ""0""),(""Dlss"", ""1""),(""Dx12"", ""0""),(""SoundVolume"", ""0""),(""RecommendQualityLevel"", ""0""),(""ImageQuality"", ""237""),(""SuperResolutionSampling"", ""3""),(""InsertFrame"", ""0""),(""Rtx"", ""0""),(""QualityLevel"", ""1""),(""ViewDistance"", ""1""),(""AntiAliasing"", ""1""),(""PostProcessing"", ""1""),(""ShadowQuality"", ""1""),(""TextureQuality"", ""1""),(""FxQuality"", ""1""),(""MaterialQuality"", ""1""),(""VegetationQuality"", ""1""),(""GlobalIllumination"", ""1""),(""ReflectionQuality"", ""1""),(""RtxLevel"", ""1""))
UISettingCustomData=()
SettingpbTag=4
PrivacyAgreement=0
AgreementReaded=1
FirstSettingFinish=True
ArchiveMarkFinish=False
CrashReportAgreement=0
ShowCrashReportUI=0
bUseVSync=True
bUseDynamicResolution=False
ResolutionSizeX=1920
ResolutionSizeY=1080
LastUserConfirmedResolutionSizeX=1920
LastUserConfirmedResolutionSizeY=1080
WindowPosX=-1
WindowPosY=-1
FullscreenMode=1
LastConfirmedFullscreenMode=1
PreferredFullscreenMode=1
Version=5
AudioQualityLevel=0
LastConfirmedAudioQualityLevel=0
FrameRateLimit=30.000000
LastUserConfirmedDesiredScreenWidth=640
LastUserConfirmedDesiredScreenHeight=360
LastRecommendedScreenWidth=-1.000000
LastRecommendedScreenHeight=-1.000000
bUseHDRDisplayOutput=False
HDRDisplayOutputNits=1000

[ScalabilityGroups]
sg.ResolutionQuality=33.3333015
sg.ViewDistanceQuality=0
sg.AntiAliasingQuality=0
sg.ShadowQuality=0
sg.GlobalIlluminationQuality=0
sg.RayTracingQuality=0
sg.ReflectionQuality=0
sg.PostProcessQuality=0
sg.TextureQuality=0
sg.EffectsQuality=0
sg.FoliageQuality=0
sg.ShadingQuality=0

[RayTracing]
r.RayTracing.EnableInGame=False

[GSRenderSetting]
GSStreamingPoolSize=384
";

        static readonly string GpuTestConfig = @"[/Script/GSGameSettings.GSGameUserSettings]
bNeverShowStartupUI=False
UISettingData=((""MainDisplay"", ""0""),(""PrivacyAgreement"", ""0""),(""Min"", ""0""),(""FrameRateQualityFirst"", ""0""),(""ScreenBrightness"", ""100""),(""ScreenMode"", ""1""),(""ScreenRatio"", ""2""),(""ScreenResolution"", ""0""),(""WindowFullImageQuality"", ""1000000""),(""LockFrameRate"", ""0""),(""Vsync"", ""0""),(""MotionBlur"", ""2""),(""Dlss"", ""1""),(""Dx12"", ""0""),(""SoundVolume"", ""100""),(""RecommendQualityLevel"", ""0""),(""ImageQuality"", ""1080""),(""SuperResolutionSampling"", ""0""),(""InsertFrame"", ""1""),(""Rtx"", ""0""),(""QualityLevel"", ""3""),(""ViewDistance"", ""3""),(""AntiAliasing"", ""3""),(""PostProcessing"", ""3""),(""ShadowQuality"", ""3""),(""TextureQuality"", ""3""),(""FxQuality"", ""3""),(""MaterialQuality"", ""3""),(""VegetationQuality"", ""3""),(""GlobalIllumination"", ""3""),(""ReflectionQuality"", ""3""),(""RtxLevel"", ""1""))
UISettingCustomData=()
SettingpbTag=4
PrivacyAgreement=0
AgreementReaded=1
FirstSettingFinish=True
ArchiveMarkFinish=False
CrashReportAgreement=0
ShowCrashReportUI=0
bUseVSync=False
bUseDynamicResolution=False
ResolutionSizeX=1920
ResolutionSizeY=1080
LastUserConfirmedResolutionSizeX=1280
LastUserConfirmedResolutionSizeY=720
WindowPosX=-1
WindowPosY=-1
FullscreenMode=1
LastConfirmedFullscreenMode=2
PreferredFullscreenMode=1
Version=5
AudioQualityLevel=0
LastConfirmedAudioQualityLevel=0
FrameRateLimit=0.000000
LastUserConfirmedDesiredScreenWidth=1920
LastUserConfirmedDesiredScreenHeight=1080
LastRecommendedScreenWidth=-1.000000
LastRecommendedScreenHeight=-1.000000
bUseHDRDisplayOutput=False
HDRDisplayOutputNits=1000
StartLevelName=
GMCommandList=()
MainMonitorID=""MONITOR\\AUO21ED\\{4d36e96e-e325-11ce-bfc1-08002be10318}\\0001""

[ScalabilityGroups]
sg.ResolutionQuality=100
sg.ViewDistanceQuality=2
sg.AntiAliasingQuality=2
sg.ShadowQuality=2
sg.GlobalIlluminationQuality=2
sg.RayTracingQuality=0
sg.ReflectionQuality=2
sg.PostProcessQuality=2
sg.TextureQuality=2
sg.EffectsQuality=2
sg.FoliageQuality=2
sg.ShadingQuality=2

[RayTracing]
r.RayTracing.EnableInGame=False

[GSRenderSetting]
GSStreamingPoolSize=512
";

        static BenchmarkResult CpuResult = new BenchmarkResult();
        static BenchmarkResult GpuResult = new BenchmarkResult();

        static void Main(string[] args)
        {
            string benchmarkPath = null;
            string configPath = null;
            string backupPath = null;
            try
            {
                Console.WriteLine("Поиск бенчмарка");
                benchmarkPath = FindBenchmarkPath();
                if (string.IsNullOrEmpty(benchmarkPath))
                {
                    Console.WriteLine("ОШИБКА: Benchmark Tool не найден.");
                    return;
                }
                Console.WriteLine($"Бенчмарк: {benchmarkPath}");

                Console.WriteLine("\nПоиск конфига");
                configPath = FindConfigFile(benchmarkPath);
                if (string.IsNullOrEmpty(configPath))
                {
                    Console.WriteLine("ОШИБКА: GameUserSettings.ini не найден.");
                    return;
                }
                Console.WriteLine($"Конфиг: {configPath}");

                CpuResult = RunBenchmarkPass("CPU", benchmarkPath, configPath, CpuTestConfig, ref backupPath);
                GpuResult = RunBenchmarkPass("GPU", benchmarkPath, configPath, GpuTestConfig, ref backupPath);
                PrintFinalReport();
            }
            catch (UnauthorizedAccessException ex)
            {
                Console.WriteLine("\n!!! ОШИБКА ДОСТУПА !!!");
                Console.WriteLine("Программа не имеет прав на запись в папку Program Files.");
                Console.WriteLine("Запустите её ОТ ИМЕНИ АДМИНИСТРАТОРА.");
                Console.WriteLine($"Подробности: {ex.Message}");
            }
            catch (Exception ex)
            {
                Console.WriteLine("\n!!! НЕОЖИДАННАЯ ОШИБКА !!!");
                Console.WriteLine($"Тип: {ex.GetType().Name}");
                Console.WriteLine($"Сообщение: {ex.Message}");
                Console.WriteLine($"Стек: {ex.StackTrace}");
            }
            finally
            {
                if (!string.IsNullOrEmpty(backupPath) && !string.IsNullOrEmpty(configPath) && File.Exists(backupPath))
                {
                    try
                    {
                        File.Copy(backupPath, configPath, overwrite: true);
                        File.Delete(backupPath);
                        Console.WriteLine("\nОригинальный конфиг восстановлен.");
                    }
                    catch { }
                }
            }

            Console.WriteLine("\nНажмите любую клавишу для выхода");
            Console.ReadKey();
        }

        static BenchmarkResult RunBenchmarkPass(string passName, string benchmarkPath, string configPath, string configContent, ref string backupPath)
        {
            var result = new BenchmarkResult();

            try
            {
                backupPath = configPath + ".bak";
                File.Copy(configPath, backupPath, overwrite: true);
                Console.WriteLine($"{passName} Бэкап оригинала: {backupPath}");

                File.WriteAllText(configPath, configContent);
                Console.WriteLine($"{passName} Настройки применены.");

                KillBenchmarkIfRunning();
                Thread.Sleep(1000);

                Console.WriteLine($"{passName} Запуск бенчмарка");
                ProcessStartInfo startInfo = new ProcessStartInfo
                {
                    FileName = benchmarkPath,
                    UseShellExecute = true,
                    WorkingDirectory = Path.GetDirectoryName(benchmarkPath)
                };

                Process process = Process.Start(startInfo);
                if (process == null)
                {
                    Console.WriteLine($"{passName} ОШИБКА: Process.Start вернул null.");
                    return result;
                }

                Console.WriteLine($"{passName} Команда отправлена (PID {process.Id}).");
                Console.WriteLine($"{passName} Ожидание окна бенчмарка (до 120 сек)");

                bool windowAppeared = WaitForBenchmarkWindow(120);
                if (!windowAppeared)
                {
                    Console.WriteLine($"{passName} Окно не появилось. Пропускаем.");
                    return result;
                }

                Console.WriteLine();
                Console.WriteLine($"  {passName}-ТЕСТ:");
                Console.WriteLine("   1. Нажмите 'Тест быстродействия'");
                Console.WriteLine("   2. Дождитесь финального экрана");
                Console.WriteLine("   3. Закройте бенчмарк");
                WaitForBenchmarkClose();
                Console.WriteLine($"{passName} Окно бенчмарка закрыто.");
                Thread.Sleep(3000);

                result = ReadResult(configPath);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"{passName} ОШИБКА: {ex.Message}");
            }
            finally
            {
                try
                {
                    if (File.Exists(backupPath))
                    {
                        File.Copy(backupPath, configPath, overwrite: true);
                        File.Delete(backupPath);
                        Console.WriteLine($"{passName} Оригинальный конфиг восстановлен.");
                        backupPath = null;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"{passName} Не удалось восстановить конфиг: {ex.Message}");
                }
            }

            return result;
        }

        static bool WaitForBenchmarkWindow(int timeoutSeconds)
        {
            int elapsed = 0;
            while (elapsed < timeoutSeconds)
            {
                Thread.Sleep(1000);
                elapsed++;

                IntPtr hwnd = FindWindow(null, "Black Myth: Wukong Benchmark Tool");
                if (hwnd == IntPtr.Zero) hwnd = FindWindow("UnrealWindow", null);
                if (hwnd == IntPtr.Zero) hwnd = FindWindow(null, "Black Myth Wukong Benchmark Tool");

                if (hwnd != IntPtr.Zero && IsWindowVisible(hwnd))
                    return true;

                var procs = Process.GetProcessesByName("b1-Win64-Shipping");
                if (procs.Length > 0)
                {
                    if (elapsed % 10 == 0)
                        Console.WriteLine($"      ... прошло {elapsed} сек, процесс жив, окно ещё не найдено");
                    continue;
                }

                if (elapsed > 10)
                {
                    Console.WriteLine($"      ... процесса b1-Win64-Shipping нет (возможно, упал).");
                    return false;
                }
            }
            return false;
        }

        static void WaitForBenchmarkClose()
        {
            while (true)
            {
                Thread.Sleep(2000);

                IntPtr hwnd = FindWindow(null, "Black Myth: Wukong Benchmark Tool");
                if (hwnd == IntPtr.Zero) hwnd = FindWindow("UnrealWindow", null);

                if (hwnd == IntPtr.Zero || !IsWindowVisible(hwnd))
                    break;

                var procs = Process.GetProcessesByName("b1-Win64-Shipping");
                if (procs.Length == 0)
                    break;
            }
            Thread.Sleep(2000);
        }

        static void KillBenchmarkIfRunning()
        {
            foreach (var p in Process.GetProcessesByName("b1-Win64-Shipping"))
            {
                try { p.Kill(); } catch { }
            }
        }

        static string FindBenchmarkPath()
        {
            string steamPath = GetSteamPathFromRegistry();
            if (string.IsNullOrEmpty(steamPath)) return null;
            return SearchInSteamLibraries(steamPath);
        }

        static string GetSteamPathFromRegistry()
        {
            string[] registryKeys = { @"Software\Valve\Steam", @"Software\Wow6432Node\Valve\Steam" };
            RegistryKey[] rootKeys = { Registry.CurrentUser, Registry.LocalMachine };

            foreach (var root in rootKeys)
                foreach (var regPath in registryKeys)
                {
                    try
                    {
                        using (var key = root.OpenSubKey(regPath))
                        {
                            if (key != null)
                            {
                                string path = key.GetValue("SteamPath")?.ToString() ?? key.GetValue("InstallPath")?.ToString();
                                if (!string.IsNullOrEmpty(path) && Directory.Exists(path))
                                    return path;
                            }
                        }
                    }
                    catch { }
                }
            return null;
        }

        static string SearchInSteamLibraries(string steamPath)
        {
            var libraryFolders = new List<string> { steamPath };
            string vdfPath = Path.Combine(steamPath, "steamapps", "libraryfolders.vdf");

            if (File.Exists(vdfPath))
            {
                try
                {
                    foreach (string line in File.ReadAllLines(vdfPath))
                    {
                        string trimmed = line.Trim();
                        if (trimmed.StartsWith("\"path\""))
                        {
                            var parts = trimmed.Split('"');
                            if (parts.Length >= 4)
                            {
                                string libPath = parts[3].Replace("\\\\", "\\");
                                if (Directory.Exists(libPath))
                                    libraryFolders.Add(libPath);
                            }
                        }
                    }
                }
                catch { }
            }

            string[] folderNames = { "Black Myth Wukong Benchmark Tool", "Black Myth Wukong Benchmark" };
            string[] exePatterns = {
                @"steamapps\common\{0}\b1\Binaries\Win64\b1-Win64-Shipping.exe",
                @"steamapps\common\{0}\b1\Binaries\Win64\Wukong_Benchmark_Tool.exe"
            };

            foreach (var library in libraryFolders)
                foreach (var folder in folderNames)
                    foreach (var pattern in exePatterns)
                    {
                        string fullPath = Path.Combine(library, string.Format(pattern, folder));
                        if (File.Exists(fullPath)) return fullPath;
                    }

            return null;
        }

        static string FindConfigFile(string benchmarkPath)
        {
            string exeDir = Path.GetDirectoryName(benchmarkPath);
            string b1Dir = Directory.GetParent(exeDir)?.Parent?.FullName;
            if (string.IsNullOrEmpty(b1Dir)) return null;
            string configPath = Path.Combine(b1Dir, "Saved", "Config", "Windows", "GameUserSettings.ini");
            return File.Exists(configPath) ? configPath : null;
        }

        class BenchmarkResult
        {
            public double? LastCpuResult;
            public double? LastGpuResult;
        }

        static BenchmarkResult ReadResult(string configPath)
        {
            var result = new BenchmarkResult();
            try
            {
                string content = File.ReadAllText(configPath);

                var mCpu = Regex.Match(content, @"LastCPUBenchmarkResult=([\d\.]+)");
                if (mCpu.Success)
                    result.LastCpuResult = double.Parse(mCpu.Groups[1].Value, System.Globalization.CultureInfo.InvariantCulture);

                var mGpu = Regex.Match(content, @"LastGPUBenchmarkResult=([\d\.]+)");
                if (mGpu.Success)
                    result.LastGpuResult = double.Parse(mGpu.Groups[1].Value, System.Globalization.CultureInfo.InvariantCulture);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка чтения результата: {ex.Message}");
            }
            return result;
        }

        static void PrintSystemInfo()
        {
            Console.WriteLine("  Характеристики компьютера:");
            try
            {
                using (var key = Registry.LocalMachine.OpenSubKey(@"HARDWARE\DESCRIPTION\System\CentralProcessor\0"))
                {
                    string cpu = key?.GetValue("ProcessorNameString")?.ToString()?.Trim();
                    Console.WriteLine($"    CPU:          {cpu ?? "N/A"}");
                }
            }
            catch { Console.WriteLine("    CPU:          (не удалось определить)"); }

            try
            {
                string gpu = "N/A";
                using (var key = Registry.LocalMachine.OpenSubKey(@"SYSTEM\CurrentControlSet\Control\Class\{4d36e968-e325-11ce-bfc1-08002be10318}\0000"))
                {
                    gpu = key?.GetValue("DriverDesc")?.ToString() ?? "N/A";
                }
                Console.WriteLine($"    GPU:          {gpu}");
            }
            catch { Console.WriteLine("    GPU:          (не удалось определить)"); }

            Console.WriteLine($"    ОС:           {Environment.OSVersion.VersionString}");
            Console.WriteLine($"    .NET:         {Environment.Version}");

        }

        static void PrintFinalReport()
        {
            Console.WriteLine("\n\n");
            PrintSystemInfo();
            Console.WriteLine();

            Console.WriteLine("  РЕЗУЛЬТАТ CPU-ТЕСТА");
            Console.WriteLine("  Настройки, применённые для CPU-теста:");
            Console.WriteLine("    - Разрешение:          640x360");
            Console.WriteLine("    - Набор графики:       Низкий");
            Console.WriteLine("    - Общий пресет:        1");
            Console.WriteLine("    - VSync:               Включён");
            Console.WriteLine("    - Порог FPS:           30");
            Console.WriteLine("    - DLSS:                Выключен");
            Console.WriteLine("    - RTX:                 Выключен");
            Console.WriteLine("    - Генерация кадров:    Выключена");
            Console.WriteLine("    - Размытие в движении: Выключено");
            Console.WriteLine();
            if (CpuResult.LastCpuResult.HasValue)
                Console.WriteLine($"  CPU (метрика движка):    {CpuResult.LastCpuResult.Value:F3}");
            else
                Console.WriteLine("  CPU (метрика движка):    не найдена");
            if (CpuResult.LastGpuResult.HasValue)
                Console.WriteLine($"  GPU (метрика движка):    {CpuResult.LastGpuResult.Value:F3}");
            Console.WriteLine();

            Console.WriteLine("  РЕЗУЛЬТАТ GPU-ТЕСТА");
            Console.WriteLine("  Настройки, применённые для GPU-теста:");
            Console.WriteLine("    - Разрешение:          1920x1080");
            Console.WriteLine("    - Набор графики:       Максимальный");
            Console.WriteLine("    - Общий пресет:        3");
            Console.WriteLine("    - VSync:               Выключен");
            Console.WriteLine("    - Порог FPS:           Без ограничения");
            Console.WriteLine("    - DLSS:                Выключен");
            Console.WriteLine("    - RTX:                 Выключен");
            Console.WriteLine("    - Генерация кадров:    Включена");
            Console.WriteLine("    - Размытие в движении: Включено");
            Console.WriteLine();
            if (GpuResult.LastCpuResult.HasValue)
                Console.WriteLine($"  CPU (метрика движка):    {GpuResult.LastCpuResult.Value:F3}");
            else
                Console.WriteLine("  CPU (метрика движка):    не найдена");
            if (GpuResult.LastGpuResult.HasValue)
                Console.WriteLine($"  GPU (метрика движка):    {GpuResult.LastGpuResult.Value:F3}");
            Console.WriteLine();
        }
    }
}