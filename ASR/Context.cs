using System.Collections.Generic;
using System.Collections.Specialized;
using ASR.Interface;

namespace ASR
{
	public class Context
	{
		internal Context()
		{
		    //Results = new List<DisplayElement>();
		}

		public DomainObjects.ReportItem ReportItem { get; set; }

      //  public List<DisplayElement> Results { get; private set; }

	    public NameValueCollection Parameters { get; set; }
		

		public string Name { get; set; }

		public Settings Settings { get { return Settings.Instance; } }
	}
}
