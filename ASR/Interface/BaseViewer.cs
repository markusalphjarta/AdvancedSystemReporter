﻿using System;
using Sitecore.Diagnostics;
using ASR.Interface;
using Sitecore.Collections;
using System.Collections.Generic;
using ASR.DomainObjects;
using System.Xml;

namespace ASR.Interface
{
	public abstract class BaseViewer : BaseReportObject
	{
	    public string Sort { get; set; }

        #region Abstract methods

		public abstract void Display(DisplayElement dElement);

		#endregion		

        //internal static BaseViewer Create(string type, string parameters, string columnsXml)
        //{
        //    Assert.ArgumentNotNull(type, "type");
        //    Assert.ArgumentNotNull(parameters, "parameters");
        ////	BaseViewer oViewer = BaseViewer.Create(type);
        ////	oViewer.AddParameters(parameters);
        //    InitializeColumns(oViewer, columnsXml);
        //    //backwards compatibility
        //    if(oViewer.Columns.Count == 0)
        //    {
        //        InitializeColumnsOld( oViewer);
        //    }
        //    return oViewer;
        //}

        private static void InitializeColumnsOld(BaseViewer baseViewer)
	    {
	        var columns = baseViewer.GetParameter("columns");
	        if (!string.IsNullOrEmpty(columns))
	        {
	            var cols = columns.Split(',');
	            foreach (var col in cols)
	            {
	                var c = new Column()
	                               {
	                                   Name = col,
	                                   Header = col
	                               };
                    baseViewer.Columns.Add(c);
	            }
	        }
	    }

	    private static void InitializeColumns(BaseViewer oViewer, string columnsXml)
		{
			oViewer.Columns = new List<Column>();
			if (!string.IsNullOrEmpty(columnsXml))
			{
				XmlDocument doc = new XmlDocument();
				doc.LoadXml(columnsXml);
				XmlNodeList columnNodes = doc.DocumentElement.SelectNodes("Column");
				for (int i = 0; i < columnNodes.Count; i++)
				{
					Column column = new Column
					{
						Name = columnNodes[i].Attributes["name"].Value,
						Header = columnNodes[i].InnerText
					};
					oViewer.Columns.Add(column);
				}
			}
			
		}

		/// <summary>
		/// Gets the columns.
		/// </summary>
		/// <value>The columns.</value>
		public List<Column> Columns { get; protected set; }
	}
}
