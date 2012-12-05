using System.Collections.Generic;
using System.Collections.Specialized;
using System.Web.UI.WebControls;
using ASR.DomainObjects;
using ASR.Interface;
using ComponentArt.Web.UI;
using Sitecore;
using Sitecore.Data;
using Sitecore.Data.Items;
using Sitecore.Diagnostics;
using Sitecore.Extensions;
using Sitecore.Security.Accounts;
using Sitecore.Shell.Framework.Commands;
using Sitecore.Web;
using Sitecore.Web.UI;
using Sitecore.Web.UI.Grids;
using Sitecore.Web.UI.Sheer;
using Sitecore.Web.UI.WebControls;
using Sitecore.Web.UI.WebControls.Ribbons;
using Sitecore.Web.UI.XamlSharp.Ajax;
using Image = System.Web.UI.WebControls.Image;
using System.Linq;

using Sitecore.Extensions.StringExtensions;

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
       //     this.DataGrid.ItemDataBound += new Grid.ItemDataBoundEventHandler(this.Users_ItemDataBound);
            this.DataGrid.ItemContentCreated += new Grid.ItemContentCreatedEventHandler(this.UsersItemContentCreated);
            Client.AjaxScriptManager.OnExecute += new AjaxScriptManager.ExecuteDelegate(Current_OnExecute);           
        }      

        protected override void OnLoad(EventArgs e)
        {
            Assert.ArgumentNotNull(e, "e");
            base.OnLoad(e);

            

            if (!IsPostBack)
            {
              //  var testqs = "id={05F200B8-27F0-4B11-8FDB-088BE6554782}&e12d5fd8-1296-466f-ac59-b808e47bf1e7^Age=-1";
              if (Current.Context.ReportItem != null)
              {
                //  RunReport();
                //  PopulateGrid();
              }
            }
        }

        private static void RunReport()
        {
            Current.Context.Report =
                Current.Context.ReportItem.TransformToReport(Current.Context.Report);
            Current.Context.Report.Run();
        }

        private void PopulateGrid()
        {
            var results = Current.Context.Report.GetResultElements();

            var first = results.FirstOrDefault();
            DataGrid.Levels[0].Columns.Clear();

            DataGrid.Levels[0].Columns.Add(new GridColumn() {Visible = false, IsSearchable = false, DataField = "scGridID"});
            DataGrid.Levels[0].Columns.Add(new GridColumn()
                {
                    Visible = true,
                    IsSearchable = false,
                    DataField = "Icon",
                    DataCellServerTemplateId = "IconTemplate"
                });

            if (first != null)
            {
                foreach (var cName in first.GetColumnNames())
                {
                    DataGrid.Levels[0].Columns.Add(new GridColumn()
                        {
                            DataField = cName,
                            DataCellServerTemplateId = "CommonTemplate"
                        });
                }
            }
            //   var managedUsers = Sitecore.Context.User.Delegation.GetManagedUsers();

            ComponentArtGridHandler<DisplayElement>.Manage(this.DataGrid, new GridSource<DisplayElement>(results),
                                                           this.RebindRequired);
            this.DataGrid.LocalizeGrid();
        }


        public CommandContext GetCommandContext()
        {
            CommandContext context = new CommandContext();

            var itemNotNull = Client.CoreDatabase.GetItem("/sitecore/content/Applications/Advanced System Reporter/Ribbon");
            context.RibbonSourceUri = itemNotNull.Uri;

            context.Parameters["reportname"] = Current.Context.ReportItem != null ? Current.Context.ReportItem.Name : "noname";
            context.Parameters["reportid"] = reportid; 
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
        }

        private void UsersItemContentCreated(object sender, GridItemContentCreatedEventArgs e)
        {
            
            Assert.ArgumentNotNull(sender, "sender");
            Assert.ArgumentNotNull(e, "e");

            if(e.Column.DataField=="Icon")
            {
                var icon = e.Content.FindControl("LabelIcon") as Label;
                if (icon != null)
                {
                    icon.Text = Sitecore.Resources.Images.GetImage(e.Item["Icon"].ToString(), ImageDimension.id16x16);
                }    
            }
            else
            {
                var label2 = e.Content.FindControl("CommonLabel") as Label;
                if (label2 != null)
                {
                    var dElement = e.Item.DataItem as DisplayElement;
                    if(dElement != null)
                    {
                        var columnValue = dElement.GetColumnValue(e.Column.DataField);
                        columnValue = columnValue.Substring(0, Math.Min(50, columnValue.Length));
                        label2.Text = HttpUtility.HtmlEncode(columnValue);
                    }
                }    
            }                                    
        }

        private string reportid; 
        private  void Current_OnExecute([NotNull] object sender, [NotNull] AjaxCommandEventArgs args)
        {
            Assert.ArgumentNotNull(sender, "sender");
            Assert.ArgumentNotNull(args, "args");
       
            if (args.Name == "asr:refreshgrid")
            {
              reportid = args.Parameters["id"];
              
              //  RunReport();
              //  PopulateGrid();
                
                SheerResponse.Redraw();
                SheerResponse.SetInnerHtml("RibbonContainer", HtmlUtil.RenderControl(Ribbon));
              SheerResponse.Eval("refresh()");
              
            }
        }
    }
}
