using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class DoorBehavior : InteractableObjBehavior
{
    [HideInInspector]
    public bool opened = false;
    [HideInInspector]
    public bool locked = false;

    protected Animator animator;
    public Animator Animator
    {
        get
        {
            if (animator == null) animator = GetComponent<Animator>();
            return animator;
        }
    }

    public IEnumerator OpenDoor()
    {
        if (!opened)
        {
            AddAnimationLock();
            animationCallback += ReleaseAnimationLock;
            PlayOpenAnimation();

            while(animationLocks.Count > 0)
            {
                yield return null;
            }

            SetOpenedClosedDoor(true);
            animationCallback -= ReleaseAnimationLock;
        }

    }

    public IEnumerator CloseDoor()
    {
        if (opened)
        {
            AddAnimationLock();
            animationCallback += ReleaseAnimationLock;
            PlayCloseAnimation();

            while (animationLocks.Count > 0)
            {
                yield return null;
            }

            SetOpenedClosedDoor(false);
            animationCallback -= ReleaseAnimationLock;
        }
    }

    public void PlayOpenAnimation()
    {
        Animator.SetTrigger("open");
    }

    public void PlayCloseAnimation()
    {
        Animator.SetTrigger("close");
    }

    public virtual void SetOpenedClosedDoor(bool value)
    {
        opened = value;

        if (obstacleCollider)
        {
            obstacleCollider.enabled = !opened;
            currentSet.GetComponent<NavMeshSurface>().BuildNavMesh();
        }

        if (opened)
            Animator.SetTrigger("opened");
        else
            Animator.SetTrigger("closed");
    }

    #region Data methods

    public void _LoadData(DoorData data)
    {
        _ApplyData(data.inScene, data.opened);
    }

    public void _ApplyData(bool inScene, bool opened)
    {
        _ApplyData(inScene);

        SetOpenedClosedDoor(opened);
    }

    public override InteractableObjData _GetObjData()
    {
        return new DoorData(inScene, opened, locked);
    }

    #endregion
}
