using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Sitecore.Data.Items;
using Sitecore.Security.Accounts;

namespace ASR.DomainObjects
{
    public class ReferenceItem : BaseItem
	{
	    
        private readonly User _currentuser;

	    public ReferenceItem(Item innerItem) : base(innerItem)
	    {
	        _currentuser = Sitecore.Context.User;
	    }

	    
		public string Assembly
		{
		    get { return InnerItem["Assembly"]; }
		}
		
        public string Class
		{
            get { return InnerItem["Class"]; }
		}
		
		public string Attributes
		{
            get { return InnerItem["Attributes"]; }

		}

		public string ReplacedAttributes
		{
			get
			{
				string replacedAttributes = Attributes;
				foreach (var pi in Parameters)
				{
					string tag = string.Concat('{', pi.Name, '}');
					replacedAttributes = replacedAttributes.Replace(tag, pi.Value);
				}

                if (replacedAttributes.Contains("$"))
                {
                    replacedAttributes = Util.MakeDateReplacements(replacedAttributes);
                    replacedAttributes = replacedAttributes.Replace("$sc_currentuser", _currentuser.ToString()); 
                }
			    return replacedAttributes;
			}
		}
		public string FullType
		{
			get
			{
				if (!string.IsNullOrEmpty(Assembly))
				{
					return string.Concat(Class, ", ", Assembly);
				}
				return Class;
			}
		}

		public void SetAttributeValue(string tag, string value)
		{
			try
			{

				ParameterItem pi = Parameters.First(p => p.Name == tag);

				if (pi != null)
				{
					pi.Value = Uri.UnescapeDataString(value);
				}
			}
			// can't find element
			catch (InvalidOperationException)
			{
				// do nothing;
			}
		}

		public bool HasParameters
		{
			get
			{
				if (_parameters == null)
				{
					MakeParameterSet();
				}

				return _parameters.Count > 0;
			}
		}

		private HashSet<ParameterItem> _parameters;
		public IEnumerable<ParameterItem> Parameters
		{
			get
			{
				if (_parameters == null || _parameters.Count == 0)
				{
					MakeParameterSet();
				}
				return _parameters;
			}
		}

		private void MakeParameterSet()
		{
			_parameters = new HashSet<ParameterItem>();
			foreach (var tag in extractParameters(Attributes))
			{
				ParameterItem pi = FindItem(tag);
				if (pi != null)
				{
					_parameters.Add(pi);
				}
			}
		}

		private ParameterItem FindItem(string name)
		{
			string path = string.Concat(Settings.Instance.ParametersFolder, "/", name);

		    return new ParameterItem(InnerItem.Database.GetItem(path));
		}

		private IEnumerable<string> extractParameters(string st)
		{
			var tags = new List<string>();
			var r = new Regex(@"\{(\w*)\}");
			Match m = r.Match(st);

			while (m.Success)
			{
				if (m.Groups.Count == 2)
				{
					tags.Add(m.Groups[1].Value);
				}
				m = m.NextMatch();
			}
			return tags;
		}
	}
}
