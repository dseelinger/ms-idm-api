﻿using System.Collections.Generic;
using System.ServiceModel.Channels;
using System.Threading.Tasks;
using IdmNet.Models;
using IdmNet.SoapModels;

// ReSharper disable InconsistentNaming

namespace IdmApi.DAL
{
    public interface IRepository
    {
        Task<int> GetCount(string filter);
        Task<IEnumerable<IdmResource>> GetByFilter(SearchCriteria criteria, int pageSize);
        Task<PagedSearchResults> GetPagedResults(SearchCriteria criteria, int pageSize);
        Task<IdmResource> GetById(string id, List<string> @select);
        Task<IdmResource> Create(IdmResource resource);
        Task<Message> PutAttribute(string objectID, string attrName, string attrValue);
        Task<Message> PostAttribute(string id, string attribute, string attributeValue);
        Task<Message> DeleteAttribute(string id, string attribute, string attributeValue);
        Task<Message> PutChanges(string id, Change[] changes);
        Task<Message> DeleteResource(string id);
    }
}