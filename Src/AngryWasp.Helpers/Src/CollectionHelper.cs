using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace AngryWasp.Helpers
{
    public static class ListExtensions
    {
        public static List<T> Join<T>(this List<T> value, IEnumerable<T> toAdd)
        {
            value.AddRange(toAdd);
            return value;
        }

        public static List<T[]> Split<T>(this T[] array, int size)
        {
            List<T[]> res = new();
            for (var i = 0; i < array.Length / size; i++)
                res.Add(array.Skip(i * size).Take(size).ToArray());

            return res;
        }
    }

    public class ThreadSafeList<V>
    {
        [JsonProperty("list")]
        private readonly List<V> list = new();

        private readonly SemaphoreSlim s = new(1);

        public ThreadSafeList()
        {
            list = new List<V>();
        }

        public ThreadSafeList(IEnumerable<V> content)
        {
            list = new List<V>(content);
        }

        public async Task<List<V>> Copy()
        {
            await s.WaitAsync().ConfigureAwait(false);

            try
            {
                return new List<V>(list.ToArray());
            }
            finally { s.Release(); }
        }

        public async Task<HashSet<V>> CopyToHashSet()
        {
            await s.WaitAsync().ConfigureAwait(false);

            try
            {
                return new HashSet<V>(list.ToArray());
            }
            finally { s.Release(); }
        }

        public async Task<int> Count()
        {
            await s.WaitAsync().ConfigureAwait(false);

            try
            {
                return list.Count;
            }
            finally { s.Release(); }
        }

        public async Task<V> Get(int index)
        {
            await s.WaitAsync().ConfigureAwait(false);

            try
            {
                return list[index];
            }
            finally { s.Release(); }
        }

        public async Task<bool> Contains(V value)
        {
            await s.WaitAsync().ConfigureAwait(false);

            try
            {
                return list.Contains(value);
            }
            finally { s.Release(); }
        }

        public async Task Add(V value)
        {
            await s.WaitAsync().ConfigureAwait(false);

            try
            {
                list.Add(value);
            }
            finally { s.Release(); }
        }

        public async Task AddRange(IEnumerable<V> value)
        {
            await s.WaitAsync().ConfigureAwait(false);

            try
            {
                list.AddRange(value);
            }
            finally { s.Release(); }
        }

        public async Task<V> Last()
        {
            await s.WaitAsync().ConfigureAwait(false);

            try
            {
                return list[^1];
            }
            finally { s.Release(); }
        }

        public async Task<bool> Remove(V value)
        {
            await s.WaitAsync().ConfigureAwait(false);

            try
            {
                if (!list.Contains(value))
                    return false;

                list.Remove(value);
                return true;
            }
            finally { s.Release(); }
        }

        public async Task Clear()
        {
            await s.WaitAsync().ConfigureAwait(false);

            try
            {
                list.Clear();
            }
            finally { s.Release(); }
        }

        public async Task<ThreadSafeList<TResult>> Select<TResult>(Func<V, TResult> selector)
        {
            await s.WaitAsync().ConfigureAwait(false);

            try
            {
                return new ThreadSafeList<TResult>(list.Select(selector));
            }
            finally { s.Release(); }
        }

        public async Task<List<V>> Where(Func<V, bool> predicate)
        {
            await s.WaitAsync().ConfigureAwait(false);

            try
            {
                return new List<V>(list.Where(predicate));
            }
            finally { s.Release(); }
        }

        public async Task Iterate(Action<V> predicate)
        {
            await s.WaitAsync().ConfigureAwait(false);

            try
            {
                foreach (var i in list)
                    predicate(i);
            }
            finally { s.Release(); }
        }
    }

    public class ThreadSafeDictionary<K, V>
    {
        [JsonProperty("dict")]
        private readonly Dictionary<K, V> dict;

        public delegate void ThreadSafeDictionaryEventHandler(K key, V value);

        public event ThreadSafeDictionaryEventHandler Added;
        public event ThreadSafeDictionaryEventHandler Updated;
        public event ThreadSafeDictionaryEventHandler Removed;

        private readonly SemaphoreSlim s = new(1);

        public ThreadSafeDictionary()
        {
            dict = new Dictionary<K, V>();
        }

        public ThreadSafeDictionary(Dictionary<K, V> value)
        {
            dict = value;
        }

        public async Task<int> Count()
        {
            await s.WaitAsync().ConfigureAwait(false);

            try
            {
                return dict.Count;
            }
            finally { s.Release(); }
        }

        public async Task<List<K>> CopyKeys()
        {
            await s.WaitAsync().ConfigureAwait(false);

            try
            {
                return new List<K>(dict.Keys.ToArray());
            }
            finally { s.Release(); }
        }

        public async Task<List<V>> CopyValues()
        {
            await s.WaitAsync().ConfigureAwait(false);

            try
            {
                return new List<V>(dict.Values.ToArray());
            }
            finally { s.Release(); }
        }

        public async Task<Dictionary<K, V>> Copy()
        {
            await s.WaitAsync().ConfigureAwait(false);

            try
            {
                return new Dictionary<K, V>(dict.ToArray());
            }
            finally { s.Release(); }
        }

        public async Task<HashSet<K>> CopyKeysToHashSet()
        {
            await s.WaitAsync().ConfigureAwait(false);

            try
            {
                return new HashSet<K>(dict.Keys.ToArray());
            }
            finally { s.Release(); }
        }

        public async Task<HashSet<V>> CopyValuesToHashSet()
        {
            await s.WaitAsync().ConfigureAwait(false);

            try
            {
                return new HashSet<V>(dict.Values.ToArray());
            }
            finally { s.Release(); }
        }

        public async Task<V> Get(K key)
        {
            await s.WaitAsync().ConfigureAwait(false);

            try
            {
                if (!dict.ContainsKey(key))
                    return default;

                return dict[key];
            }
            finally { s.Release(); }
        }

        public async Task<(bool, V)> TryGet(K key)
        {
            await s.WaitAsync().ConfigureAwait(false);

            try
            {
                if (!dict.ContainsKey(key))
                    return (false, default(V));

                return (true, dict[key]);
            }
            finally { s.Release(); }
        }

        public async Task<bool> ContainsKey(K key)
        {
            await s.WaitAsync().ConfigureAwait(false);

            try
            {
                return dict.ContainsKey(key);
            }
            finally { s.Release(); }
        }

        public async Task<bool> Add(K key, V value)
        {
            await s.WaitAsync().ConfigureAwait(false);

            try
            {
                if (dict.ContainsKey(key))
                    return false;

                dict.Add(key, value);
                return true;
            }
            finally { s.Release(); }
        }

        public async Task<bool> Remove(K key)
        {
            await s.WaitAsync().ConfigureAwait(false);

            try
            {
                if (!dict.ContainsKey(key))
                    return false;

                var value = dict[key];

                dict.Remove(key);
                Removed?.Invoke(key, value);
                return true;
            }
            finally { s.Release(); }
        }

        public async Task AddOrUpdate(K key, V value)
        {
            await s.WaitAsync().ConfigureAwait(false);

            try
            {
                if (dict.ContainsKey(key))
                {
                    dict[key] = value;
                    Updated?.Invoke(key, value);
                }
                else
                {
                    dict.Add(key, value);
                    Added?.Invoke(key, value);
                }
            }
            finally { s.Release(); }
        }

        public async Task Clear()
        {
            await s.WaitAsync().ConfigureAwait(false);

            try
            {
                dict.Clear();
            }
            finally { s.Release(); }
        }

        public async Task<V> Last()
        {
            await s.WaitAsync().ConfigureAwait(false);

            try
            {
                return dict.Last().Value;
            }
            finally { s.Release(); }
        }

        public async Task<Dictionary<K, V>> Where(Func<KeyValuePair<K, V>, bool> predicate)
        {
            await s.WaitAsync().ConfigureAwait(false);

            try
            {
                return dict.Where(predicate).ToDictionary(x => x.Key, x => x.Value);
            }
            finally { s.Release(); }
        }

        public async Task<bool> Modify(K key, Func<V, bool> predicate)
        {
            await s.WaitAsync().ConfigureAwait(false);

            try
            {
                return predicate(dict[key]);
            }
            finally {
                s.Release();
            }
        }

        public async Task<bool> ModifyAll(Action<V> predicate)
        {
            await s.WaitAsync().ConfigureAwait(false);

            try
            {
                foreach (var i in dict.Values)
                    predicate(i);

                return true;
            }
            catch { return false; }
            finally { s.Release(); }
        }
    }
}
