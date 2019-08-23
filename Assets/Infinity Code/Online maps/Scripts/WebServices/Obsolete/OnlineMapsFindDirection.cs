/*     INFINITY CODE 2013-2017      */
/*   http://www.infinity-code.com   */

using System;

/// <summary>
/// This class is obsolete. Use OnlineMapsGoogleDirections.
/// </summary>
[Obsolete("OnlineMapsFindDirection is obsolete. Use OnlineMapsGoogleDirections.")]
public class OnlineMapsFindDirection: OnlineMapsGoogleDirections
{
    public new static OnlineMapsFindDirectionResult GetResult(string response)
    {
        OnlineMapsGoogleDirectionsResult result = OnlineMapsGoogleDirections.GetResult(response);
        return OnlineMapsUtils.DeepCopy<OnlineMapsFindDirectionResult>(result);
    }
}

/// <summary>
/// This class is obsolete. Use OnlineMapsGoogleDirectionsResult.
/// </summary>
[Obsolete("OnlineMapsFindDirectionResult is obsolete. Use OnlineMapsGoogleDirectionsResult.")]
public class OnlineMapsFindDirectionResult : OnlineMapsGoogleDirectionsResult
{
    public OnlineMapsFindDirectionResult()
    {
        
    }

    public OnlineMapsFindDirectionResult(OnlineMapsXML xml) : base(xml)
    {
    }
}