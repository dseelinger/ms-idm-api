using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using IdmNet.Models;
using IdmNet.SoapModels;
using Microsoft.Practices.ObjectBuilder2;

namespace IdmApi.Models
{
    /// <summary>
    /// ETag is a class that represents the Identity Manager Object Type for retrieving subsequent Pulls from a 
    /// previous search request
    /// </summary>
    public sealed class ETag : IdmResource
    {
        const string ForcedObjType = "ETag";

        /// <summary>
        /// Parameterless CTOR
        /// </summary>
        public ETag()
        {
            ObjectType = "ETag";
        }

        /// <summary>
        /// Create a new ETag from a paging context
        /// </summary>
        /// <param name="pagingContext">The PagingContext to be retrieved in the next pull.</param>
        public ETag(PagingContext pagingContext) : this()
        {
            CurrentIndex = pagingContext.CurrentIndex;
            EnumerationDirection = pagingContext.EnumerationDirection;
            Expires = pagingContext.Expires;
            Filter = pagingContext.Filter;
            Select = pagingContext.Selection.JoinStrings(",");
            SortingDialect = pagingContext.Sorting.Dialect;
            SortingAttributes = (from sa in pagingContext.Sorting.SortingAttributes select sa.AttributeName + ':' + sa.Ascending).JoinStrings(",");
        }

        /// <summary>
        /// Create a new ETag from an existing IdmResource object
        /// </summary>
        /// <param name="resource"></param>
        /// <exception cref="NotImplementedException"></exception>
        public ETag(IdmResource resource)
        {
            Attributes = resource.Attributes;
            ObjectType = "ETag";
            if (resource.Creator == null)
                return;
            Creator = resource.Creator;
        }

        /// <summary>
        /// Object Type (can only be Group)
        /// </summary>
        [Required]
        public override string ObjectType
        {
            get { return GetAttrValue("ObjectType"); }
            set
            {
                if (value != ForcedObjType)
                    throw new InvalidOperationException("Object Type of Person can only be 'Person'");
                SetAttrValue("ObjectType", value);
            }
        }

        /// <summary>
        /// Current Index in the paging "session"
        /// </summary>
        public int? CurrentIndex 
        {
            get { return AttrToInteger("CurrentIndex"); }
            set { SetAttrValue("CurrentIndex", value.ToString()); }
        }

        /// <summary>
        /// Enumeration Direction (Forwards|Backwards)
        /// </summary>
        public string EnumerationDirection
        {
            get { return GetAttrValue("EnumerationDirection"); }
            set { SetAttrValue("EnumerationDirection", value); }
        }

        /// <summary>
        /// Always some string time millenia in the far future
        /// </summary>
        public string Expires
        {
            get { return GetAttrValue("Expires"); }
            set { SetAttrValue("Expires", value); }
        }

        /// <summary>
        /// XPath Filter for the original Search
        /// </summary>
        public string Filter
        {
            get { return GetAttrValue("Filter"); }
            set { SetAttrValue("Filter", value); }
        }

        public string Select
        {
            get { return GetAttrValue("Select"); }
            set { SetAttrValue("Select", value); }
        }

        public string SortingDialect
        {
            get { return GetAttrValue("SortingDialect"); }
            set { SetAttrValue("SortingDialect", value); }
        }

        public string SortingAttributes
        {
            get { return GetAttrValue("SortingAttributes"); }
            set { SetAttrValue("SortingAttributes", value); }
        }

        public PagingContext ToPagingContext()
        {
            var sortAttrs =
                SortingAttributes.Split(',').Select(sort => sort.Split(':')).Select(sortParts => new SortingAttribute
                {
                    AttributeName = sortParts[0],
                    Ascending = Boolean.Parse(sortParts[1])
                }).ToArray();


            var returnVal = new PagingContext
            {
                CurrentIndex = CurrentIndex ?? 50,
                EnumerationDirection = EnumerationDirection,
                Expires = Expires,
                Filter = Filter,
                Selection = Select.Split(','),
                Sorting = new Sorting
                {
                    Dialect = SortingDialect,
                    SortingAttributes = sortAttrs
                }
            };

            return returnVal;
        }
    }
}