using System;
using System.Collections.Generic;
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

            var control = new TicketPaneControl();
            CustomTaskPane pane = CustomTaskPanes.Add(control, "ITCC 未分配工单", explorer);
            pane.DockPosition = MsoCTPDockPosition.msoCTPDockPositionRight;
            pane.Width = 360;
            pane.Visible = true;

            var context = new ExplorerPaneContext(explorer, pane, RemoveTaskPane);
            _paneContexts.Add(context);
        }

        private void RemoveTaskPane(ExplorerPaneContext context)
        {
            if (context == null)
            {
                return;
            }

            _paneContexts.Remove(context);
            try
            {
                CustomTaskPanes.Remove(context.Pane);
            }
            catch (ArgumentException)
            {
                // Outlook 关闭窗口时可能已先移除任务窗格。
            }

            context.Dispose();
        }

        private sealed class ExplorerPaneContext : IDisposable
        {
            private readonly Action<ExplorerPaneContext> _closedCallback;
            private bool _disposed;

            internal ExplorerPaneContext(
                Outlook.Explorer explorer,
                CustomTaskPane pane,
                Action<ExplorerPaneContext> closedCallback)
            {
                Explorer = explorer;
                Pane = pane;
                _closedCallback = closedCallback;
                ((Outlook.ExplorerEvents_10_Event)Explorer).Close += OnExplorerClose;
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
                ((Outlook.ExplorerEvents_10_Event)Explorer).Close -= OnExplorerClose;
                (Pane.Control as IDisposable)?.Dispose();
            }
        }
    }
}
