using System;
using System.Collections.Generic;

namespace SimpleFlow.Core
{
    interface IDataStore
    {
        int Add(int jobId, object data, Type dataType);
        int AddReference(int jobId, int id);
        int AddReferences(int jobId, IEnumerable<int> ids, Type childDataType);
        object Get(int id);
        IEnumerable<int> SplitAndGetIds(int id); // split an array and get ids
    }

    static class DataStoreExtensions
    {
        public static T Get<T>(this IDataStore dataStore, int id)
        {
            var result = dataStore.Get(id);

            if (result == null)
                return default(T);

            return (T) result;
        }
    }
}