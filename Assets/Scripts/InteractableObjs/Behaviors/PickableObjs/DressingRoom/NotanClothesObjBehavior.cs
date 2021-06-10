using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NotanClothesObjBehavior : PickableObjBehavior
{
    public float emissionForce = 50f;
    public bool inEmission;
    public Transform emissionPointToFace;
    public Transform emittedPosition;

    protected Rigidbody m_Rigidbody;
    public Rigidbody Rigidbody
    {
        get
        {
            if (m_Rigidbody == null) m_Rigidbody = GetComponent<Rigidbody>();
            return m_Rigidbody;
        }
    }

    public override void InitializeObjBehavior(GameObject currentSet)
    {
        this.currentSet = currentSet;

        UpdateMethods();
        if (obj != null)
        {
            obj.behavior = this;

            foreach (UseOfVerb verb in useOfVerbs)
            {
                verb.actuatorObj = this;
                verb.targetObj = null;
            }
        }
    }

    public void StartInEmittedPosition()
    {
        gameObject.SetActive(true); 
        inEmission = false;
        Rigidbody.isKinematic = true;
        transform.position = emittedPosition.position;
    }

    public override IEnumerator GiveMethod(InteractableObjBehavior targetObj)
    {
        int index = GetObjRelationIndex(targetObj, giveObjRelations);

        //Belinda
        if(index == 1)
        {
            BelindaBehavior belinda = (BelindaBehavior)targetObj;
            yield return belinda.StartCoroutine(belinda._GiveObj(obj));
        }

        yield return base.GiveMethod(targetObj);
    }

    public void Emit()
    {
        inEmission = true;
        Rigidbody.isKinematic = false;
        Vector3 forceDirection = (emissionPointToFace.position - transform.position).normalized;
        forceDirection.y = 1;
        Rigidbody.AddForce(forceDirection * emissionForce);

        StartCoroutine(FinishEmission());
    }

    IEnumerator FinishEmission()
    {
        yield return new WaitForSeconds(0.5f);

        while(Mathf.Abs(Rigidbody.velocity.y) > Mathf.Epsilon)
        {
            yield return null;
        }

        inEmission = false;
        Rigidbody.isKinematic = true;
    }
}
