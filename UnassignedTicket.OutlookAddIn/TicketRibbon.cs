using System.Runtime.InteropServices;
using Office = Microsoft.Office.Core;

namespace UnassignedTicket.OutlookAddIn
{
    [ComVisible(true)]
    public sealed class TicketRibbon : Office.IRibbonExtensibility
    {
        private const string PaneButtonId = "itccToggleUnassignedTickets";
        private readonly ThisAddIn _addIn;
        private Office.IRibbonUI _ribbonUi;

        internal TicketRibbon(ThisAddIn addIn)
        {
            _addIn = addIn;
        }

        public string GetCustomUI(string ribbonId)
        {
            if (ribbonId != "Microsoft.Outlook.Explorer") return null;

            return @"<?xml version=""1.0"" encoding=""UTF-8""?>
<customUI xmlns=""http://schemas.microsoft.com/office/2009/07/customui"" onLoad=""OnLoad"">
  <ribbon>
    <tabs>
      <tab id=""itccTicketTab"" label=""ITCC 工单"">
        <group id=""itccTicketGroup"" label=""工单监控"">
          <toggleButton id=""itccToggleUnassignedTickets""
                        label=""未分配工单""
                        screentip=""显示或隐藏未分配工单侧边栏""
                        size=""large""
                        getPressed=""GetPaneVisible""
                        onAction=""OnTogglePane"" />
        </group>
      </tab>
    </tabs>
  </ribbon>
</customUI>";
        }

        public void OnLoad(Office.IRibbonUI ribbonUi)
        {
            _ribbonUi = ribbonUi;
        }

        public bool GetPaneVisible(Office.IRibbonControl control)
        {
            return _addIn.IsActiveTaskPaneVisible();
        }

        public void OnTogglePane(Office.IRibbonControl control, bool pressed)
        {
            _addIn.SetActiveTaskPaneVisible(pressed);
            InvalidatePaneButton();
        }

        internal void InvalidatePaneButton()
        {
            _ribbonUi?.InvalidateControl(PaneButtonId);
        }
    }
}
