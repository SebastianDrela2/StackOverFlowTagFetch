﻿using MediPortApi.HttpProcessing;
using MediPortApi.TagProcessing;

namespace MediPort.RestApi.Data
{
    public interface ITagsStore
    {
        Task RefreshAllTags(string apiKey);
        Task<IEnumerable<SimplifiedTag>> GetTagsSorted(int page, SortOption sortOption);      
        Task<SimplifiedTag> GetTag(int id);
        Task<SimplifiedTag> CreateTag(Tag tag);
        Task UpdateTag(int id, Tag tag);
        Task DeleteTag(int id);
    }
}
