using System.Globalization;
using System.Collections.Generic;
using System;
using System.Collections.Specialized;
using Sitecore.Data.Items;
using Sitecore.Web.UI.HtmlControls;
using System.Linq;

namespace ASR.DomainObjects
{	
	public class ParameterItem : CustomItem
	{
	    public ParameterItem(Item innerItem) : base(innerItem)
	    {
	    }
	    
		public string Title
		{
            get { return InnerItem["Title"]; }
		}
		
		public string Type
		{
		    get { return InnerItem["Type"]; }		    
		}

		
		private string DefaultValue {
            get { return InnerItem["default value"]; }
		}

	    
		public string Value
		{
		    get
			{
				string replacedValue = DefaultValue;
				if (!string.IsNullOrEmpty(replacedValue))
				{
					replacedValue = replacedValue.Replace("$sc_lastyear", DateTime.Today.AddYears(-1).ToString("yyyyMMddTHHmmss"));
					replacedValue = replacedValue.Replace("$sc_lastweek", DateTime.Today.AddDays(-7).ToString("yyyyMMddTHHmmss"));
					replacedValue = replacedValue.Replace("$sc_lastmonth", DateTime.Today.AddMonths(-1).ToString("yyyyMMddTHHmmss"));
					replacedValue = replacedValue.Replace("$sc_yesterday", DateTime.Today.AddDays(-1).ToString("yyyyMMddTHHmmss"));
					replacedValue = replacedValue.Replace("$sc_today", DateTime.Today.ToString("yyyyMMddTHHmmss"));
					replacedValue = replacedValue.Replace("$sc_now", DateTime.Now.ToString("yyyyMMddTHHmmss"));
					replacedValue = replacedValue.Replace("$sc_currentuser", Sitecore.Context.User == null ? string.Empty : Sitecore.Context.User.Name);
				}
				return replacedValue;
			}
            //set
            //{
            //    DefaultValue = value;
            //}
		    set { throw new NotImplementedException(); }
		}


	    private string parameters { get { return InnerItem["parameters"]; } }

        private NameValueCollection _params;
        public NameValueCollection Parameters
        {
            get
            {
                if (_params == null)
                {
                    //_params = Sitecore.StringUtil.ParseNameValueCollection(parameters, '|', '=');
                    _params = new NameValueCollection();
                    string[] substrings = parameters.Split('|');
                    foreach (var st in substrings)
                    {
                        int i = st.IndexOf('=');
                        if (i > 0)
                        {
                            _params.Add(st.Substring(0, i), st.Substring(i+1));
                        }
                    }
                }
                return _params;
            }
        }

		public IEnumerable<ValueItem> PossibleValues()
		{
		    return InnerItem.Children.Select(i=>new ValueItem(i));
		}

        public Control MakeControl()
        {
            Control input = null;
            if (Type == "Text")
            {
                input = new Edit {ID = Control.GetUniqueID("input")};
            }
            else if (Type == "Dropdown")
            {
                var c = new Combobox();
                foreach (var value in PossibleValues())
                {
                    var li = new ListItem {Header = value.Name, Value = value.Value};
                    c.Controls.Add(li);
                }
                input = c;
                input.ID = Control.GetUniqueID("input");
            }
            else if (Type == "Item Selector")
            {
                var iSelect = new Controls.ItemSelector();
                input = iSelect;
                input.ID = Control.GetUniqueID("input");
                iSelect.Click = string.Concat("itemselector", ":", input.ID);
                if (Parameters["root"] != null) iSelect.Root = Parameters["root"];
                if (Parameters["folder"] != null) iSelect.Folder = Parameters["folder"];
                if (Parameters["displayresult"] != null) iSelect.DisplayValueType = (Controls.ItemInfo)Enum.Parse(typeof(Controls.ItemInfo), Parameters["displayresult"]);
                if (Parameters["valueresult"] != null) iSelect.ValueType = (Controls.ItemInfo)Enum.Parse(typeof(Controls.ItemInfo), Parameters["valueresult"].ToString(CultureInfo.InvariantCulture));
                if (Parameters["filter"] != null) iSelect.Filter = Parameters["filter"];
            }
            else if (Type == "User Selector")
            {
                var uSelect = new Controls.UserSelector();
                input = uSelect;
                input.ID = Control.GetUniqueID("input");
                uSelect.Click = string.Concat("itemselector", ":", input.ID);
                if (Parameters["filter"] != null) uSelect.Filter = Parameters["filter"];
            }
            else if (Type == "Date picker")
            {
                var dtPicker = new Controls.DateTimePicker();
                dtPicker.Style.Add("float", "left");
                dtPicker.ID = Control.GetUniqueID("input");
                dtPicker.ShowTime = false;
                dtPicker.Click = "datepicker" + ":" + dtPicker.ID;
                dtPicker.Style.Add(System.Web.UI.HtmlTextWriterStyle.Display, "inline");
                dtPicker.Style.Add(System.Web.UI.HtmlTextWriterStyle.VerticalAlign, "middle");
                if (Parameters["Format"] != null) dtPicker.Format = Parameters["Format"];
                input = dtPicker;
            }
            //input.ID = Control.GetUniqueID("input");
            input.Value = Value;
            return input;
        }

	}
}
