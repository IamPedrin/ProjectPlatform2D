using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class BlockedButton : MonoBehaviour
{
    private Button _blockedButton;
    public float shakeDuration = 2f;
    public float shakeMagnitude = 4f;
    private Vector3 originalPosition;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        _blockedButton = GetComponent<Button>();

        originalPosition = _blockedButton.transform.localPosition;
    }

    public void OnClickButton()
    {
        StartCoroutine(Shake());
    }

    IEnumerator Shake()
    {
        float timePassed = 0f;

        while (timePassed < shakeDuration)
        {
            float moveX = Random.Range(-1f, 1f) * shakeMagnitude;
            float moveY = Random.Range(-1f, 1f) * shakeMagnitude;

            _blockedButton.transform.localPosition = new Vector3(originalPosition.x + moveX, originalPosition.y + moveY, originalPosition.z);

            timePassed += Time.deltaTime;
            yield return null;
        }

        _blockedButton.transform.localPosition = originalPosition;
    }

}
