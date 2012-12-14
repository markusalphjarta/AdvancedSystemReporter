using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ASR.DomainObjects;
using ASR.Interface;
using Sitecore.Diagnostics;
using Sitecore.Shell.Framework.Commands;
using Sitecore.Web;
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

            var parameters = WebUtil.ParseUrlParameters(args.Parameters["parameters"] ?? string.Empty);

            Current.Context.Parameters = parameters;

            Current.Context.ReportItem.Run(Current.Context);

            
            AjaxScriptManager.Current.Dispatch("asr:runfinished");

            //Sitecore.Shell.Applications.Dialogs.ProgressBoxes.ProgressBox.Execute(
            //    "Scanning...",
            //    "Scanning items",
            //    "",
            //     reportItem.Run,
            //    "asr:runfinished",
            //    new object[] {parameters, Current.Context.Results});
        }



        public override CommandState QueryState(CommandContext context)
        {
            return Current.Context.ReportItem == null ? CommandState.Disabled : base.QueryState(context);
        }
    }
}
