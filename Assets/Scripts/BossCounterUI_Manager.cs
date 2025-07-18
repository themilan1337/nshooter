using UnityEngine;
using TMPro; // Обязательно добавьте эту строку

public class BossCounterUI_Manager : MonoBehaviour
{
    void Awake()
    {
        // Находим компонент текста на этом же объекте
        TextMeshProUGUI textElement = GetComponent<TextMeshProUGUI>();

        if (textElement != null)
        {
            // Передаем (регистрируем) наш текстовый компонент в основной скрипт счетчика
            CounterAndRandSpawn.RegisterCounterText(textElement);
        }
        else
        {
            Debug.LogError("На объекте не найден компонент TextMeshProUGUI!");
        }
    }
}