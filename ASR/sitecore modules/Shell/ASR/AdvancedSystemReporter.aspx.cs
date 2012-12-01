using System.Collections.Generic;
using System.Web.UI.WebControls;
using ComponentArt.Web.UI;
using Sitecore;
using Sitecore.Diagnostics;
using Sitecore.Extensions;
using Sitecore.Security.Accounts;
using Sitecore.Shell.Framework.Commands;
using Sitecore.Web.UI.Grids;
using Sitecore.Web.UI.WebControls;

namespace ASR.sitecore_modules.Shell.ASR
{
    using System;
    using System.Web;
    using System.Web.UI;

    public partial class AdvancedSystemReporter : Page, IHasCommandContext
    {
        private bool RebindRequired
        {
            get
            {
                return ((!this.Page.IsPostBack && (base.Request.QueryString["Cart_Users_Callback"] != "yes")) || (this.Page.Request.Params["requireRebind"] == "true"));
            }
        }
 

        protected override void OnInit(EventArgs e)
        {
            Assert.ArgumentNotNull(e, "e");
            base.OnInit(e);
            Sitecore.Context.State.DataBind = false;
            //this.DataGrid.ItemDataBound += new Grid.ItemDataBoundEventHandler(this.Users_ItemDataBound);
            this.DataGrid.ItemContentCreated += new Grid.ItemContentCreatedEventHandler(this.UsersItemContentCreated);
            //Client.AjaxScriptManager.OnExecute += new AjaxScriptManager.ExecuteDelegate(Sitecore.Shell.Applications.Security.UserManager.UserManager.Current_OnExecute);
        }

        protected override void OnLoad(EventArgs e)
        {
            Assert.ArgumentNotNull(se, "e");
            base.OnLoad(e);
            Assert.CanRunApplication("Security/User Manager");
            IEnumerable<User> managedUsers = Sitecore.Context.User.Delegation.GetManagedUsers();
            ComponentArtGridHandler<User>.Manage(this.DataGrid, new GridSource<User>(managedUsers), this.RebindRequired);
            this.DataGrid.LocalizeGrid();
        }

        public CommandContext GetCommandContext()
        {
            CommandContext context = new CommandContext();

            var itemNotNull = Client.GetItemNotNull("/sitecore/content/Applications/Advanced System Reporter/Ribbon", Client.CoreDatabase);
            context.RibbonSourceUri = itemNotNull.Uri;
            //string selectedValue = GridUtil.GetSelectedValue("Users");
            //string str2 = string.Empty;
            //ListString str3 = new ListString(selectedValue);
            //if (str3.Count > 0)
            //{
            //    str2 = str3[0].Split(new char[] { '^' })[0];
            //}
            //context.Parameters["username"] = selectedValue;
            //context.Parameters["domainname"] = SecurityUtility.GetDomainName();
            //context.Parameters["accountname"] = str2;
            //context.Parameters["accounttype"] = AccountType.User.ToString();
            return context;



            return context;
        }

        private void UsersItemContentCreated(object sender, GridItemContentCreatedEventArgs e)
        {
            Assert.ArgumentNotNull(sender, "sender");
            Assert.ArgumentNotNull(e, "e");
            var label = e.Content.FindControl("FullNameLabel") as Label;
            if (label != null)
            {
                label.Text = HttpUtility.HtmlEncode(e.Item["Profile.FullName"].ToString());
            }
            var label2 = e.Content.FindControl("CommentLabel") as Label;
            if (label2 != null)
            {
                label2.Text = HttpUtility.HtmlEncode((e.Item["Profile.Comment"] == null) ? string.Empty : e.Item["Profile.Comment"].ToString());
            }
        }

 

 

    }
}
