using System;
using System.Collections.Specialized;
using System.Reflection;
using ASR.DomainObjects;
using Sitecore.Diagnostics;


namespace ASR.Interface
{
	public abstract class BaseReportObject
	{
        private NameValueCollection _parameters;
		
        public string Name { get; set; }

		protected string GetParameter(string name)
		{
		    return _parameters == null ? null : _parameters[name];
		}
	}
}
