using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Reflection;
using System.Text.RegularExpressions;
using Sitecore.Data.Items;
using Sitecore.Diagnostics;
using Sitecore.Web;
using System.Linq;

namespace ASR.DomainObjects
{
    public class ReferenceItem : BaseItem
    {
        public ReferenceItem(Item innerItem)
            : base(innerItem)
        {
        }

        #region Item fields
        /// <summary>
        /// Assembly field
        /// </summary>
        public string Assembly
        {
            get { return InnerItem["Assembly"]; }
        }

        /// <summary>
        /// Class field
        /// </summary>
        public string Class
        {
            get { return InnerItem["Class"]; }
        }

        public string Attributes { get { return InnerItem["Attributes"]; } }

        private NameValueCollection _attributes;
        /// <summary>
        /// Attributes field as a NameValueCollection
        /// </summary>
        public NameValueCollection AttributesCollection
        {
            get
            {
                return _attributes = _attributes ?? WebUtil.ParseUrlParameters(InnerItem["Attributes"], '|');
            }
        }

        private IEnumerable<ParameterItem> _parameters;
        /// <summary>
        /// Parameters used in the attributes for this item
        /// </summary>
        public IEnumerable<ParameterItem> Parameters
        {
            get
            {
                return _parameters ?? (_parameters =
                    ExtractParameters(Attributes).Select(ParameterItem.CreateParameter));
            }
        }

        #endregion

        /// <summary>
        /// Concatenatiion of Class and Assembly fields
        /// </summary>
        public string FullType
        {
            get
            {
                return !string.IsNullOrEmpty(Assembly) ? string.Concat(Class, ", ", Assembly) : Class;
            }
        }


        /// <summary>
        /// Makes any macro replacements in the attributes with the key/values supplied.
        /// Macros are defined as {Key}
        /// </summary>
        /// <param name="replacements">Key/Values to replace</param>
        public void ReplaceAttributes(NameValueCollection replacements)
        {
            var r = new Regex(@"\{(\w*)\}");
            foreach (var key in AttributesCollection.AllKeys)
            {
                var attribute = AttributesCollection[key];
                attribute = r.Replace(attribute, m => replacements[m.Groups[1].Value] ?? string.Empty);

                if (attribute.Contains("$"))
                {
                    Util.MakeDateReplacements(ref attribute);
                }
                AttributesCollection[key] = attribute;
            }
        }

        /// <summary>
        /// Parses the string supplied and finds the parameteres defined (e.g. {Parameter})
        /// </summary>
        /// <param name="st"></param>
        /// <returns></returns>
        public static IEnumerable<string> ExtractParameters(string st)
        {
            var tags = new List<string>();
            var r = new Regex(Current.Context.Settings.ParametersRegex);
            var m = r.Match(st);
            while (m.Success)
            {
                if (m.Groups.Count == 2)
                {
                    tags.Add(m.Groups[1].Value);
                }
                m = m.NextMatch();
            }
            return tags;
        }

        public object MakeObject(NameValueCollection replacements = null)
        {
            var o = Sitecore.Reflection.ReflectionUtil.CreateObject(Assembly, Class, new object[] { });
           
            if(replacements != null) ReplaceAttributes(replacements);

            foreach (var key in AttributesCollection.AllKeys)
            {
                var attribute = AttributesCollection[key];

                var propertyinfo = o.GetType().GetProperty(key, BindingFlags.NonPublic
                                                                   | BindingFlags.Public | BindingFlags.Instance);

                if (propertyinfo != null)
                {
                    Sitecore.Reflection.ReflectionUtil.SetProperty(o, propertyinfo, attribute);
                }
                else
                {
                    Log.Warn(
                        String.Format("ASR: cannot assign value to property {0} in type {1}", attribute, o.GetType()), this);
                }
            }
            return o;
        }

    }
}
