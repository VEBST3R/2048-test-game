using UnityEngine;

public interface ICubeFactory
{
    GameObject GetCube(Vector3 position, Quaternion rotation, Transform parent = null);
    void SetRandomValue(GameObject cube, float probability2 = 0.75f);
}
