using System;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using UnityEngine;


public class DungeonTile : MonoBehaviour
{

    [SerializeField] private List<Vector3> _connectors = new();

    [SerializeField] private Transform _animRoot;

    private void Awake()
    {
        _animRoot.localScale = Vector3.zero;
        _animRoot.DOScale(Vector3.one, 0.5f).SetEase(Ease.OutBounce);
    }

    public List<Vector3> GetConnectors(bool worldPosition)
    {
        if (worldPosition)
            return _connectors.Select(connector => transform.TransformPoint(connector)).ToList();
        else
            return _connectors;
    }

    public List<Vector3Int> GetSurroundingTilePositions(bool worldPosition)
    {
        List<Vector3> connectors = GetConnectors(false);

        if (worldPosition)
            return connectors.Select(connector => Vector3Int.RoundToInt(transform.TransformPoint(connector * 2f))).ToList();
        else
            return connectors.Select(connector => Vector3Int.RoundToInt(connector * 2f)).ToList();
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.cyan;

        foreach (Vector3 connector in GetConnectors(true))
        {
            Gizmos.DrawSphere(connector, 0.05f);
        }

        Gizmos.color = Color.green;

        foreach (Vector3 connector in GetSurroundingTilePositions(true))
        {
            Gizmos.DrawSphere(connector, 0.1f);
        }
    }

}
