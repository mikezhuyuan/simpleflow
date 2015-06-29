using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

namespace SimpleFlow.Core
{
    class InMemoryDataStore : IDataStore
    {
        readonly IDictionary<int, DataEntry> _jsonStore = new Dictionary<int, DataEntry>();

        public int Add(int jobId, object data, Type dataType)
        {
            if (!dataType.IsInstanceOfType(data))
                throw new Exception("Incompatible type against data");

            return AddInternal(jobId, data, dataType, DataEntryType.Direct);
        }

        public int AddReference(int jobId, int id)
        {
            string dataType = null;

            lock (_jsonStore)
            {
                if (_jsonStore.ContainsKey(id))
                    dataType = _jsonStore[id].DataType;
            }

            return AddInternal(jobId, id, dataType, DataEntryType.Indirect);
        }

        public int AddReferences(int jobId, IEnumerable<int> ids, Type childDataType)
        {
            return AddInternal(jobId, ids, childDataType, DataEntryType.Aggregate);
        }

        public object Get(int id)
        {
            DataEntry entry = null;
            lock (_jsonStore)
            {
                if (_jsonStore.ContainsKey(id))
                {
                    entry = _jsonStore[id];
                }
            }

            if (entry == null)
                return null;

            var dataType = JsonConvert.DeserializeObject<Type>(entry.DataType);
            switch (entry.Type)
            {
                case DataEntryType.Direct:
                    return JsonConvert.DeserializeObject(entry.Data, dataType);
                case DataEntryType.Indirect:
                    return Get(JsonConvert.DeserializeObject<int>(entry.Data));
                case DataEntryType.Aggregate:
                    var itemIds = JsonConvert.DeserializeObject<IEnumerable<int>>(entry.Data);
                    var results = new ArrayList();

                    foreach (var itemId in itemIds)
                    {
                        results.Add(Get(itemId));
                    }

                    return results.ToArray(dataType);
            }

            throw new Exception("invalid entry type " + entry.Type);
        }

        public IEnumerable<int> SplitAndGetIds(int id)
        {
            DataEntry entry;
            lock (_jsonStore)
            {
                entry = _jsonStore[id];
            }

            if (entry.Type == DataEntryType.Aggregate)
            {
                return JsonConvert.DeserializeObject<IEnumerable<int>>(entry.Data);
            }

            IEnumerable inputs;
            lock (_jsonStore)
            {
                inputs = (IEnumerable) Get(id);
            }

            var ids = new List<int>();
            var dataType = JsonConvert.DeserializeObject<Type>(entry.DataType);
            var unamplifiedType = dataType.Unamplify();

            foreach (var input in inputs)
            {
                ids.Add(Add(entry.JobId, input, unamplifiedType));
            }

            return ids;
        }

        int AddInternal(int jobId, object data, Type dataType, DataEntryType entryType)
        {
            return AddInternal(jobId, data, JsonConvert.SerializeObject(dataType), entryType);
        }

        int AddInternal(int jobId, object data, string dataType, DataEntryType entryType)
        {
            if (data == null) throw new ArgumentNullException("data");
            if (dataType == null) throw new ArgumentNullException("dataType");

            lock (_jsonStore)
            {
                var maxId = _jsonStore.Keys.Any() ? _jsonStore.Keys.Max() : 0;

                var id = maxId + 1;

                _jsonStore[id] =
                    new DataEntry
                    {
                        JobId = jobId,
                        Data = JsonConvert.SerializeObject(data),
                        DataType = dataType,
                        Type = entryType
                    };

                return id;
            }
        }

        enum DataEntryType
        {
            Direct = 0,
            Indirect = 1,
            Aggregate = 2
        }

        class DataEntry
        {
            public string Data;
            public string DataType;
            public int JobId;
            public DataEntryType Type;
        }
    }
}