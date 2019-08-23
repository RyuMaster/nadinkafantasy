/*     INFINITY CODE 2013-2017      */
/*   http://www.infinity-code.com   */

using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

/// <summary>
/// The base class of JSON elements.
/// </summary>
public abstract class OnlineMapsJSONItem: IEnumerable<OnlineMapsJSONItem>
{
    /// <summary>
    /// Get the element by index
    /// </summary>
    /// <param name="index">Index of element</param>
    /// <returns>Element</returns>
    public abstract OnlineMapsJSONItem this[int index] { get; }

    /// <summary>
    /// Get the element by key.\n
    /// Supports XPath like selectors:\n
    /// ["key"] - get element by key.\n
    /// ["key1/key2"] - get element key2, which is a child of the element key1.\n
    /// ["key/N"] - where N is number. Get array element by index N, which is a child of the element key1.\n
    /// ["key/*"] - get all array elements, which is a child of the element key1.\n
    /// ["//key"] - get all elements with the key on the first or the deeper levels of the current element. \n
    /// </summary>
    /// <param name="key">Element key</param>
    /// <returns>Element</returns>
    public abstract OnlineMapsJSONItem this[string key] { get; }

    /// <summary>
    /// Deserializes current element
    /// </summary>
    /// <typeparam name="T">Type</typeparam>
    /// <returns>Object</returns>
    public T Deserialize<T>()
    {
        return (T)Deserialize(typeof (T));
    }

    /// <summary>
    /// Deserializes current element
    /// </summary>
    /// <param name="type">Type</param>
    /// <returns>Object</returns>
    public abstract object Deserialize(Type type);

    /// <summary>
    /// Get all elements with the key on the first or the deeper levels of the current element.
    /// </summary>
    /// <param name="key">Key</param>
    /// <returns>Elements</returns>
    public abstract OnlineMapsJSONItem GetAll(string key);

    /// <summary>
    /// Converts the current and the child elements to JSON string.
    /// </summary>
    /// <param name="b">StringBuilder instance</param>
    public abstract void ToJSON(StringBuilder b);

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    public virtual IEnumerator<OnlineMapsJSONItem> GetEnumerator()
    {
        return null;
    }

    public override string ToString()
    {
        StringBuilder b = new StringBuilder();
        ToJSON(b);
        return b.ToString();
    }

    /// <summary>
    /// Returns the value of the element, converted to the specified type.
    /// </summary>
    /// <param name="type">Type of variable</param>
    /// <returns>Value</returns>
    public abstract object Value(Type type);

    /// <summary>
    /// Returns the value of the element, converted to the specified type.
    /// </summary>
    /// <typeparam name="T">Type of variable</typeparam>
    /// <returns>Value</returns>
    public virtual T Value<T>()
    {
        return default(T);
    }
}