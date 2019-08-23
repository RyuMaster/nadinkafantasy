/*     INFINITY CODE 2013-2017      */
/*   http://www.infinity-code.com   */

using System;

/// <summary>
/// This class is obsolete. Use OnlineMapsGooglePlacesAutocomplete.
/// </summary>
[Obsolete("OnlineMapsFindAutocomplete is obsolete. Use OnlineMapsGooglePlacesAutocomplete.")]
public class OnlineMapsFindAutocomplete: OnlineMapsGooglePlacesAutocomplete
{
    public new static OnlineMapsFindAutocompleteResult[] GetResults(string response)
    {
        OnlineMapsGooglePlacesAutocompleteResult[] results = OnlineMapsGooglePlacesAutocomplete.GetResults(response);
        return OnlineMapsUtils.DeepCopy<OnlineMapsFindAutocompleteResult[]>(results);
    }
}

/// <summary>
/// This class is obsolete. Use OnlineMapsGooglePlacesAutocompleteResult.
/// </summary>
[Obsolete("OnlineMapsFindAutocompleteResult is obsolete. Use OnlineMapsGooglePlacesAutocompleteResult.")]
public class OnlineMapsFindAutocompleteResult : OnlineMapsGooglePlacesAutocompleteResult
{
    public OnlineMapsFindAutocompleteResult()
    {
        
    }

    public OnlineMapsFindAutocompleteResult(OnlineMapsXML node) : base(node)
    {
    }
}

/// <summary>
/// This class is obsolete. Use OnlineMapsGooglePlacesAutocompleteResult.Term.
/// </summary>
[Obsolete("OnlineMapsFindAutocompleteResultTerm is obsolete. Use OnlineMapsGooglePlacesAutocompleteResult.Term.")]
public class OnlineMapsFindAutocompleteResultTerm : OnlineMapsGooglePlacesAutocompleteResult.Term
{
    public OnlineMapsFindAutocompleteResultTerm()
    {
        
    }

    public OnlineMapsFindAutocompleteResultTerm(OnlineMapsXML node) : base(node)
    {
    }
}

/// <summary>
/// This class is obsolete. Use OnlineMapsGooglePlacesAutocompleteResult.MatchedSubstring.
/// </summary>
[Obsolete("OnlineMapsFindAutocompleteResultMatchedSubstring is obsolete. Use OnlineMapsGooglePlacesAutocompleteResult.MatchedSubstring.")]
public class OnlineMapsFindAutocompleteResultMatchedSubstring : OnlineMapsGooglePlacesAutocompleteResult.MatchedSubstring
{
    public OnlineMapsFindAutocompleteResultMatchedSubstring()
    {
        
    }

    public OnlineMapsFindAutocompleteResultMatchedSubstring(OnlineMapsXML node) : base(node)
    {
    }
}