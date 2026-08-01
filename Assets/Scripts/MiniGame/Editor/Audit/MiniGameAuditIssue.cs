namespace CatCafe.MiniGame.Editor.Audit
{
    public enum MiniGameAuditSeverity
    {
        Info,
        Warning,
        Blocking
    }

    public sealed class MiniGameAuditIssue
    {
        public MiniGameAuditSeverity Severity;
        public string Code;
        public string Message;
        public string Path;

        public static MiniGameAuditIssue Create(
            MiniGameAuditSeverity severity,
            string code,
            string message,
            string path = "")
        {
            return new MiniGameAuditIssue
            {
                Severity = severity,
                Code = code ?? string.Empty,
                Message = message ?? string.Empty,
                Path = path ?? string.Empty
            };
        }
    }
}
