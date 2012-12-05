using Sitecore.Data.Items;

namespace ASR.DomainObjects
{
    public class BaseItem : CustomItem
    {
        public BaseItem(Item innerItem)
            : base(innerItem)
        {
        }

        public string Path { get { return InnerItem.Paths.FullPath; } }
    }
}
