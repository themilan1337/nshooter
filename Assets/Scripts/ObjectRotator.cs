using UnityEngine;

public class ObjectRotator : MonoBehaviour
{
    [Tooltip("Скорость вращения вокруг оси X")]
    public float xSpeed = 0f;

    [Tooltip("Скорость вращения вокруг оси Y (вращение по кругу)")]
    public float ySpeed = 50f;

    [Tooltip("Скорость вращения вокруг оси Z")]
    public float zSpeed = 0f;

    void Update()
    {
        transform.Rotate(xSpeed * Time.deltaTime, ySpeed * Time.deltaTime, zSpeed * Time.deltaTime);
    }
}