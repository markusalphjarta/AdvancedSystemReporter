using Sitecore.Data.Items;

namespace ASR.DomainObjects
{

    public class ViewerItem : ReferenceItem
    {
        public ViewerItem(Item innerItem)
            : base(innerItem)
        {
        }
        public string ColumnsXml
        {
            get { return InnerItem["columns"]; }
        }
    }

}

