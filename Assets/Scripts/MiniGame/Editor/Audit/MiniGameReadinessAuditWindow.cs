using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace CatCafe.MiniGame.Editor.Audit
{
    public sealed class MiniGameReadinessAuditWindow : EditorWindow
    {
        private Vector2 _scroll;
        private List<MiniGameAuditIssue> _issues = new List<MiniGameAuditIssue>();

        [MenuItem("Tools/Cat Cafe/MiniGame Readiness Audit")]
        public static void Open()
        {
            MiniGameReadinessAuditWindow window = GetWindow<MiniGameReadinessAuditWindow>("MiniGame Audit");
            window.RunAudit();
            window.Show();
        }

        private void OnGUI()
        {
            EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
            if (GUILayout.Button("Run Audit", EditorStyles.toolbarButton, GUILayout.Width(100)))
                RunAudit();

            GUILayout.FlexibleSpace();
            GUILayout.Label("Issues: " + _issues.Count, EditorStyles.miniLabel);
            EditorGUILayout.EndHorizontal();

            _scroll = EditorGUILayout.BeginScrollView(_scroll);

            if (_issues.Count == 0)
            {
                EditorGUILayout.HelpBox("No audit results yet.", MessageType.Info);
            }
            else
            {
                for (int i = 0; i < _issues.Count; i++)
                {
                    DrawIssue(_issues[i]);
                }
            }

            EditorGUILayout.EndScrollView();
        }

        private void RunAudit()
        {
            _issues = MiniGameReadinessAudit.Run();
            Repaint();
        }

        private static void DrawIssue(MiniGameAuditIssue issue)
        {
            MessageType type = MessageType.Info;
            if (issue.Severity == MiniGameAuditSeverity.Warning)
                type = MessageType.Warning;
            else if (issue.Severity == MiniGameAuditSeverity.Blocking)
                type = MessageType.Error;

            string body = issue.Code + "\n" + issue.Message;
            if (!string.IsNullOrWhiteSpace(issue.Path))
                body += "\n" + issue.Path;

            EditorGUILayout.HelpBox(body, type);
        }
    }
}
