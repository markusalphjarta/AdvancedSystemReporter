﻿using System.Collections.Generic;
using System.Linq;
using System;
using System.Collections;
using System.Collections.Specialized;
using Sitecore;
using ASR.DomainObjects;

namespace ASR.Interface
{
	public class Report
	{
		private readonly Dictionary<string, FilterItem> filters;
		private readonly Dictionary<string, ScannerItem> scanners;
		private readonly Dictionary<string, ViewerItem> viewers;

		// buffer for the result of the scanner
		private ArrayList _scannerResults;

		// buffer for filtered results i.e. scannerResults => Filter()
		private ArrayList _results;

		IEnumerable<BaseViewer> _viewers;
		public IEnumerable<BaseViewer> Viewers
		{
			get {
			    return _viewers ??
			           (_viewers =
			            viewers.Select(v => BaseViewer.Create(v.Value.FullType, v.Value.ReplacedAttributes, v.Value.ColumnsXml))
			                   .ToList());
			}
		}

		private List<DisplayElement> _displayElements;
		/// <summary>
		/// Gets or sets the display elements.
		/// </summary>
		/// <value>The display elements.</value>
		public List<DisplayElement> DisplayElements
		{
			get
			{
				if (_displayElements == null)
				{
					var tmp = _results.OfType<object>().Select(IntializeDisplayElement);
					tmp = Sort(tmp);
					_displayElements = tmp.ToList();
				}
				return _displayElements;
			}
			set { _displayElements = value; }
		}

		private IEnumerable<DisplayElement> Sort(IEnumerable<DisplayElement> tmp)
		{
            // if no columns to sort by, then just return the tmp variable
            if (SortColumns == null || SortColumns.Count == 0)
            {
                return tmp;
            }

            IOrderedEnumerable<DisplayElement> sortedList = null;

            bool isFirstTimeThrough = true;

            foreach (string columnName in SortColumns.Keys)
            {
                Func<DisplayElement, object> columnValue;
                string sortOptions = SortColumns[columnName];
                // need to copy the column name, or it executes against the wrong column when we actually do the sort
                string copyOfColumnName = columnName;

                // sort datetime values separately
                if (sortOptions.Contains("DateTime"))
                {
                    columnValue = t => ParseDate(t.GetColumnValue(copyOfColumnName));
                }
                else
                {
                    columnValue = t => t.GetColumnValue(copyOfColumnName);
                }

                // and sort based on order
                if (sortOptions.Contains("ASC"))
                {
                    sortedList = (isFirstTimeThrough) ? tmp.OrderBy(columnValue):sortedList.ThenBy(columnValue);
                    isFirstTimeThrough = false;
                }
                else if (sortOptions.Contains("DESC"))
                {
                    sortedList = (isFirstTimeThrough) ? tmp.OrderByDescending(columnValue):sortedList.ThenByDescending(columnValue);
                    isFirstTimeThrough = false;
                }
            }
            return sortedList;

		}

		private object ParseDate(string value)
		{
			if (!string.IsNullOrEmpty(value))
			{
				if (DateUtil.IsIsoDate(value))
				{
					return DateUtil.IsoDateToDateTime(value);
				}
			    DateTime result;
			    DateTime.TryParse(value, out result);
			    return result;
			}
			return null;
		}

		private DisplayElement IntializeDisplayElement(object resultItem)
		{
			var dElement = new DisplayElement(resultItem);
			foreach (var oViewer in Viewers)
			{
				oViewer.Display(dElement);
			}
			return dElement;
		}

		public Report()
		{
			scanners = new Dictionary<string, ScannerItem>();
			viewers = new Dictionary<string, ViewerItem>();
			filters = new Dictionary<string, FilterItem>();
		}

	    /// <summary>
	    /// Add a filter to the report
	    /// </summary>
	    public void AddFilter(FilterItem filter)
		{
			if (!filters.ContainsKey(filter.ID.ToString()))
			{
				filters.Add(filter.ID.ToString(), filter);
				FlushFilterCache();
			}
		}

