namespace ASR.DomainObjects
{
	public class Column
	{
		private string _name;
		public string Name
		{
			get
			{
				return _name;
			}
			set
			{
				_name = value.ToLower();
			}
		}
		public string Header;
	}
}
