using UnityEngine;

using CommonsPattern;

public class TitleManager : SingletonManager<TitleManager>
{
    /// Callback for clicking anywhere on the title canvas. Add a PointerClick event to the TitleCanvas itself
    /// to handle all clicks not intercepted by other elements, or to some invisible panel covering the whole screen
    /// that you'd only activate when you want to detect such clicks.
    /// Only useful on PRESS START BUTTON message to enter the lobby.
    public void OnClickAnywhere()
    {
        Debug.Log("click!");
    }
}
