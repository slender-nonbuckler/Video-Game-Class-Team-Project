using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class AiFlowRect : MonoBehaviour {
    [SerializeField] private float width = 1f;
    private float _width;
    [SerializeField] private float height = 1f;
    private float _height;
    [SerializeField] private float pointsPerWidthUnit = 0.5f;
    private float _pointsPerWidthUnit;
    [SerializeField] private float pointsPerHeightUnit = 0.5f;
    private float _pointsPerHeightUnit;
    [SerializeField] private float angleOffset = 0f;
    private float _angleOffset;

    void Update() {
        if (!isChanged()) {
            return;
        }

        if (isNumOfPointsChanged()) {
            DestroyPoints();
            MakePoints();
        }

        SyncPoints();
        SyncValues();
    }

    private bool isNumOfPointsChanged() {
        return (
            width != _width
            || height != _height
            || pointsPerWidthUnit != _pointsPerWidthUnit
            || pointsPerHeightUnit != _pointsPerHeightUnit
        );
    }

    private bool isChanged() {
        return (
            width != _width
            || height != _height
            || pointsPerWidthUnit != _pointsPerWidthUnit
            || pointsPerHeightUnit != _pointsPerHeightUnit
            || angleOffset != _angleOffset
        );
    }
    
    private void MakePoints() {
        int numOfWidthPoints = Mathf.FloorToInt(width * pointsPerWidthUnit); 
        int numOfHeightPoints = Mathf.FloorToInt(height * pointsPerHeightUnit);
        int numOfPoints = numOfWidthPoints * numOfHeightPoints;
        for (int i = 0; i < numOfPoints; i++) {
            GameObject point = new GameObject($"{name}:{i}", typeof(AiWaypoint));
            point.transform.parent = transform;
        }
    }
    
    private void DestroyPoints() {
        while (transform.childCount > 0) {
            //Destroy(transform.GetChild(0).gameObject);
        }
    }
    
    private void SyncValues() {
        _width = width;
        _height = height;
        _pointsPerWidthUnit = pointsPerWidthUnit;
        _pointsPerHeightUnit = pointsPerHeightUnit;
        _angleOffset = angleOffset;
    }
    
    private void SyncPoints() {
        int numOfWidthPoints = Mathf.FloorToInt(width * pointsPerWidthUnit); 
        int numOfHeightPoints = Mathf.FloorToInt(height * pointsPerHeightUnit);

        float widthSpacing = width / numOfWidthPoints;
        float heightSpacing = height / numOfHeightPoints;

        float widthOffset = widthSpacing / 2f - width / 2f;
        float heightOffset = widthSpacing / 2f - height / 2f;
        
        for (int i = 0; i < transform.childCount; i++) {

            float x = i % numOfWidthPoints * widthSpacing + widthOffset;
            float z = i / numOfWidthPoints * heightSpacing + heightOffset;

            Transform point = transform.GetChild(i);
            point.localPosition = new Vector3(x, 0f, z);
            point.localRotation = UnityEngine.Quaternion.Euler(0f, angleOffset, 0f);
        }
    }
}