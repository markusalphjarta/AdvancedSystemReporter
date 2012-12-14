using System;
using System.Collections.Generic;
using Sitecore.Data;
using System.Linq;
using ASR.Interface;
using Sitecore.Collections;
using Sitecore.Data.Fields;
using Sitecore.Data.Items;
using Sitecore.Shell.Applications.Dialogs.ProgressBoxes;
using Sitecore.Web;


namespace ASR.DomainObjects
{
    using System.Collections.Specialized;   
    using Sitecore;
    using Sitecore.Diagnostics;

    [Serializable]

    public class ReportItem : BaseItem
    {
        public ReportItem(Item innerItem)
            : base(innerItem)
        {
        }

        public static ReportItem Create(string reportid)
        {
            var item = Client.ContentDatabase.GetItem(reportid);
            Assert.IsNotNull(item, "can create item {0}", reportid);
            return new ReportItem(item);
        }

        public IEnumerable<ScannerItem> _scanners;
        public IEnumerable<ScannerItem> Scanners
        {
            get
            {
                if (_scanners == null)
                {
                    MultilistField scanners = InnerItem.Fields["Scanners"];
                    _scanners = scanners.GetItems().Select(i => new ScannerItem(i));
                }
                return _scanners;
            }
        }

        private IEnumerable<ViewerItem> _viewers;
        public IEnumerable<ViewerItem> Viewers
        {
            get
            {
                if (_viewers == null)
                {
                    MultilistField viewers = InnerItem.Fields["Viewers"];
                    _viewers = viewers.GetItems().Select(i => new ViewerItem(i));
                }
                return _viewers;
            }
        }

        private IEnumerable<CommandItem> _commands;
        public IEnumerable<CommandItem> Commands
        {

            get
            {
                if (_commands == null)
                {
                    MultilistField commands = InnerItem.Fields["Commands"];
                    _commands = commands.GetItems().Select(i => new CommandItem(i));
                }
                return _commands;
            }
        }

        private IEnumerable<FilterItem> _filters;
        public IEnumerable<FilterItem> Filters
        {
            get
            {
                if (_filters == null)
                {
                    MultilistField filters = InnerItem.Fields["Filters"];
                    _filters = filters.GetItems().Select(i => new FilterItem(i));

                }
                return _filters;
            }
        }

        public string EmailText
        {
            get { return InnerItem["email text"]; }

        }

        public void RunCommand(string commandname, StringList values)
        {
            foreach (var item in Commands)
            {
                if (item.Name == commandname)
                {
                    item.Run(values);
                    break;
                }
            }
        }               

        public ReferenceItem FindItem(string name)
        {
            return
                Scanners.Cast<ReferenceItem>().FirstOrDefault(ri => ri.Name == name)
                ?? Filters.Cast<ReferenceItem>().FirstOrDefault(ri => ri.Name == name)
                ?? Viewers.Cast<ReferenceItem>().FirstOrDefault(ri => ri.Name == name);
        }

        public ReferenceItem FindItem(ID id)
        {
            return
                Scanners.Cast<ReferenceItem>().FirstOrDefault(ri => ri.ID == id)
                ?? Filters.Cast<ReferenceItem>().FirstOrDefault(ri => ri.ID == id)
                ?? Viewers.Cast<ReferenceItem>().FirstOrDefault(ri => ri.ID == id);
        }

       
        public string SerializeParameters()
        {
            return SerializeParameters("^", "&");
        }

        public string SerializeParameters(string valueSeparator, string parameterSeparator)
        {
            throw new NotImplementedException();
            //var nvc = new NameValueCollection
            //        {
            //            {"id", ID.ToString()}
            //        };
            //foreach (var item in Scanners)
            //{
            //    foreach (var p in item.Parameters)
            //    {
            //        nvc.Add(string.Concat(item.ID, valueSeparator, p.Name), p.Value);
            //    }
            //}
            //foreach (var item in Filters)
            //{
            //    foreach (var p in item.Parameters)
            //    {
            //        nvc.Add(string.Concat(item.ID, valueSeparator, p.Name), p.Value);
            //    }
            //}
            //foreach (var item in Viewers)
            //{
            //    foreach (var p in item.Parameters)
            //    {
            //        nvc.Add(string.Concat(item.ID, valueSeparator, p.Name), p.Value);
            //    }
            //}
            //return StringUtil.NameValuesToString(nvc, parameterSeparator);
        }

        //public void MakeReplacements(NameValueCollection parameters)
        //{            
        //        Scanners.Cast<ReferenceItem>()
        //                .Concat(Filters)
        //                .Concat(Viewers)
        //                .ForEach(o => o.ReplaceAttributes(parameters));                        
        //}

        public static ReportItem CreateFromParameters(string parameters)
        {
            Assert.ArgumentNotNullOrEmpty(parameters, "parameters");
            var nvc = StringUtil.ParseNameValueCollection(parameters, '&', '=');
            return CreateFromParameters(nvc);
        }

        public static ReportItem CreateFromParameters(NameValueCollection nvc)
        {
            Assert.IsNotNull(nvc, "Incorrect Parameters Format");
            var id = nvc["id"];
            if (id == null) return null;            
            
            var reportItem = new ReportItem(Client.ContentDatabase.GetItem(id));
            if (reportItem == null) throw new Exception("Report has been deleted");

            foreach (string key in nvc.Keys)
            {
                if (key.Contains("^"))
                {
                    var itemParameter = key.Split('^');
                    var guid = new ID(itemParameter[0]);

                    var ri = reportItem.FindItem(guid);
                    if (ri != null)
                    {
                        ri.ReplaceAttributes(WebUtil.ParseUrlParameters(nvc[key],'|'));
                    }
                }
            }
            return reportItem;
        }

        public IEnumerable<DisplayElement> Results { get; set; }

        public HashSet<string> Columns
        {
            get; set;
        }

        public void Run(params object[] parameters)
        
        {
            var context = parameters[0] as ASR.Context;
            if(context == null) throw new NotImplementedException(); //TODO 
            var replacements = context.Parameters;
        
            var scanners = Scanners.Select(s => s.MakeObject(replacements)).Cast<BaseScanner>();
            var filters = Filters.Select(f => f.MakeObject(replacements)).Cast<BaseFilter>();
            var viewers = Viewers.Select(v => v.MakeObject(replacements)).Cast<BaseViewer>();        

            Results = scanners
                .SelectMany(s => s.Scan().Cast<object>())
                .Where(o => filters.All(f => f.Filter(o)))
                .Select(o => new DisplayElement(o,this)).ToList();

            Columns = new HashSet<string>();
            Results.ForEach(d => viewers.ForEach(v => v.Display(d)));                        
        }
    }
}
