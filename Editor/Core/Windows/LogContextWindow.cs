﻿using ThunderKit.Core.Pipelines;
using UnityEditor;
using ThunderKit.Markdown;
using System.Linq;
#if UNITY_2019_1_OR_NEWER
using UnityEngine.UIElements;
using UnityEditor.UIElements;
#else
using UnityEngine.Experimental.UIElements;
using UnityEditor.Experimental.UIElements;
#endif

namespace ThunderKit.Core.Windows
{
    public class LogContextWindow : TemplatedWindow
    {
        internal static LogContextWindow instance;

        public LogEntry logEntry;
        private VisualElement tabSection;
        private VisualElement contentSection;
        public static bool IsOpen { get; private set; }
        public static LogContextWindow ShowContext(LogEntry logEntry)
        {
            if (!IsOpen || instance == null)
            {
                var content = EditorGUIUtility.IconContent("d_UnityEditor.InspectorWindow");
                content.text = "Log Inspector";
                instance = GetWindow<LogContextWindow>($"Log Inspector");
                instance.titleContent = content;
            }
            instance.logEntry = logEntry;
            instance.Initialize();

            return instance;
        }

        public override void OnEnable()
        {
            base.OnEnable();
            Initialize();
            IsOpen = true;
        }
        private void OnDisable() => IsOpen = false;
        private void OnDestroy() => IsOpen = false;

        private void Initialize()
        {
            if (tabSection == null) tabSection = rootVisualElement.Q<VisualElement>("tab-section");
            if (contentSection == null) contentSection = rootVisualElement.Q<VisualElement>("content-section");

            contentSection.Clear();
            tabSection.Clear();

            if (logEntry.context != null)
                foreach (var data in logEntry.context)
                {
                    var firstLine = data.Substring(0, data.IndexOf("\r\n"));
                    var remainingData = data.Substring(firstLine.Length);

                    var tabButton = new Toggle();
                    tabButton.value = false;
                    tabButton.text = firstLine;
                    tabButton.AddToClassList("tab-button");
                    tabButton.name = $"tab-{firstLine.ToLowerInvariant()}";

                    tabSection.Add(tabButton);

                    var markdownContent = new MarkdownElement { Data = remainingData, MarkdownDataType = MarkdownDataType.Text };
                    var stacktraceScrollView = new ScrollView { markdownContent };
                    stacktraceScrollView.name = $"content-{firstLine.ToLowerInvariant()}";
                    stacktraceScrollView.StretchToParentSize();
#if UNITY_2019_1_OR_NEWER
#elif UNITY_2018_1_OR_NEWER
                    stacktraceScrollView.stretchContentWidth = true;
#endif
                    contentSection.Add(stacktraceScrollView);

                    tabButton.OnValueChanged(evt =>
                    {
                        foreach (var child in contentSection.Children())
                            child.visible = false;

                        foreach (var child in tabSection.Children().OfType<Toggle>())
                            if (child != tabButton)
                                child.value = false;

                        stacktraceScrollView.visible = tabButton.value;
                        if (stacktraceScrollView.visible && markdownContent.childCount == 0)
                            markdownContent.RefreshContent();
                    });
                }

            var firstDetail = tabSection.Children().OfType<Toggle>().FirstOrDefault();
            if (firstDetail != null) firstDetail.value = true;
            if (tabSection.childCount == 1)
                firstDetail.SetEnabled(false);
            rootVisualElement.Bind(new SerializedObject(this));
        }

        internal void Clear()
        {
            logEntry = default;
        }
    }
}