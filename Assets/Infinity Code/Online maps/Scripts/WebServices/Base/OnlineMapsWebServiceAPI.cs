/*     INFINITY CODE 2013-2017      */
/*   http://www.infinity-code.com   */

using System;

/// <summary>
/// The base class for working with the web services.
/// </summary>
public abstract class OnlineMapsWebServiceAPI
{
    /// <summary>
    /// Event that occurs when the current request instance is disposed.
    /// </summary>
    public Action<OnlineMapsWebServiceAPI> OnDispose;

    /// <summary>
    /// Event that occurs after OnComplete, when the response from webservice processed.
    /// </summary>
    public Action<OnlineMapsWebServiceAPI> OnFinish;

    /// <summary>
    /// In this variable you can put any data that you need to work with requests.
    /// </summary>
    public object customData;

    protected OnlineMapsQueryStatus _status;
    protected OnlineMapsWWW www;

    /// <summary>
    /// Gets the current status of the request to webservice.
    /// </summary>
    /// <value>
    /// The status.
    /// </value>
    public OnlineMapsQueryStatus status
    {
        get { return _status; }
    }

    /// <summary>
    /// Destroys the current request to webservice.
    /// </summary>
    public abstract void Destroy();
}