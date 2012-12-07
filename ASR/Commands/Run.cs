using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Sitecore.Diagnostics;
using Sitecore.Shell.Framework.Commands;
using Sitecore.Web.UI.Sheer;
using Sitecore.Web.UI.WebControls;
using Sitecore.Web.UI.XamlSharp.Continuations;

namespace ASR.Commands
{
    public class Run : Command, ISupportsContinuation
    {
        public override void Execute(CommandContext context)
        {
            ContinuationManager.Current.Start(this, "Start", new ClientPipelineArgs(context.Parameters));
            
        }

        public void Start(ClientPipelineArgs args)
        {
            var parameters = args.Parameters["parameters"];
            
            //get parameters from the ui
            //Sitecore.Context.ClientPage.SendMessage(this, "ASR.MainForm:updateparameters");
            // var reportItem = new ReportItem(Client.ContentDatabase.GetItem(args.Parameters["reportid"]));
            Log.Info("RJ +" + parameters.Aggregate((a,c)=> a+= c), this);
            // var report = reportItem.TransformToReport(null);
            //if (Current.Context.Report == null)
            //{
            //  Current.Context.Report = new Report();
            //}
            //foreach (var sItem in Current.Context.ReportItem.Scanners)
            //{
            //  Current.Context.Report.AddScanner(sItem);
            //}
            //foreach (var vItem in Current.Context.ReportItem.Viewers)
            //{
            //  Current.Context.Report.AddViewer(vItem);
            //}
            //foreach (var fItem in Current.Context.ReportItem.Filters)
            //{
            //  Current.Context.Report.AddFilter(fItem);
            //}

            //Sitecore.Shell.Applications.Dialogs.ProgressBoxes.ProgressBox.Execute(
            //    "Scanning...",
            //    "Scanning items",
            //    "",
            //     Current.Context.Report.Run,
            //    "MainForm:runfinished",
            //    new object[] { });
        }
        public override CommandState QueryState(CommandContext context)
        {
            if (context.Parameters["reportid"] == null)
            {
                return CommandState.Disabled;
            }

            return base.QueryState(context);
        }
    }
}
