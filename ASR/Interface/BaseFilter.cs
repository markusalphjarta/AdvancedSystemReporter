using Sitecore.Diagnostics;

namespace ASR.Interface
{
	public abstract class BaseFilter : BaseReportObject
	{
		public abstract bool Filter(object element);		
	}
}
