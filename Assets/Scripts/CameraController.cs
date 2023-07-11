using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    public Transform target;
    public float lerpSpeed = 1.0f;

    private Vector3 offset;

    private Vector3 targetPos;
    private Vector3 mousePos;

    public static bool shaking;
    // Start is called before the first frame update
    void Start()
    {
        target = GameObject.FindGameObjectWithTag("Player").transform;
        offset = transform.position - target.position;
        shaking = false;
    }

    // Update is called once per frame
    void Update()
    {
        if (target == null) return;

        mousePos = GetComponentInChildren<Camera>().ScreenToWorldPoint(Input.mousePosition);
        Vector3 mouseOffset = mousePos - targetPos;

        targetPos = target.position + offset + 0.3f * mouseOffset;
        if (!shaking)
        {
            transform.position = Vector3.Lerp(transform.position, targetPos, lerpSpeed * Time.deltaTime);
        }
    }

    public static IEnumerator cameraShake(float duration, float magnitude)
    {
        Vector3 originalPos = GameObject.FindGameObjectWithTag("MainCamera")
            .transform.localPosition;

        float elapsed = 0f;
        shaking = true;

        while (elapsed < duration)
        {
            float x = Random.Range(-1f, 1f) * magnitude;
            float y = Random.Range(-1f, 1f) * magnitude;

            GameObject.FindGameObjectWithTag("MainCamera").transform.localPosition = new Vector3(x, y, originalPos.z);

            elapsed += Time.deltaTime;
            yield return null;
        }

        shaking = false;
        GameObject.FindGameObjectWithTag("MainCamera").transform.localPosition = originalPos;
    }
}
