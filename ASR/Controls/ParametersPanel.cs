using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.UI;
using ASR.DomainObjects;
using Sitecore;
using Sitecore.Data.Items;
using Sitecore.Shell.Framework.Commands;
using Sitecore.Shell.Web.UI.WebControls;
using Sitecore.Web.UI.WebControls.Ribbons;

namespace ASR.Controls
{
    public class ParametersPanel : RibbonPanel
    {
        
        public override void Render(HtmlTextWriter output, Ribbon ribbon, Item button, CommandContext context)
        {
            var reportItem = Current.Context.ReportItem;
            if (reportItem == null) return;
            RenderParameters(output, reportItem, reportItem.Scanners);
            RenderParameters(output, reportItem, reportItem.Filters);            
       }
        
        private static void RenderParameters(HtmlTextWriter output, ReportItem reportItem, IEnumerable<ReferenceItem> referenceItems)
        {
            
            foreach (var ri in referenceItems)
            {
                foreach (var parameter in ri.Parameters)
                {
                    dynamic ctl = parameter.BuildControl();
                    if (ctl != null)
                    {                        
                        ctl.RenderControl(output);    
                    }                    
                }
            }
        }
    }
}
