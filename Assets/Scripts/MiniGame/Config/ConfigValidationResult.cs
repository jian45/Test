using System;
using System.Collections.Generic;

namespace CatCafe.MiniGame.Config
{
    public enum ConfigValidationSeverity
    {
        Info,
        Warning,
        Blocking
    }

    [Serializable]
    public sealed class ConfigValidationIssue
    {
        public ConfigValidationSeverity Severity;
        public string Code;
        public string Message;
    }

    [Serializable]
    public sealed class ConfigValidationResult
    {
        private readonly List<ConfigValidationIssue> _issues = new List<ConfigValidationIssue>();

        public IReadOnlyList<ConfigValidationIssue> Issues
        {
            get { return _issues; }
        }

        public bool HasBlockingIssues
        {
            get
            {
                for (int i = 0; i < _issues.Count; i++)
                {
                    if (_issues[i].Severity == ConfigValidationSeverity.Blocking)
                        return true;
                }

                return false;
            }
        }

        public void Add(ConfigValidationSeverity severity, string code, string message)
        {
            _issues.Add(new ConfigValidationIssue
            {
                Severity = severity,
                Code = code ?? string.Empty,
                Message = message ?? string.Empty
            });
        }
    }
}