	    /// <summary>
	    /// Add a scanner to the report
	    /// </summary>
	    /// <param name="scanner"></param>
	    public void AddScanner(ScannerItem scanner)
		{
			Sitecore.Diagnostics.Assert.ArgumentNotNull(scanner, "scanner");

			if (!scanners.ContainsKey(scanner.ID.ToString()))
			{
				scanners.Add(scanner.ID.ToString(), scanner);
				FlushCache();
			}
		}

		/// <summary>
		/// Add a viewer to the report
		/// </summary>
		/// <param name="viewer"></param>
		public void AddViewer(ViewerItem viewer)
		{
			Sitecore.Diagnostics.Assert.ArgumentNotNull(viewer, "viewer");

			if (!viewers.ContainsKey(viewer.ID.ToString()))
			{
				viewers.Add(viewer.ID.ToString(), viewer);
			}
		}

		/// <summary>
		/// flush the buffer for the scanner, so next time the report runs, scanner will be queried again.
		/// </summary>
		public void FlushCache()
		{
			_scannerResults = null;
		}

		/// <summary>
		/// Flush the cache for the filtered elements, next time the filters will be called again.
		/// </summary>
		private void FlushFilterCache()
		{
			_results = null;
		}

		/// <summary>
		/// Run the report. It will only requery the Scanner if FlushCache has been used before calling this method.
		/// </summary>
		/// <param name="parameters">Not used. Only so it can be called as a ProgressBoxMethod delegate</param>
		public void Run(params object[] parameters)
		{
			if (_scannerResults == null)
			{
				_scannerResults = new ArrayList();
				foreach (var scanner in scanners)
				{
					BaseScanner oScanner = BaseScanner.Create(scanner.Value.FullType, scanner.Value.ReplacedAttributes);
					//TODO throw exception if null??
					_scannerResults.AddRange(oScanner.Scan());
				}
			}
			Filter();			
		}

		/// <summary>
		/// Filters the results. All filters are "and" so an item needs to pass all filters to appear on the report.
		/// </summary>
		public void Filter()
		{
			IEnumerable<BaseFilter> oFilters = filters.Select(p => BaseFilter.Create(p.Value.FullType, p.Value.ReplacedAttributes)).ToList();
			_results = new ArrayList();

			foreach (var element in _scannerResults)
			{
				var add = oFilters.All(filter => filter.Filter(element));
			    if (add)
				{
					Sitecore.Diagnostics.Assert.IsNotNull(element, "element is null ");
					_results.Add(element);
				}
			}
		}

		/// <summary>
		/// Gets the results in this report. Will call Run() if it hasn't been called yet.
		/// </summary>
		/// <returns>All the results from running the report</returns>
		public IEnumerable<DisplayElement> GetResultElements()
		{
			if (_results == null)
				Run();
			return GetResultElements(0, _results.Count);
		}

		/// <summary>
		/// Gets only a selection of the results elements. Useful for paging.
		/// </summary>
		/// <param name="start">first result to return</param>
		/// <param name="count">number of results to return</param>
		/// <returns>Results of the report</returns>
		public IEnumerable<DisplayElement> GetResultElements(int start, int count)
		{
			int end = Math.Min(start + count, ResultsCount());

			return DisplayElements.Where((t, i) => (start <= i) && (i < end));
		}

		/// <summary>
		/// Total number of results.
		/// </summary>
		/// <returns>Number of results or -1 if report has not been run yet.</returns>
		public int ResultsCount()
		{
			if (_results == null)
				return -1;
			return _results.Count;
		}

	  
		private NameValueCollection _sortColumns;
		protected NameValueCollection SortColumns
		{
			get
			{
				if (_sortColumns == null)
				{ 
                    _sortColumns = new NameValueCollection();
					foreach (var viewer in Viewers)
					{

					    string sortParameter = viewer.Sort;
						if (sortParameter != null)
						{
							_sortColumns.Add( StringUtil.ParseNameValueCollection(sortParameter, '&', ',') );
						}
					}
				}
				return _sortColumns;
			}
		}
	}
}
