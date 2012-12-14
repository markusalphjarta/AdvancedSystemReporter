using Sitecore.Diagnostics;
using System.Collections;

namespace ASR.Interface
{
	public abstract class BaseScanner : BaseReportObject
	{
		public abstract ICollection Scan();		
	}
}
