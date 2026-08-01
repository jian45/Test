using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text.RegularExpressions;
using CatCafe.MiniGame.Config;
using UnityEngine;

namespace CatCafe.MiniGame.Editor.Audit
{
    public static class MiniGameReadinessAudit
    {
        public static List<MiniGameAuditIssue> Run()
        {
            string projectRoot = Path.GetFullPath(Path.Combine(Application.dataPath, ".."));
            List<MiniGameAuditIssue> issues = new List<MiniGameAuditIssue>();

            CheckPackageState(projectRoot, issues);
            CheckRuntimeDefaults(issues);
            CheckReferenceBoundaries(projectRoot, issues);
            CheckAddressablesSettings(projectRoot, issues);
            CheckSensitiveValues(projectRoot, issues);
            CheckGitDirtyState(projectRoot, issues);

            return issues;
        }

        private static void CheckPackageState(string projectRoot, List<MiniGameAuditIssue> issues)
        {
            string manifestPath = Path.Combine(projectRoot, "Packages", "manifest.json");
            string lockPath = Path.Combine(projectRoot, "Packages", "packages-lock.json");
            string manifest = File.Exists(manifestPath) ? File.ReadAllText(manifestPath) : string.Empty;
            string lockText = File.Exists(lockPath) ? File.ReadAllText(lockPath) : string.Empty;

            string wxPackageId = "com.qq.weixin.minigame";
            bool wxPackageVisible = manifest.Contains(wxPackageId) ||
                                    lockText.Contains(wxPackageId) ||
                                    PackageCacheContains(projectRoot, wxPackageId);

            if (wxPackageVisible)
            {
                issues.Add(MiniGameAuditIssue.Create(
                    MiniGameAuditSeverity.Info,
                    "WECHAT_SDK_VISIBLE",
                    "微信小游戏 SDK 可见。阶段 C 允许在 Platform/WeChat 平台适配层集中使用。",
                    manifestPath));
            }
            else
            {
                issues.Add(MiniGameAuditIssue.Create(
                    MiniGameAuditSeverity.Blocking,
                    "WECHAT_SDK_NOT_VISIBLE",
                    "阶段 C 需要 WXSDK，但 manifest、packages-lock 和 PackageCache 中未发现 com.qq.weixin.minigame。IDE 工程文件引用不能作为可编译证据。",
                    manifestPath));
            }

            string addressablesPackageId = "com.unity." + "addressables";
            bool addressablesVisible = manifest.Contains(addressablesPackageId) ||
                                       lockText.Contains(addressablesPackageId) ||
                                       PackageCacheContains(projectRoot, addressablesPackageId);
            issues.Add(MiniGameAuditIssue.Create(
                addressablesVisible ? MiniGameAuditSeverity.Info : MiniGameAuditSeverity.Blocking,
                addressablesVisible ? "ADDRESSABLES_VISIBLE" : "ADDRESSABLES_MISSING",
                addressablesVisible
                    ? "Addressables 已安装。阶段 C 允许在 Resource / Runtime Resource / Editor Addressables 工具中使用。"
                    : "阶段 C 需要 Addressables，但 manifest / packages-lock 中未发现 com.unity.addressables。",
                manifestPath));

            bool dotweenVisible = Directory.Exists(Path.Combine(projectRoot, "Assets", "Plugins", "Demigiant", "DOTween")) ||
                                  File.Exists(Path.Combine(projectRoot, "Assets", "Resources", "DOTweenSettings.asset"));
            issues.Add(MiniGameAuditIssue.Create(
                dotweenVisible ? MiniGameAuditSeverity.Info : MiniGameAuditSeverity.Warning,
                dotweenVisible ? "DOTWEEN_VISIBLE" : "DOTWEEN_MISSING",
                dotweenVisible
                    ? "DOTween 已存在。阶段 C 允许用于 Loading、提示、按钮冷却和 UI 过渡。"
                    : "未发现 DOTween。若表现层不依赖 DOTween，可作为非阻塞项。",
                Path.Combine(projectRoot, "Assets", "Plugins", "Demigiant", "DOTween")));
        }

