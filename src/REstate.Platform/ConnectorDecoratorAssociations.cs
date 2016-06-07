using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace REstate.Platform
{
    public class ConnectorDecoratorAssociations 
        : IDictionary<string, IEnumerable<IConnectorDecorator>>
    {
        private IDictionary<string, IEnumerable<IConnectorDecorator>> DataDictionary { get; }

        public ConnectorDecoratorAssociations()
        {
            DataDictionary = new Dictionary<string, IEnumerable<IConnectorDecorator>>();
        }

        public ConnectorDecoratorAssociations(IEnumerable<KeyValuePair<string, IEnumerable<IConnectorDecorator>>> pairs)
        {
            DataDictionary = pairs.ToDictionary(pair => pair.Key, pair => pair.Value);
        }

        public IEnumerator<KeyValuePair<string, IEnumerable<IConnectorDecorator>>> GetEnumerator()
        {
            return DataDictionary.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public void Add(KeyValuePair<string, IEnumerable<IConnectorDecorator>> item)
        {
            DataDictionary.Add(item);
        }

        public void Clear()
        {
            DataDictionary.Clear();
        }

        public bool Contains(KeyValuePair<string, IEnumerable<IConnectorDecorator>> item)
        {
            return DataDictionary.Contains(item);
        }

        public void CopyTo(KeyValuePair<string, IEnumerable<IConnectorDecorator>>[] array, int arrayIndex)
        {
            DataDictionary.CopyTo(array, arrayIndex);
        }

        public bool Remove(KeyValuePair<string, IEnumerable<IConnectorDecorator>> item)
        {
            return DataDictionary.Remove(item);
        }

        public int Count => DataDictionary.Count;
        public bool IsReadOnly => DataDictionary.IsReadOnly;
        public bool ContainsKey(string key)
        {
            return DataDictionary.ContainsKey(key);
        }

        public void Add(string key, IEnumerable<IConnectorDecorator> value)
        {
            DataDictionary.Add(key, value);
        }

        public bool Remove(string key)
        {
            return DataDictionary.Remove(key);
        }

        public bool TryGetValue(string key, out IEnumerable<IConnectorDecorator> value)
        {
            return DataDictionary.TryGetValue(key, out value);
        }

        public IEnumerable<IConnectorDecorator> this[string key]
        {
            get { return DataDictionary[key]; }
            set { DataDictionary[key] = value; }
        }

        public ICollection<string> Keys => DataDictionary.Keys;

        public ICollection<IEnumerable<IConnectorDecorator>> Values => DataDictionary.Values;
    }
}