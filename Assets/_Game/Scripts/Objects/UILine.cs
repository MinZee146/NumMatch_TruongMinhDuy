using System;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class UILine : MonoBehaviour
{
    [SerializeField] private RectTransform _myTransform;

    public void CreateLine(Vector3 positionOne, Vector3 positionTwo)
    {
        var point1 = (Vector2)positionTwo;
        var point2 = (Vector2)positionOne;
        var midpoint = (point1 + point2) / 2f;
        var dir = point1 - point2;

        _myTransform.position = midpoint;
        _myTransform.rotation = Quaternion.Euler(0f, 0f, Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg);
        _myTransform.localScale = new Vector3(dir.magnitude, 1f, 0f);
    }

    public void FadeAndDestroy()
    {
        GetComponent<Image>().DOFade(0f, 1f).OnComplete(() => Destroy(gameObject));
    }
}