        private static void CheckRuntimeDefaults(List<MiniGameAuditIssue> issues)
        {
            MiniGameRuntimeConfig config = MiniGameConfigDefaults.CreateSafeDefaults();

            if (string.IsNullOrWhiteSpace(config.AppId) || config.AppId == MiniGameRuntimeConfig.PlaceholderAppId)
            {
                issues.Add(MiniGameAuditIssue.Create(
                    MiniGameAuditSeverity.Warning,
                    "APP_ID_PLACEHOLDER",
                    "AppID 为空或占位。Editor Mock 可运行，但 WebGL、开发者工具导入、体验版、提审和发布前必须补齐正式 AppID。"));
            }

            for (int i = 0; i < config.RewardedAdPlacements.Count; i++)
            {
                RewardedAdPlacementConfig placement = config.RewardedAdPlacements[i];
                if (placement == null)
                    continue;

                if (string.IsNullOrWhiteSpace(placement.AdUnitId))
                {
                    issues.Add(MiniGameAuditIssue.Create(
                        MiniGameAuditSeverity.Warning,
                        "AD_UNIT_ID_EMPTY",
                        "adUnitId 为空：" + placement.PlacementId + "。Editor Mock 可运行，真实广告和发布前阻塞。"));
                }
            }
        }

        private static void CheckReferenceBoundaries(string projectRoot, List<MiniGameAuditIssue> issues)
        {
            string scriptsRoot = Path.Combine(projectRoot, "Assets", "Scripts");
            string editorRoot = Path.Combine(projectRoot, "Assets", "Editor");
            if (!Directory.Exists(scriptsRoot))
                return;

            Regex wxPattern = new Regex("WeChat" + "WASM|\\bW" + "X\\s*\\.");
            Regex addressablesPattern = new Regex("UnityEngine" + "\\." + "AddressableAssets|UnityEngine\\.ResourceManagement|Addressables\\s*\\.");
            Regex dotweenPattern = new Regex("DG\\.Tweening|DOTween");

            foreach (string file in EnumerateFilesSafe(scriptsRoot))
            {
                string normalized = Normalize(file);
                string text = File.ReadAllText(file);

                if (wxPattern.IsMatch(text) && !IsUnder(normalized, "/Assets/Scripts/MiniGame/Platform/WeChat/"))
                {
                    issues.Add(MiniGameAuditIssue.Create(
                        MiniGameAuditSeverity.Blocking,
                        "WX_CALL_OUTSIDE_PLATFORM",
                        "阶段 C 真实微信调用必须集中在 Platform/WeChat 或等价平台适配层。",
                        file));
                }

                if (addressablesPattern.IsMatch(text) && !IsAllowedAddressablesPath(normalized))
                {
                    issues.Add(MiniGameAuditIssue.Create(
                        MiniGameAuditSeverity.Blocking,
                        "ADDRESSABLES_OUTSIDE_RESOURCE_LAYER",
                        "阶段 C Addressables 调用必须集中在 Resource、Runtime Resource 或 Editor Addressables 工具。",
                        file));
                }

                if (dotweenPattern.IsMatch(text) && IsCoreLogicPath(normalized))
                {
                    issues.Add(MiniGameAuditIssue.Create(
                        MiniGameAuditSeverity.Blocking,
                        "DOTWEEN_IN_CORE_LOGIC",
                        "DOTween 不应进入平台初始化、奖励判定、广告回调或资源释放核心逻辑。",
                        file));
                }
            }

            if (Directory.Exists(editorRoot))
            {
                foreach (string file in EnumerateFilesSafe(editorRoot))
                {
                    string normalized = Normalize(file);
                    string text = File.ReadAllText(file);
                    if (addressablesPattern.IsMatch(text) && !IsUnder(normalized, "/Assets/Editor/Addressables/"))
                    {
                        issues.Add(MiniGameAuditIssue.Create(
                            MiniGameAuditSeverity.Warning,
                            "EDITOR_ADDRESSABLES_USAGE_REVIEW",
                            "Editor 中的 Addressables 调用应限定在专门资源工具或审查工具中，并保持只读或显式人工触发。",
                            file));
                    }
                }
            }

            issues.Add(MiniGameAuditIssue.Create(
                MiniGameAuditSeverity.Info,
                "REFERENCE_BOUNDARIES_CHECKED",
                "已按阶段 C 口径检查 WX、Addressables、DOTween 的使用边界。"));
        }

