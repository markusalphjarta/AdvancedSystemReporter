using System;
using System.Collections.Generic;
using System.Web.UI;
using Sitecore;
using Sitecore.Data.Items;
using Sitecore.Diagnostics;
using Sitecore.Resources;
using Sitecore.Web.UI;

namespace ASR.Controls
{
    public class Dropdown : System.Web.UI.WebControls.WebControl
    {
        #region Fields

        private string _keyCode;
        private readonly List<Item> _items = new List<Item>();

        #endregion

        #region Public properties

        /// <summary>
        /// Gets or sets the command.
        /// </summary>
        /// <value>The command.</value>
        public string Command { get; set; }

        /// <summary>
        /// Gets or sets the header.
        /// </summary>
        /// <value>The header.</value>
        public string Header { get; set; }

        /// <summary>
        /// Gets or sets the icon.
        /// </summary>
        /// <value>The icon.</value>
        public string Icon { get; set; }

        /// <summary>
        /// Gets the items.
        /// </summary>
        /// <value>The items.</value>
        public List<Item> Items
        {
            get { return _items; }
        }

        /// <summary>
        /// Gets or sets the key code.
        /// </summary>
        /// <value>The key code.</value>
        [NotNull]
        public string KeyCode
        {
            get { return _keyCode ?? string.Empty; }
            set
            {
                Assert.ArgumentNotNull(value, "value");

                _keyCode = value;
            }
        }

        /// <summary>
        /// Gets or sets the value.
        /// </summary>
        /// <value>The value.</value>
        public string Value { get; set; }

        #endregion

        /// <summary>
        /// Renders the specified output.
        /// </summary>
        /// <param name="output">The output.</param>
        protected override void Render([NotNull] HtmlTextWriter output)
        {
            Assert.ArgumentNotNull(output, "output");

            if (!Visible)
            {
                return;
            }

            var alt = StringUtil.GetString(ToolTip) + Keyboard.GetToolTip(KeyCode);
            var tooltip = (alt.Length > 0 ? " title=\"" + alt + "\"" : String.Empty);
            var accessKey = !string.IsNullOrEmpty(AccessKey) ? " accesskey=\"" + AccessKey + "\"" : String.Empty;
            var value = StringUtil.EscapeQuote(Value);

            var image = new ImageBuilder
                {
                    Src = Images.GetThemedImageSource(Icon, ImageDimension.id16x16),
                    Class = "scRibbonToolbarSmallButtonIcon",
                    Alt = alt
                };

            if (!Enabled)
            {
                output.Write("<span class=\"scRibbonToolbarSmallDropDownBoxDisabled\"" + tooltip + ">");

                output.Write("<span class=\"scRibbonToolbarSmallDropDownIconGrayed\">");

                image.Class = "scRibbonToolbarSmallDropDownBoxIconDisabled";
                image.Disabled = true;

                output.Write(image.ToString());

                output.Write("</span>");

                output.Write(Header);

                output.Write("<select class=\"scRibbonToolbarSmallDropDownBoxInput\" value=\"" + value +
                             "\" disabled=\"disabled\">");

                foreach (var item in Items)
                {
                    output.Write("<option>");
                    output.Write(item.DisplayName);
                    output.Write("</option>");
                }

                output.Write("</select>");

                output.Write("</span>");

                return;
            }

            string id = String.Empty;
            string inputID = Sitecore.Web.UI.HtmlControls.Control.GetUniqueID("C");

            if (!String.IsNullOrEmpty(ID))
            {
                id = " id=\"" + ID + "\"";
                inputID = "Input_" + ID;
            }

            string click = String.Empty;

            if (!string.IsNullOrEmpty(Command))
            {
                click = " onchange=\"" + Sitecore.Context.ClientPage.GetClientEvent(Command) + "\"";
            }

            output.Write("<span" + id + " class=\"scRibbonToolbarSmallDropDownBox\"" + tooltip + accessKey + ">");

            if (!string.IsNullOrEmpty(KeyCode))
            {
                output.Write("<span class=\"scRibbonToolbarKeyCodeSmallButton\" style=\"display:none\">" +
                             Keyboard.GetKeyCodeText(KeyCode) + "</span>");
            }

            output.Write("<label for=\"" + inputID + "\">");

            image.Class = "scRibbonToolbarSmallDropDownBoxIcon";
            output.Write(image.ToString());

            output.Write(Header);

            output.Write("</label>");

            output.Write("<select id=\"" + inputID + "\" name=\"" + inputID +
                         "\" class=\"scRibbonToolbarSmallDropDownBoxInput\" " + click + ">");

            foreach (var item in Items)
            {
                output.Write("<option value=\"");
                output.Write(item["value"]);
                output.Write("\"");

                if (string.Compare(item["value"], value, StringComparison.InvariantCultureIgnoreCase) == 0)
                {
                    output.Write(" selected=\"selected\"");
                }

                output.Write(">");
                output.Write(item.DisplayName);
                output.Write("</option>");
            }

            output.Write("</select>");

            output.Write("</span>");
        }
    }
}
