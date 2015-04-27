﻿#pragma warning disable 1591
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel.Channels;
using System.Threading.Tasks;
using IdmNet;
using IdmNet.Models;
using IdmNet.SoapModels;

// ReSharper disable InconsistentNaming

namespace IdmApi.DAL
{
    public class Repository : IRepository
    {
        private readonly IdmNetClient _idmNet;

        public Repository(IdmNetClient idmNet)
        {
            _idmNet = idmNet;
        }

        public async Task<IdmResource> GetById(string id, List<string> @select)
        {
            return await _idmNet.GetAsync(id, @select);
        }

        public async Task<IEnumerable<IdmResource>> GetByFilter(SearchCriteria criteria)
        {
            var searchResults = await _idmNet.SearchAsync(criteria);
            return searchResults;
        }

        public async Task<IdmResource> Post(IdmResource resource)
        {
            var result = await _idmNet.PostAsync(resource);
            return result;
        }

        public async Task<Message> PutAttribute(string objectID, string attrName, string attrValue)
        {
            return await _idmNet.ReplaceValueAsync(objectID, attrName, attrValue);
        }

        public async Task<Message> PostAttribute(string id, string attribute, string attributeValue)
        {
            return await _idmNet.AddValueAsync(id, attribute, attributeValue);
        }

        public async Task<Message> DeleteAttribute(string id, string attribute, string attributeValue)
        {
            return await _idmNet.RemoveValueAsync(id, attribute, attributeValue);
        }

        public async Task<Message> PutChanges(string id, Change[] changes)
        {
            return await _idmNet.ChangeMultipleAttrbutes(id, changes);
        }

        public async Task<Message> DeleteResource(string id)
        {
            return await _idmNet.DeleteAsync(id);
        }
    }
}