        private static void CheckAddressablesSettings(string projectRoot, List<MiniGameAuditIssue> issues)
        {
            string settingsPath = Path.Combine(projectRoot, "Assets", "AddressableAssetsData", "AddressableAssetSettings.asset");
            if (!File.Exists(settingsPath))
            {
                issues.Add(MiniGameAuditIssue.Create(
                    MiniGameAuditSeverity.Blocking,
                    "ADDRESSABLE_SETTINGS_MISSING",
                    "未发现 AddressableAssetSettings.asset。",
                    settingsPath));
                return;
            }

            string settings = File.ReadAllText(settingsPath);
            if (settings.Contains("m_BuildRemoteCatalog: 1"))
            {
                issues.Add(MiniGameAuditIssue.Create(
                    MiniGameAuditSeverity.Blocking,
                    "REMOTE_CATALOG_ENABLED",
                    "首版策略要求关闭 Remote Catalog。",
                    settingsPath));
            }
            else
            {
                issues.Add(MiniGameAuditIssue.Create(
                    MiniGameAuditSeverity.Info,
                    "REMOTE_CATALOG_DISABLED",
                    "Addressables Remote Catalog 当前关闭。",
                    settingsPath));
            }

            if (settings.Contains("DATA_CDN"))
            {
                issues.Add(MiniGameAuditIssue.Create(
                    MiniGameAuditSeverity.Blocking,
                    "DATA_CDN_PRESENT",
                    "首版不应写入 DATA_CDN。",
                    settingsPath));
            }

            if (settings.Contains("http://[PrivateIpAddress]:[HostingServicePort]"))
            {
                issues.Add(MiniGameAuditIssue.Create(
                    MiniGameAuditSeverity.Warning,
                    "REMOTE_PROFILE_PLACEHOLDER",
                    "Remote.LoadPath 仍保留 Addressables 默认占位值；当前 Remote Catalog 关闭，首版不使用 CDN，发布前仍建议复核。",
                    settingsPath));
            }
        }

        private static void CheckSensitiveValues(string projectRoot, List<MiniGameAuditIssue> issues)
        {
            string scriptsRoot = Path.Combine(projectRoot, "Assets", "Scripts");
            Regex sensitivePattern = new Regex("AppSecret\\s*=|session_key\\s*=|adunit-[0-9A-Za-z_-]+|DATA_CDN\\s*=");

            foreach (string file in EnumerateFilesSafe(scriptsRoot))
            {
                string text = File.ReadAllText(file);
                if (!sensitivePattern.IsMatch(text))
                    continue;

                issues.Add(MiniGameAuditIssue.Create(
                    MiniGameAuditSeverity.Blocking,
                    "SENSITIVE_VALUE_IN_CODE",
                    "代码中疑似存在 AppSecret、session_key、真实 adUnitId 或 DATA_CDN。",
                    file));
            }

            issues.Add(MiniGameAuditIssue.Create(
                MiniGameAuditSeverity.Info,
                "SENSITIVE_VALUE_CHECKED",
                "已检查脚本中的敏感参数风险。"));
        }

