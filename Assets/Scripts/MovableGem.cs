using UnityEngine;
using System.Collections;
using System;
namespace Match3Game
{
    public class MovableGem : MonoBehaviour
    {
        private Gem gem;
        private IEnumerator moveCoroutine;

        // Event: Hareket bittiğinde haber vermek için
        public Action OnMoveComplete;

        private void Awake()
        {
            gem = GetComponent<Gem>();
        }

        public void Move(int newX, int newY, float time)
        {
            
            if (moveCoroutine != null)
            {
                StopCoroutine(moveCoroutine);
            }
            moveCoroutine = MoveCoroutine(newX, newY, time);
            StartCoroutine(moveCoroutine);
        }

        private IEnumerator MoveCoroutine(int newX, int newY, float time)
        {
            gem.X = newX;
            gem.Y = newY;

            Vector3 startPos = transform.position;
            Vector3 endPos = gem.GridRef.GetWorldPosition(newX, newY);

            for (float t = 0; t <= time; t += Time.deltaTime)
            {
                gem.transform.position = Vector3.Lerp(startPos, endPos, t / time);
                yield return null;
            }

            gem.transform.position = endPos;

            // Hareket bittiğini bildir
            OnMoveComplete?.Invoke();
        }
    }
}
