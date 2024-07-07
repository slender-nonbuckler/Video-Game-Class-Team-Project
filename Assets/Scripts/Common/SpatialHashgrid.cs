using System.Collections.Generic;
using UnityEngine;

namespace Common {
    public class SpatialHashgrid {
        private Dictionary<Vector2Int, HashSet<Transform>> cells = new Dictionary<Vector2Int, HashSet<Transform>>();
        private Bounds bounds;
        private Vector2Int dimensions;

        public SpatialHashgrid(Bounds bounds, Vector2Int dimensions) {
            this.bounds = bounds;
            this.dimensions = dimensions;

            for (int i = 0; i < this.dimensions.x; i++) {
                for (int j = 0; j < this.dimensions.y; j++) {
                    Vector2Int cellIndex = new Vector2Int(i, j);
                    cells[cellIndex] = new HashSet<Transform>();
                }
            }
        }

        public HashSet<Transform> GetNearby(Vector3 position, float radius) {
            HashSet<Transform> nearby = new HashSet<Transform>();
            Vector2Int minCellIndex = PositionToCellIndex(new Vector3(position.x - radius, 0, position.z - radius));
            Vector2Int maxCellIndex = PositionToCellIndex(new Vector3(position.x + radius, 0, position.z + radius));
            
            for (int i = minCellIndex.x; i <= maxCellIndex.x; i++) {
                for (int j = minCellIndex.y; j <= maxCellIndex.y; j++) {
                    Vector2Int cellIndex = new Vector2Int(i, j);
                    if (IsInSpatialGrid(cellIndex)) {
                        nearby.UnionWith(cells[cellIndex]);
                    }
                }
            }
            
            return nearby;
        }

        public void AddItem(Transform transform) {
            Vector2Int cellIndex = PositionToCellIndex(transform.position);
            if (IsInSpatialGrid(cellIndex)) {
                cells[cellIndex].Add(transform);
            }
        }

        public void RemoveItem(Transform transform) {
            Vector2Int cellIndex = PositionToCellIndex(transform.position);
            if (IsInSpatialGrid(cellIndex)) {
                cells[cellIndex].Remove(transform);
            }
        }

        private Vector2Int PositionToCellIndex(Vector3 position) {
            float x = (position.x - bounds.min.x) / bounds.size.x;
            float z = (position.z - bounds.min.z) / bounds.size.z;

            return new Vector2Int(
                Mathf.FloorToInt(x * dimensions.x - 1),
                Mathf.FloorToInt(z * dimensions.y - 1)
            );
        }

        private bool IsInSpatialGrid(Vector2Int index) {
            return !(index.x < 0
                     || index.y < 0
                     || index.x >= dimensions.x
                     || index.y >= dimensions.y);
        }
    }
}