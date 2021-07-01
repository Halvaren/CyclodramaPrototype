using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Objects that use animations based on physics simulations used Alembic components, and that components are particularly problematic:
///     - In order to work properly, an object with Alembic components cannot be spawned during a game, but it must exist in scene since game starts.
///     - Any object inside a set is spawned together with the set when character moves in.
///     - Then, Alembic objects are not spawned with their set, but they exist deactivated in scene since beginning and get active and move to their position when
///     their set is spawned.
///     - ObjBehavior of an Alembic object does need to spawn together with their set, because of loading data reasons.
///     - Alembic objects need a Animator component to play their physical-simulated animations
///     - An ObjBehavior has a series of animation callbacks that they need to be called by animations from an Animator component part of the ObjBehavior gameObject.
/// So, in summary, there is a gameObject where there is the ObjBehavior component, and, as its child, it will be added another gameObject with the Alembic components
/// and an Animator component when the set is spawned. Since, animation callbacks and the Animator are not in the same hierarchy level, animations cannot call those
/// animation callbacks.
/// That's why this behavior is needed: it will be added as a component in the Alembic object (the child of the ObjBehavior gameObject), so it's in the same
/// hierarchy level than the Animator. Then, animation can call the animation callbacks of this script, and, in turn, they will call ObjBehavior animation callbacks
/// </summary>
public class AlembicAnimatorListener : MonoBehaviour
{
    [HideInInspector]
    public InteractableObjBehavior behavior;

    /// <summary>
    /// Main animation callback
    /// </summary>
    public void ExecuteAnimationCallback()
    {
        behavior?.ExecuteAnimationCallback();
    }

    /// <summary>
    /// Secondary animation callback
    /// </summary>
    public void ExecuteSecondAnimationCallback()
    {
        behavior?.ExecuteSecondAnimationCallback();
    }
}
