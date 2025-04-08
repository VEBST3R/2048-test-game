using UnityEngine;
using System;
using System.Collections;

public class CubeMergeHandler : MonoBehaviour
{
    public event Action<GameObject, GameObject> OnCubeMerged;

    [SerializeField] private float minCollisionForce = 2f;
    private CubeFacade cubeFacade;
    private bool hasBeenMerged = false;

    private void Awake()
    {
        cubeFacade = GetComponent<CubeFacade>();
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (!cubeFacade) return;

        GameObject otherObject = collision.gameObject;
        CubeFacade otherFacade = otherObject.GetComponent<CubeFacade>();

        if (!otherFacade) return;

        Debug.Log($"Collision: This={cubeFacade.GetValue()}, Other={otherFacade.GetValue()}, " +
                 $"Force={collision.relativeVelocity.magnitude}, ThisMerged={hasBeenMerged}, " +
                 $"OtherMerged={otherFacade.GetMergeHandler().IsMerged()}");

        float requiredForce = cubeFacade.GetValue() <= 4 ? minCollisionForce * 0.7f : minCollisionForce;
        if (collision.relativeVelocity.magnitude < requiredForce) return;

        CubeMergeHandler otherHandler = otherFacade.GetMergeHandler();
        if (!otherHandler) return;

        if (!hasBeenMerged && !otherHandler.IsMerged() && otherFacade.GetValue() == cubeFacade.GetValue())
        {
            MergeWithCube(otherFacade);
        }
    }

    private void MergeWithCube(CubeFacade otherFacade)
    {
        hasBeenMerged = true;
        otherFacade.GetMergeHandler().SetMerged(true);

        int newValue = cubeFacade.GetValue() * 2;
        cubeFacade.SetValue(newValue);

        cubeFacade.GetAnimationController().PlayMergeAnimation();

        OnCubeMerged?.Invoke(gameObject, otherFacade.gameObject);

        IScoreManager scoreManager = GameStateManager.Instance;
        if (scoreManager != null)
        {
            scoreManager.AddScore(newValue / 2);

            if (newValue >= 2048)
            {
                IGameStateManager gameStateManager = GameStateManager.Instance;
                if (gameStateManager != null)
                {
                    gameStateManager.WinGame();
                }
            }
        }
        else
        {
            Debug.LogError("ScoreManager не знайдено! Очки не будуть додані.");
        }

        DisableCubeComponents(otherFacade.gameObject);

        if (ObjectPooler.Instance != null)
        {
            StartCoroutine(ReturnToPoolDelayed(otherFacade.gameObject));
        }
        else
        {
            Destroy(otherFacade.gameObject);
        }

        Invoke(nameof(ResetMergeState), 0.1f);
    }

    public bool IsMerged()
    {
        return hasBeenMerged;
    }

    public void SetMerged(bool merged)
    {
        hasBeenMerged = merged;
    }

    public void ResetMergeState()
    {
        hasBeenMerged = false;
    }

    private IEnumerator ReturnToPoolDelayed(GameObject objectToReturn)
    {
        yield return new WaitForFixedUpdate();

        if (objectToReturn != null && ObjectPooler.Instance != null)
        {
            CubeFacade otherFacade = objectToReturn.GetComponent<CubeFacade>();
            if (otherFacade != null)
            {
                otherFacade.SetInPool(true);
            }

            objectToReturn.SetActive(false);
            objectToReturn.transform.position = new Vector3(1000, 1000, 1000);
            ObjectPooler.Instance.ReturnToPool("Cube", objectToReturn);

            Debug.Log($"Куб зі значенням {cubeFacade.GetValue()} повернуто у пул");
        }
    }

    public void DisableCubeComponents(GameObject cube)
    {
        Rigidbody rb = cube.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.Sleep();
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            rb.isKinematic = true;
        }

        Collider col = cube.GetComponent<Collider>();
        if (col != null)
        {
            col.enabled = false;
        }

        MeshRenderer renderer = cube.GetComponent<MeshRenderer>();
        if (renderer != null)
        {
            renderer.enabled = false;
        }
    }

    public void EnableComponents()
    {
        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = false;
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }

        Collider col = GetComponent<Collider>();
        if (col != null)
        {
            col.enabled = true;
        }

        MeshRenderer renderer = GetComponent<MeshRenderer>();
        if (renderer != null)
        {
            renderer.enabled = true;
        }
    }
}
