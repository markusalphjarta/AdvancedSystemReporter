using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.UI;
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
            output.AddAttribute(HtmlTextWriterAttribute.Id, "ParametersPanel");
            output.RenderBeginTag(HtmlTextWriterTag.Div);            
            var ctl = new LargeButton()
                {
                    Header = StringUtil.GetString(value:context.Parameters["reportid"],defaultValue:"null"),
                    Icon = "WordProcessing/32x32/columns.png"
                };
            ctl.RenderControl(output);
            output.RenderEndTag();
        }
    }
}
