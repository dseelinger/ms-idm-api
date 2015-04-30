using System;
using IdmNet.Models;
using IdmNet.SoapModels;

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
        public ETag(PagingContext pagingContext)
        {
            throw new System.NotImplementedException();
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
    }
}