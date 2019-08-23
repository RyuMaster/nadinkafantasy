/*     INFINITY CODE 2013-2017      */
/*   http://www.infinity-code.com   */

using System;

/// <summary>
/// This class is obsolete. Use OnlineMapsGoogleElevation.
/// </summary>
[Obsolete("OnlineMapsGetElevation is obsolete. Use OnlineMapsGoogleElevation.")]
public class OnlineMapsGetElevation:OnlineMapsGoogleElevation
{
    
}

/// <summary>
/// This class is obsolete. Use OnlineMapsGoogleElevationResult.
/// </summary>
[Obsolete("OnlineMapsGetElevationResult is obsolete. Use OnlineMapsGoogleElevationResult.")]
public class OnlineMapsGetElevationResult : OnlineMapsGoogleElevationResult
{
    public OnlineMapsGetElevationResult()
    {
        
    }

    public OnlineMapsGetElevationResult(OnlineMapsXML node) : base(node)
    {
    }
}
