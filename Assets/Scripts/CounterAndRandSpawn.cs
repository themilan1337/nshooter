using UnityEngine;
using UnityEngine.SceneManagement; // Необходимо для управления сценами
using TMPro; // Необходимо для работы с текстом TextMeshPro

public class CounterAndRandSpawn : MonoBehaviour
{
    [Header("Настройки Врага")]
    [Tooltip("Отметьте, если этот враг является боссом")]
    public bool isBoss = false; // Галочка, видимая в Инспекторе

    // --- Статические переменные (общие для всех врагов) ---
    
    // Ссылка на текстовый элемент UI для отображения счетчика
    private static TextMeshProUGUI bossCounterText; 
    
    // Общий счетчик оставшихся боссов
    private static int bossesRemaining = 0;

    /// <summary>
    /// Этот метод регистрирует текстовое поле для счетчика.
    /// Он должен быть вызван из другого скрипта, который висит на объекте с текстом.
    /// </summary>
    public static void RegisterCounterText(TextMeshProUGUI textElement)
    {
        bossCounterText = textElement;
        Debug.Log("UI для счетчика боссов успешно зарегистрирован.");
    }
    
    // Этот метод вызывается один раз, когда объект появляется на сцене.
    void Awake()
    {
        // Если это босс, увеличиваем счетчик
        if (isBoss)
        {
            bossesRemaining++;
            Debug.Log("Босс появился. Всего боссов на сцене: " + bossesRemaining);
        }
        // Обновляем текст на экране
        UpdateBossCounterUI();
    }

    // Этот метод вызывается, когда объект с этим скриптом уничтожается.
    void OnDestroy()
    {
        // Если это был босс, уменьшаем счетчик
        if (isBoss)
        {
            bossesRemaining--;
            Debug.Log("Босс уничтожен. Осталось боссов: " + bossesRemaining);
            
            // Обновляем текст на экране
            UpdateBossCounterUI();

            // Проверяем, не осталось ли боссов на сцене.
            if (bossesRemaining <= 0)
            {
                Debug.Log("Все боссы уничтожены! Загрузка сцены 'Ending'...");
                // Обнуляем счетчик перед загрузкой новой сцены, чтобы избежать проблем при перезапуске
                bossesRemaining = 0; 
                // Загружаем сцену с названием "Ending". Убедитесь, что она добавлена в Build Settings.
                SceneManager.LoadScene("Ending");
            }
        }
    }

    /// <summary>
    /// Обновляет текст счетчика на экране.
    /// </summary>
    private static void UpdateBossCounterUI()
    {
        if (bossCounterText != null)
        {
            // Отображаем, только если есть хотя бы один босс
            if (bossesRemaining > 0)
            {
                bossCounterText.gameObject.SetActive(true); // Показываем текст
                bossCounterText.text = "Осталось боссов: " + bossesRemaining;
            }
            else
            {
                bossCounterText.gameObject.SetActive(false); // Прячем текст, когда боссов нет
            }
        }
    }
}