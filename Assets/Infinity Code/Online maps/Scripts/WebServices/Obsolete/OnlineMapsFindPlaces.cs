/*     INFINITY CODE 2013-2017      */
/*   http://www.infinity-code.com   */

using System;

/// <summary>
/// This class is obsolete. Use OnlineMapsGooglePlaces.
/// </summary>
[Obsolete("OnlineMapsFindPlaces is obsolete. Use OnlineMapsGooglePlaces.")]
public class OnlineMapsFindPlaces:OnlineMapsGooglePlaces
{
    public new static OnlineMapsFindPlacesResult[] GetResults(string response, out string nextPageToken)
    {
        OnlineMapsGooglePlacesResult[] results = OnlineMapsGooglePlaces.GetResults(response, out nextPageToken);
        return OnlineMapsUtils.DeepCopy<OnlineMapsFindPlacesResult[]>(results);
    }
}

/// <summary>
/// This class is obsolete. Use OnlineMapsGooglePlacesResult.
/// </summary>
[Obsolete("OnlineMapsFindPlacesResult is obsolete. Use OnlineMapsGooglePlacesResult.")]
public class OnlineMapsFindPlacesResult : OnlineMapsGooglePlacesResult
{
    public OnlineMapsFindPlacesResult()
    {
        
    }

    public OnlineMapsFindPlacesResult(OnlineMapsXML node) : base(node)
    {
    }
}

/// <summary>
/// This class is obsolete. Use OnlineMapsGooglePlacesResult.Photo.
/// </summary>
[Obsolete("OnlineMapsFindPlacesResultPhoto is obsolete. Use OnlineMapsGooglePlacesResult.Photo.")]
public class OnlineMapsFindPlacesResultPhoto : OnlineMapsGooglePlacesResult.Photo
{
    public OnlineMapsFindPlacesResultPhoto ()
    {
        
    }

    public OnlineMapsFindPlacesResultPhoto(OnlineMapsXML node) : base(node)
    {
    }
}