        private static void CheckGitDirtyState(string projectRoot, List<MiniGameAuditIssue> issues)
        {
            string output = RunGitStatus(
                projectRoot,
                "status --short -- Packages/manifest.json Packages/packages-lock.json ProjectSettings Assets/Scenes");

            if (string.IsNullOrWhiteSpace(output))
            {
                issues.Add(MiniGameAuditIssue.Create(
                    MiniGameAuditSeverity.Info,
                    "PROTECTED_FILES_CLEAN",
                    "git status 未显示 Packages、ProjectSettings 或场景文件修改。"));
                return;
            }

            if (output.Contains("Packages/manifest.json") || output.Contains("Packages/packages-lock.json"))
            {
                issues.Add(MiniGameAuditIssue.Create(
                    MiniGameAuditSeverity.Info,
                    "PACKAGES_MODIFIED_PHASE_C",
                    "Packages 当前存在修改，可能包含 WXSDK、Addressables 及其依赖锁定记录。阶段 C 允许这些依赖存在；请用户确认变更来源后再纳入版本控制。",
                    "Packages"));
            }

            if (output.Contains("ProjectSettings/EditorBuildSettings.asset"))
            {
                issues.Add(MiniGameAuditIssue.Create(
                    MiniGameAuditSeverity.Info,
                    "EDITOR_BUILD_SETTINGS_ADDRESSABLES_CONFIG",
                    "EditorBuildSettings 当前存在 Addressables 配置对象变更。阶段 C 允许使用既有 Addressables 配置。",
                    "ProjectSettings/EditorBuildSettings.asset"));
            }

            if (output.Contains("Assets/Scenes/"))
            {
                issues.Add(MiniGameAuditIssue.Create(
                    MiniGameAuditSeverity.Blocking,
                    "SCENE_CURRENTLY_MODIFIED",
                    "Assets/Scenes 下存在场景修改。本轮禁止擅自修改场景。",
                    "Assets/Scenes"));
            }
        }

        private static IEnumerable<string> EnumerateFilesSafe(string root)
        {
            if (string.IsNullOrWhiteSpace(root) || !Directory.Exists(root))
                yield break;

            string[] files;
            try
            {
                files = Directory.GetFiles(root, "*.cs", SearchOption.AllDirectories);
            }
            catch
            {
                yield break;
            }

            for (int i = 0; i < files.Length; i++)
                yield return files[i];
        }

        private static bool IsAllowedAddressablesPath(string normalizedPath)
        {
            return IsUnder(normalizedPath, "/Assets/Scripts/MiniGame/Resource/") ||
                   IsUnder(normalizedPath, "/Assets/Scripts/Runtime/Resource/") ||
                   IsUnder(normalizedPath, "/Assets/Editor/Addressables/") ||
                   IsUnder(normalizedPath, "/Assets/Scripts/MiniGame/Editor/Audit/");
        }

        private static bool PackageCacheContains(string projectRoot, string packageId)
        {
            string cacheRoot = Path.Combine(projectRoot, "Library", "PackageCache");
            if (!Directory.Exists(cacheRoot))
                return false;

            try
            {
                string[] directories = Directory.GetDirectories(cacheRoot);
                for (int i = 0; i < directories.Length; i++)
                {
                    string name = Path.GetFileName(directories[i]);
                    if (string.Equals(name, packageId, System.StringComparison.OrdinalIgnoreCase) ||
                        name.StartsWith(packageId + "@", System.StringComparison.OrdinalIgnoreCase))
                    {
                        return true;
                    }
                }
            }
            catch
            {
                return false;
            }

            return false;
        }

        private static bool IsCoreLogicPath(string normalizedPath)
        {
            return IsUnder(normalizedPath, "/Assets/Scripts/MiniGame/Platform/") ||
                   IsUnder(normalizedPath, "/Assets/Scripts/MiniGame/Commercialization/Ads/") ||
                   IsUnder(normalizedPath, "/Assets/Scripts/MiniGame/Resource/") ||
                   IsUnder(normalizedPath, "/Assets/Scripts/Runtime/Resource/");
        }

        private static bool IsUnder(string normalizedPath, string expectedToken)
        {
            return normalizedPath.Contains(expectedToken);
        }

        private static string Normalize(string path)
        {
            return (path ?? string.Empty).Replace('\\', '/');
        }

        private static string RunGitStatus(string projectRoot, string arguments)
        {
            try
            {
                ProcessStartInfo startInfo = new ProcessStartInfo
                {
                    FileName = "git",
                    Arguments = arguments,
                    WorkingDirectory = projectRoot,
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true
                };

                using (Process process = Process.Start(startInfo))
                {
                    if (process == null)
                        return string.Empty;

                    string output = process.StandardOutput.ReadToEnd();
                    process.WaitForExit(2000);
                    return output;
                }
            }
            catch
            {
                return string.Empty;
            }
        }
    }
}
