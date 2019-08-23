/*     INFINITY CODE 2013-2017      */
/*   http://www.infinity-code.com   */

using System;

/// <summary>
/// This class is obsolete. Use OnlineMapsGoogleGeocoding.
/// </summary>
[Obsolete("OnlineMapsFindLocation is obsolete. Use OnlineMapsGoogleGeocoding.")]
public class OnlineMapsFindLocation: OnlineMapsGoogleGeocoding
{
    public new static OnlineMapsFindLocationResult[] GetResults(string response)
    {
        OnlineMapsGoogleGeocodingResult[] results = OnlineMapsGoogleGeocoding.GetResults(response);
        return OnlineMapsUtils.DeepCopy<OnlineMapsFindLocationResult[]>(results);
    }
}

/// <summary>
/// This class is obsolete. Use OnlineMapsGoogleGeocodingResult.
/// </summary>
[Obsolete("OnlineMapsFindLocationResult is obsolete. Use OnlineMapsGoogleGeocodingResult.")]
public class OnlineMapsFindLocationResult : OnlineMapsGoogleGeocodingResult
{
    public OnlineMapsFindLocationResult()
    {
        
    }

    public OnlineMapsFindLocationResult(OnlineMapsXML node) : base(node)
    {
    }
}

/// <summary>
/// This class is obsolete. Use OnlineMapsGoogleGeocodingResult.AddressComponent.
/// </summary>
[Obsolete("OnlineMapsFindLocationResultAddressComponent is obsolete. Use OnlineMapsGoogleGeocodingResult.AddressComponent.")]
public class OnlineMapsFindLocationResultAddressComponent : OnlineMapsGoogleGeocodingResult.AddressComponent
{
    public OnlineMapsFindLocationResultAddressComponent()
    {
        
    }

    public OnlineMapsFindLocationResultAddressComponent(OnlineMapsXML node) : base(node)
    {
    }
}