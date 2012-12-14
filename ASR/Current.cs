using System.Web;

namespace ASR
{
    internal static class Current
    {
        internal static ASR.Context Context
        {
            get
            {
                if (HttpContext.Current.Application["ASR.Context"] == null)
                {
                    ASR.Context c = new ASR.Context();
                    HttpContext.Current.Application["ASR.Context"] = c;
                }
                return (ASR.Context)HttpContext.Current.Application["ASR.Context"];
            }

            private set
            {
                HttpContext.Current.Application["ASR.Context"] = value;
            }

        }

        internal static void ClearContext()
        {
            Context = null;
        }

    }
}
