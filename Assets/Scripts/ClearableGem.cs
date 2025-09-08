using Match3Game;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClearableGem : MonoBehaviour
{
    public AnimationClip clearAnimation;

    private bool isBeingCleared=false;

    public bool IsBeingCleared
    {
        get { return isBeingCleared; }
    }
    protected Gem gem;

    private void Awake()
    {
        gem = GetComponent<Gem>();
    }

    public void Clear()
    {
        isBeingCleared = true;
        StartCoroutine(ClearCoroutine());
    }

    private IEnumerator ClearCoroutine()
    {
        Animator animator = GetComponent<Animator>();
        if (animator)
        {
            animator.Play(clearAnimation.name);

            yield return new WaitForSeconds(clearAnimation.length);

            Destroy(gameObject);

        }
    }
}
