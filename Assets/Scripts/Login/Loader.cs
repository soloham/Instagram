using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class Loader : MonoBehaviour
{
    public float fillRate = 5f;
    [SerializeField] private float rotationSpeed = 60f;
    [SerializeField] private Image Image;

    void OnEnable()
    {
        StartCoroutine(StartLoaderAnim());
    }

    // Update is called once per frame
    void Update()
    {
        // Rotate the GameObject smoothly over time
        transform.Rotate(Vector3.forward, rotationSpeed * Time.deltaTime);
    }

    IEnumerator StartLoaderAnim()
    {
        while (true)
        {
            var targetFill = 0f;
            targetFill = Image.fillAmount <= 0.25f ? 1 : Image.fillAmount >= 0.95f ? 0.2f : targetFill;
            float curFill = Image.fillAmount;

            while (Mathf.Abs(curFill - targetFill) > 0.01f)
            {
                Image.fillAmount = Mathf.Lerp(curFill, targetFill, fillRate * Time.deltaTime);
                curFill = Image.fillAmount;
                yield return null;
            }
        }
    }
}
