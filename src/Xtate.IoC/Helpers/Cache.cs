// Copyright © 2019-2025 Sergii Artemenko
// 
// This file is part of the Xtate project. <https://xtate.net/>
// 
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU Affero General Public License as published
// by the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU Affero General Public License for more details.
// 
// You should have received a copy of the GNU Affero General Public License
// along with this program.  If not, see <https://www.gnu.org/licenses/>.

using System.Collections.Concurrent;

namespace Xtate.IoC;

/// <summary>
///     Represents a thread-safe cache that stores key-value pairs.
/// </summary>
/// <typeparam name="TKey">The type of the keys in the cache.</typeparam>
/// <typeparam name="TValue">The type of the values in the cache.</typeparam>
internal class Cache<TKey, TValue> where TKey : notnull
{
    /// <summary>
    ///     The estimated number of threads that will update the <see cref="ConcurrentDictionary{TKey,TValue}" /> concurrently.
    /// </summary>
    private const int ConcurrencyLevel = 1;

    private readonly ConcurrentDictionary<TKey, TValue> _dictionary;

    /// <summary>
    ///     Initializes a new instance of the <see cref="Cache{TKey, TValue}" /> class with the specified initial capacity.
    /// </summary>
    /// <param name="initialCapacity">The initial number of elements that the cache can contain.</param>
    public Cache(int initialCapacity) => _dictionary = new ConcurrentDictionary<TKey, TValue>(ConcurrencyLevel, initialCapacity);

    /// <summary>
    ///     Initializes a new instance of the <see cref="Cache{TKey, TValue}" /> class that contains elements copied from the
    ///     specified collection.
    /// </summary>
    /// <param name="initialCollection">The collection whose elements are copied to the new cache.</param>
    public Cache(IEnumerable<KeyValuePair<TKey, TValue>> initialCollection) =>
        _dictionary = new ConcurrentDictionary<TKey, TValue>(ConcurrencyLevel, initialCollection, EqualityComparer<TKey>.Default);

    /// <summary>
    ///     Attempts to get the value associated with the specified key.
    /// </summary>
    /// <param name="key">The key of the value to get.</param>
    /// <param name="value">
    ///     When this method returns, contains the object from the cache that has the specified key, or the
    ///     default value of the type if the operation failed.
    /// </param>
    /// <returns>true if the key was found in the cache; otherwise, false.</returns>
    public bool TryGetValue(TKey key, [MaybeNullWhen(false)] out TValue value) => _dictionary.TryGetValue(key, out value);

    /// <summary>
    ///     Determines whether the cache contains the specified key.
    /// </summary>
    /// <param name="key">The key to locate in the cache.</param>
    /// <returns>true if the cache contains an element with the specified key; otherwise, false.</returns>
    public bool ContainsKey(TKey key) => _dictionary.ContainsKey(key);

    /// <summary>
    ///     Gets the value associated with the specified key. If the key does not exist, adds the key with the specified value.
    /// </summary>
    /// <param name="key">The key of the value to get or add.</param>
    /// <param name="value">The value to add if the key does not exist.</param>
    /// <returns>The value associated with the specified key, or the new value if the key was not present.</returns>
    public TValue GetOrAdd(TKey key, TValue value) => _dictionary.GetOrAdd(key, value);

    /// <summary>
    ///     Attempts to add the specified key and value to the cache.
    /// </summary>
    /// <param name="key">The key of the element to add.</param>
    /// <param name="value">The value of the element to add.</param>
    public void TryAdd(TKey key, TValue value) => _dictionary.TryAdd(key, value);
}