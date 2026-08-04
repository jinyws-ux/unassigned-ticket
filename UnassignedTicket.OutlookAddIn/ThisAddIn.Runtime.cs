using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Microsoft.Office.Core;
using UnassignedTicket.OutlookAddIn.UI;
using CustomTaskPane = Microsoft.Office.Tools.CustomTaskPane;
using Outlook = Microsoft.Office.Interop.Outlook;

namespace UnassignedTicket.OutlookAddIn
{
    public partial class ThisAddIn
    {
        private readonly List<ExplorerPaneContext> _paneContexts = new List<ExplorerPaneContext>();
        private Outlook.Explorers _explorers;
        private TicketRibbon _ticketRibbon;

        protected override IRibbonExtensibility CreateRibbonExtensibilityObject()
        {
            _ticketRibbon = new TicketRibbon(this);
            return _ticketRibbon;
        }

        private void StartTicketMonitor()
        {
            _explorers = Application.Explorers;
            _explorers.NewExplorer += OnNewExplorer;

            for (int index = 1; index <= _explorers.Count; index++)
            {
                AttachTaskPane(_explorers[index]);
            }

            Outlook.Explorer activeExplorer = Application.ActiveExplorer();
            if (activeExplorer != null)
            {
                AttachTaskPane(activeExplorer);
            }
        }

        private void StopTicketMonitor()
        {
            if (_explorers != null)
            {
                _explorers.NewExplorer -= OnNewExplorer;
            }

            foreach (ExplorerPaneContext context in _paneContexts.ToArray())
            {
                context.Dispose();
            }

            _paneContexts.Clear();
        }

        private void OnNewExplorer(Outlook.Explorer explorer)
        {
            AttachTaskPane(explorer);
        }

        private void AttachTaskPane(Outlook.Explorer explorer)
        {
            if (explorer == null || _paneContexts.Exists(item => item.IsFor(explorer)))
            {
                return;
            }

            var control = new TicketPaneControl(() => ShowTicketPane(explorer));
            CustomTaskPane pane = CustomTaskPanes.Add(control, "ITCC 未分配工单", explorer);
            pane.DockPosition = MsoCTPDockPosition.msoCTPDockPositionRight;
            pane.Width = 360;
            pane.Visible = true;

            var context = new ExplorerPaneContext(explorer, pane, RemoveTaskPane, OnTaskPaneVisibleChanged);
            _paneContexts.Add(context);
        }

        internal bool IsActiveTaskPaneVisible()
        {
            Outlook.Explorer explorer = Application.ActiveExplorer();
            ExplorerPaneContext context = FindPaneContext(explorer);
            return context != null && context.Pane.Visible;
        }

        internal void SetActiveTaskPaneVisible(bool visible)
        {
            Outlook.Explorer explorer = Application.ActiveExplorer();
            if (explorer == null) return;

            AttachTaskPane(explorer);
            ShowTicketPane(explorer, visible);
        }

        internal void OpenDatabaseSettings()
        {
            Outlook.Explorer explorer = Application.ActiveExplorer();
            if (explorer == null) return;

            AttachTaskPane(explorer);
            ShowTicketPane(explorer, true);

            ExplorerPaneContext context = FindPaneContext(explorer);
            (context?.Pane.Control as TicketPaneControl)?.OpenSettingsDialog();
        }

        private void ShowTicketPane(Outlook.Explorer explorer)
        {
            ShowTicketPane(explorer, true);
        }

        private void ShowTicketPane(Outlook.Explorer explorer, bool visible)
        {
            ExplorerPaneContext context = FindPaneContext(explorer);
            if (context == null) return;

            context.Pane.Visible = visible;
            if (visible)
            {
                explorer.Activate();
            }
        }

        private ExplorerPaneContext FindPaneContext(Outlook.Explorer explorer)
        {
            return explorer == null ? null : _paneContexts.Find(item => item.IsFor(explorer));
        }

        private void OnTaskPaneVisibleChanged(object sender, EventArgs e)
        {
            _ticketRibbon?.InvalidatePaneButton();
        }

        private void RemoveTaskPane(ExplorerPaneContext context)
        {
            if (context == null)
            {
                return;
            }

            _paneContexts.Remove(context);
            context.Dispose();
            try
            {
                CustomTaskPanes.Remove(context.Pane);
            }
            catch (ArgumentException)
            {
                // Outlook 关闭窗口时可能已先移除任务窗格。
            }
            catch (ObjectDisposedException)
            {
                // Outlook 退出时可能已释放任务窗格。
            }
            catch (COMException)
            {
                // Outlook COM 对象已进入关闭流程。
            }
        }

        private sealed class ExplorerPaneContext : IDisposable
        {
            private readonly Action<ExplorerPaneContext> _closedCallback;
            private readonly EventHandler _visibleChangedCallback;
            private bool _disposed;

            internal ExplorerPaneContext(
                Outlook.Explorer explorer,
                CustomTaskPane pane,
                Action<ExplorerPaneContext> closedCallback,
                EventHandler visibleChangedCallback)
            {
                Explorer = explorer;
                Pane = pane;
                _closedCallback = closedCallback;
                _visibleChangedCallback = visibleChangedCallback;
                ((Outlook.ExplorerEvents_10_Event)Explorer).Close += OnExplorerClose;
                Pane.VisibleChanged += _visibleChangedCallback;
            }

            internal Outlook.Explorer Explorer { get; }
            internal CustomTaskPane Pane { get; }

            internal bool IsFor(Outlook.Explorer explorer)
            {
                return ReferenceEquals(Explorer, explorer);
            }

            private void OnExplorerClose()
            {
                _closedCallback(this);
            }

            public void Dispose()
            {
                if (_disposed)
                {
                    return;
                }

                _disposed = true;
                try
                {
                    ((Outlook.ExplorerEvents_10_Event)Explorer).Close -= OnExplorerClose;
                }
                catch (COMException)
                {
                    // Explorer 已由 Outlook 释放。
                }
                catch (ObjectDisposedException)
                {
                    // Explorer 包装对象已释放。
                }

                try
                {
                    Pane.VisibleChanged -= _visibleChangedCallback;
                }
                catch (ObjectDisposedException)
                {
                    // 任务窗格已由 VSTO 释放。
                }
                catch (COMException)
                {
                    // 任务窗格 COM 对象已释放。
                }
            }
        }
    }
}
