using TMPro;

using UnityEngine;

public class MessagesLoader : MonoBehaviour
{
    [SerializeField] private Transform ImageTransform;

    [SerializeField] private float rotationSpeed = 60f;
    [SerializeField] private float resetSpeed = 120f;

    public TextMeshProUGUI TimestampTMP;

    public void Initialise(string timestamp)
    {
        TimestampTMP.text = timestamp;
    }

    private void Update()
    {
        // Rotate the GameObject smoothly over time
        ImageTransform.Rotate(Vector3.forward, rotationSpeed * Time.deltaTime);
    }
}
