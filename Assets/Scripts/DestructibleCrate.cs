using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class DestructibleCrate : MonoBehaviour
{
    public static event EventHandler OnAnyDestroyed;
    private GridPosition gridPosition;
    [SerializeField] private Transform crateDestroyedPrefab;

    private void Start()
    {
        gridPosition = LevelGrid.Instance.GetGridPosition(transform.position);
    }

    public GridPosition GetGridPosition()
    {
        return gridPosition;
    }

    public void Damage()
    {
        Transform crateDestroyedTransform = Instantiate(crateDestroyedPrefab, transform.position, transform.rotation);
        ApplyExplosionToChildren(crateDestroyedTransform, 180f, transform.position, 10f);

        OnAnyDestroyed?.Invoke(this, EventArgs.Empty);

        Destroy(gameObject);
    }

    private void ApplyExplosionToChildren(Transform root, float explosionForce, Vector3 explosionPosition, float explosionRange)
    {
        foreach (Transform child in root)
        {
            if (child.TryGetComponent<Rigidbody>(out Rigidbody childRigidbody))
            {
                childRigidbody.AddExplosionForce(explosionForce, explosionPosition, explosionRange);
            }

            ApplyExplosionToChildren(child, explosionForce, explosionPosition, explosionRange);
        }
    }
}
