using ASR.DomainObjects;
using Sitecore.Shell.Framework.Commands;
using Sitecore.Web.UI.Sheer;
using Sitecore.Data.Items;
using Sitecore.Data;
using Sitecore.Web.UI.WebControls;
using Sitecore.Web.UI.XamlSharp.Continuations;

namespace ASR.Commands
{
    class Open : Command, ISupportsContinuation
    {
        public override void Execute(CommandContext context)
        {
             ContinuationManager.Current.Start(this, "Start");
        }

        public void Start(ClientPipelineArgs args)
        {

            if (!args.IsPostBack)
            {
                Util.ShowItemBrowser(
                    "Select the report",
                    "Select the report",
                    "Database/32x32/view_h.png",
                    "Select",
                    Current.Context.Settings.ReportsFolder,
                    Current.Context.ReportItem == null ? Current.Context.Settings.ReportsFolder : Current.Context.ReportItem.Path,
                    Current.Context.Settings.ConfigurationDatabase);
                args.WaitForPostBack();
            }
            else
            {
                if (!Sitecore.Data.ID.IsID(args.Result))
                {
                    return;
                }
                Database database = Sitecore.Configuration.Factory.GetDatabase(Current.Context.Settings.ConfigurationDatabase);
                Sitecore.Diagnostics.Assert.IsNotNull(database,"no configuration databsae");

                Item item = database.GetItem(args.Result);

                Sitecore.Diagnostics.Assert.IsNotNull(item, "report item cannot be loaded");

                switch(item.Template.Key)
                {
                    case "report":

                    //if (rItem != null)
                    {
                        Current.Context.ReportItem = new ReportItem(item);                        
                        AjaxScriptManager.Current.Dispatch("asr:refreshgrid");
                    }
                        break;
                   
                    case "saved report":
                
                    Message m = Message.Parse(this, "ASR.MainForm:openlink");
                    System.Collections.Specialized.NameValueCollection nvc = 
                    Sitecore.StringUtil.ParseNameValueCollection(item["parameters"], '&', '=');

                    m.Arguments.Add(nvc);
                    Sitecore.Context.ClientPage.SendMessage(m);
                    break;
                }

               
            }
        }

    }
}
