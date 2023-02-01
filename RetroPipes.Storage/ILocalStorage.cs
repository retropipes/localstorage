// MARS Web App by Rockwell Automation, Inc. (C) 2019-present

using System;
using System.Collections.Generic;

namespace RetroPipes.Storage;

public interface ILocalStorage
{
    /// <summary>
    /// Gets the number of elements contained in the LocalStorage.
    /// </summary>
    int Count { get; }

    /// <summary>
    /// Clears the in-memory contents of the LocalStorage, but leaves any persisted state on disk intact.
    /// </summary>
    /// <remarks>
    /// Use the Destroy method to delete the persisted file on disk.
    /// </remarks>
    void Clear();

    /// <summary>
    /// Deletes the persisted file on disk, if it exists, but keeps the in-memory data intact.
    /// </summary>
    /// <remarks>
    /// Use the Clear method to clear only the in-memory contents.
    /// </remarks>
    void Destroy();

    /// <summary>
    /// Determines whether this LocalStorage instance contains the specified key.
    /// </summary>
    /// <param name="key"></param>
    /// <returns></returns>
    bool Exists(string key);

    /// <summary>
    /// Loads an object from the LocalStorage, without knowing its type.
    /// </summary>
    /// <param name="key">Unique key, as used when the object was stored.</param>
    object Load(string key);

    /// <summary>
    /// Loads a strongly typed object from the LocalStorage.
    /// </summary>
    /// <param name="key">Unique key, as used when the object was stored.</param>
    T Load<T>(string key);

    /// <summary>
    /// Gets a collection containing all the keys in the LocalStorage.
    /// </summary>
    IReadOnlyCollection<string> Keys();

    /// <summary>
    /// Loads the persisted state from disk into memory, overriding the current memory instance.
    /// </summary>
    /// <remarks>
    /// Simply doesn't do anything if the file is not found on disk.
    /// </remarks>
    void Unpersist();

    /// <summary>
    /// Removes an object from the LocalStorage.
    /// </summary>
    /// <param name="key">Unique key for the object.</param>
    /// <remarks>
    /// Will ignore when the key does not exist.
    /// </remarks>
    void Remove(string key);

    /// <summary>
    /// Stores an object into the LocalStorage.
    /// </summary>
    /// <param name="key">Unique key, can be any string, used for retrieving it later.</param>
    /// <param name="instance"></param>
    void Store<T>(string key, T instance);

    /// <summary>
    /// Syntax sugar that transforms the response to an IEnumerable<T>, whilst also passing along an optional WHERE-clause.
    /// </summary>
    IEnumerable<T> Query<T>(string key, Func<T, bool> predicate = null);

    /// <summary>
    /// Persists the in-memory store to disk.
    /// </summary>
    void Persist();
}
