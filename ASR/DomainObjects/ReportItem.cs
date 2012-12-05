using System;
using System.Collections.Generic;
using Sitecore.Data;
using System.Linq;
using ASR.Interface;
using Sitecore.Collections;
using Sitecore.Data.Fields;
using Sitecore.Data.Items;

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

        private HashSet<ReferenceItem> _objects;

        public ReferenceItem FindItem(string name)
        {
            if (_objects == null)
            {
                LoadObjects();
            }
            return _objects.First(ri => ri.Name == name);
        }

        public ReferenceItem FindItem(ID id)
        {
            if (_objects == null)
            {
                LoadObjects();
            }
            return _objects.First(ri => ri.ID == id);
        }

        private void LoadObjects()
        {
            _objects = new HashSet<ReferenceItem>();
            foreach (var item in Scanners) { _objects.Add(item); }
            foreach (var item in Viewers) { _objects.Add(item); }
            foreach (var item in Filters) { _objects.Add(item); }
        }

        public string SerializeParameters()
        {
            return SerializeParameters("^", "&");
        }

        public string SerializeParameters(string valueSeparator, string parameterSeparator)
        {
            var nvc = new NameValueCollection
                    {
                        {"id", Current.Context.ReportItem.ID.ToString()}
                    };
            foreach (var item in Current.Context.ReportItem.Scanners)
            {
                foreach (var p in item.Parameters)
                {
                    nvc.Add(string.Concat(item.ID, valueSeparator, p.Name), p.Value);
                }
            }
            foreach (var item in Current.Context.ReportItem.Filters)
            {
                foreach (var p in item.Parameters)
                {
                    nvc.Add(string.Concat(item.ID, valueSeparator, p.Name), p.Value);
                }
            }
            foreach (var item in Current.Context.ReportItem.Viewers)
            {
                foreach (var p in item.Parameters)
                {
                    nvc.Add(string.Concat(item.ID, valueSeparator, p.Name), p.Value);
                }
            }
            return StringUtil.NameValuesToString(nvc, parameterSeparator);
        }

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
                        ri.SetAttributeValue(itemParameter[1], nvc[key]);
                    }
                }
            }
            return reportItem;
        }

        public Report TransformToReport(Report report)
        {
            if (report == null)
            {
                report = new Report();
            }
            foreach (var sItem in Scanners)
            {
                report.AddScanner(sItem);
            }
            foreach (var vItem in Viewers)
            {
                report.AddViewer(vItem);
            }
            foreach (var fItem in Filters)
            {
                report.AddFilter(fItem);
            }
            return report;
        }
    }
}
