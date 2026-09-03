using UnityEngine;

/// <summary>
/// Marks a spawned chain-reaction explosion VFX so popups can clear it
/// off screen. No fields — presence of the component is the signal.
///
/// Fired cannonballs aur break fragments par pehle se
/// LowerGroundDisappearEffect + Rigidbody hote hain, is liye
/// PopupGameplayVisibilityController unhein pehchan leta tha. Ye VFX un
/// dono ke baghair hai, is liye popup khulne par bhi screen par urta
/// reh jata tha — yehi marker usay bhi qaabil-e-shanakht banata hai.
/// </summary>
public sealed class ChainReactionVfxMarker : MonoBehaviour
{
}
