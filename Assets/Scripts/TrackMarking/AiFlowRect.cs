using UnityEngine;

[ExecuteInEditMode]
public class AiFlowRect : MonoBehaviour {
    [SerializeField] private float width = 100f;
    private float _width;
    [SerializeField] private float length = 100f;
    private float _height;
    [SerializeField] private float pointsPerWidthUnit = 0.1f;
    private float _pointsPerWidthUnit;
    [SerializeField] private float pointsPerLenghtUnit = 0.1f;
    private float _pointsPerHeightUnit;
    [SerializeField] private float angleOffset;
    private float _angleOffset;

    [SerializeField] private float marginWidth = 10f;
    private float _marginWidth;
    [SerializeField] private float marginAngleOffset;
    private float _marginAngleOffset;

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
            || length != _height
            || marginWidth != _marginWidth
            || pointsPerWidthUnit != _pointsPerWidthUnit
            || pointsPerLenghtUnit != _pointsPerHeightUnit
        );
    }

    private bool isChanged() {
        return (
            width != _width
            || length != _height
            || pointsPerWidthUnit != _pointsPerWidthUnit
            || pointsPerLenghtUnit != _pointsPerHeightUnit
            || angleOffset != _angleOffset
            || marginWidth != _marginWidth
            || marginAngleOffset != _marginAngleOffset
        );
    }

    private void MakePoints() {
        float totalWidth = width + marginWidth * 2;
        int numOfWidthPoints = Mathf.FloorToInt(totalWidth * pointsPerWidthUnit);
        int numOfHeightPoints = Mathf.FloorToInt(length * pointsPerLenghtUnit);
        int numOfPoints = numOfWidthPoints * numOfHeightPoints;
        for (int i = 0; i < numOfPoints; i++) {
            GameObject point = new GameObject($"{name}:{i}", typeof(AiWaypoint));
            point.transform.parent = transform;
        }
    }

    private void DestroyPoints() {
        while (transform.childCount > 0) {
            DestroyImmediate(transform.GetChild(0).gameObject);
        }
    }

    private void SyncValues() {
        _width = width;
        _height = length;
        _pointsPerWidthUnit = pointsPerWidthUnit;
        _pointsPerHeightUnit = pointsPerLenghtUnit;
        _angleOffset = angleOffset;
        _marginWidth = marginWidth;
        _marginAngleOffset = marginAngleOffset;
    }

    private void SyncPoints() {
        float totalWidth = width + marginWidth * 2;
        int numOfWidthPoints = Mathf.FloorToInt(totalWidth * pointsPerWidthUnit);
        int numOfHeightPoints = Mathf.FloorToInt(length * pointsPerLenghtUnit);

        float widthSpacing = totalWidth / numOfWidthPoints;
        float heightSpacing = length / numOfHeightPoints;

        float widthOffset = widthSpacing / 2f - totalWidth / 2f;
        // float heightOffset = widthSpacing / 2f - height / 2f;

        for (int i = 0; i < transform.childCount; i++) {
            float x = i % numOfWidthPoints * widthSpacing + widthOffset;
            float z = i / numOfWidthPoints * heightSpacing;

            Transform point = transform.GetChild(i);
            point.localPosition = new Vector3(x, 0f, z);
            point.localRotation = Quaternion.Euler(0f, GetRotation(point) + angleOffset, 0f);
        }
    }

    private float GetRotation(Transform point) {
        float xPosition = point.localPosition.x;
        if (Mathf.Abs(xPosition) < width / 2f) {
            return 0f;
        }

        float distFromCore = Mathf.Abs(xPosition) - width / 2f;
        float percentInMargin = distFromCore / marginWidth;

        return Mathf.LerpAngle(0f, marginAngleOffset, percentInMargin) * Mathf.Sign(xPosition);
    }
}