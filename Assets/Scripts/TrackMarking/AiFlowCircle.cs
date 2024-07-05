using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using Quaternion = System.Numerics.Quaternion;

[ExecuteInEditMode]
public class AiFlowCircle : MonoBehaviour {
    [SerializeField] private int numOfPoints = 10;
    private int _numOfPoints;
    [SerializeField] private float radius = 1f;
    private float _radius;
    [SerializeField] private float angleOffset = 0f;
    private float _angleOffset;
    

    private float pow = 0.5f;
    private float turnFraction = 1.618033988749f;

    private void Start() {
        SyncValues();
        MakePoints();
    }

    void Update() {
        if (isChanged()) {
            DestroyPoints();
            MakePoints();
            SyncValues();
        }
    }

    private bool isChanged() {
        if (
            numOfPoints != _numOfPoints
            || radius != _radius
            || angleOffset != _angleOffset
        ) {
            return true;
        }

        return false;
    }

    private void MakePoints() {
        for (int i = 0; i < numOfPoints; i++) {
            float dst = Mathf.Pow(i / (numOfPoints - 1f), pow) * radius;
            float angle = 2 * Mathf.PI * turnFraction * i;
            float angleDeg = angle * Mathf.Rad2Deg;

            float z = dst * Mathf.Cos(angle);
            float x = dst * Mathf.Sin(angle);

            GameObject point = new GameObject($"{name}:{i}", typeof(AiWaypoint));
            point.transform.parent = transform;
            point.transform.localPosition = new Vector3(x, 0f, z);
            point.transform.localRotation = UnityEngine.Quaternion.Euler(0f, angleDeg + angleOffset, 0f);
        }
    }

    private void DestroyPoints() {
        while (transform.childCount > 0) {
            DestroyImmediate(transform.GetChild(0).gameObject);
        }
    }

    private void SyncValues() {
        _numOfPoints = numOfPoints;
        _radius = radius;
        _angleOffset = angleOffset;
    }
}