using System.Collections;
using UnityEngine;
using TMPro; // Используем TextMeshPro
using UnityEngine.SceneManagement;

public class CutsceneController : MonoBehaviour
{
    [Header("Ссылки на объекты")]
    // Перетащите сюда ваш текстовый объект из иерархии
    public TextMeshProUGUI cutsceneText;

    [Header("Настройки сцены")]
    // Имя сцены, которая загрузится после катсцены
    public string nextSceneName = "Game";

    [Header("Текст Катсцены")]
    // Здесь в инспекторе вы напишете фразы для катсцены
    [TextArea(3, 5)] // Делает поле в инспекторе удобнее для ввода текста
    public string[] dialogueLines;

    [Header("Настройки времени")]
    // Длительность появления/исчезновения
    public float fadeDuration = 1.5f;
    // Сколько времени текст будет виден на экране после появления
    public float displayTime = 3.0f;
    // Пауза между фразами
    public float timeBetweenLines = 0.5f;

    // --- Переменные для камеры ---
    private Camera mainCamera;
    private CameraClearFlags originalClearFlags;
    private Color originalBackgroundColor;

    void Start()
    {
        // 1. Настраиваем камеру для черного фона
        SetupCamera();

        // 2. Проверяем, все ли настроено правильно
        if (cutsceneText == null)
        {
            Debug.LogError("ОШИБКА: Поле 'Cutscene Text' не заполнено в инспекторе! Перетащите туда текстовый объект.");
            // Можно даже остановить игру в редакторе, чтобы вы точно заметили ошибку
            #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
            #endif
            return; // Прекращаем выполнение, если текст не назначен
        }

        if (dialogueLines == null || dialogueLines.Length == 0)
        {
            Debug.LogWarning("ВНИМАНИЕ: Нет фраз для катсцены (массив 'Dialogue Lines' пуст). Загружаю следующую сцену немедленно.");
            LoadNextScene();
            return; // Сразу загружаем след. сцену, если нет диалогов
        }

        // 3. Если все в порядке, запускаем катсцену
        StartCoroutine(PlayCutscene());
    }

    void OnDestroy()
    {
        // Возвращаем камере оригинальные настройки, когда сцена выгружается
        RestoreCameraSettings();
    }

    private void SetupCamera()
    {
        mainCamera = Camera.main;
        if (mainCamera != null)
        {
            // Сохраняем оригинальные настройки
            originalClearFlags = mainCamera.clearFlags;
            originalBackgroundColor = mainCamera.backgroundColor;

            // Устанавливаем новые настройки для черного фона
            mainCamera.clearFlags = CameraClearFlags.SolidColor;
            mainCamera.backgroundColor = Color.black;
        }
    }

    private void RestoreCameraSettings()
    {
        if (mainCamera != null)
        {
            mainCamera.clearFlags = originalClearFlags;
            mainCamera.backgroundColor = originalBackgroundColor;
        }
    }

    private IEnumerator PlayCutscene()
    {
        // В начале текст полностью прозрачен
        cutsceneText.color = new Color(cutsceneText.color.r, cutsceneText.color.g, cutsceneText.color.b, 0);

        // Проходим по каждой строке диалога
        foreach (string line in dialogueLines)
        {
            cutsceneText.text = line;

            // Плавно показываем текст (alpha от 0 до 1)
            yield return StartCoroutine(FadeText(1f));

            // Ждем displayTime секунд
            yield return new WaitForSeconds(displayTime);

            // Плавно скрываем текст (alpha от 1 до 0)
            yield return StartCoroutine(FadeText(0f));

            // Небольшая пауза перед следующей фразой
            yield return new WaitForSeconds(timeBetweenLines);
        }

        // После того как все фразы показаны, загружаем следующую сцену
        LoadNextScene();
    }

    private IEnumerator FadeText(float targetAlpha)
    {
        float timer = 0f;
        Color startColor = cutsceneText.color;

        while (timer < fadeDuration)
        {
            timer += Time.deltaTime;
            float progress = Mathf.Clamp01(timer / fadeDuration);
            float newAlpha = Mathf.Lerp(startColor.a, targetAlpha, progress);
            cutsceneText.color = new Color(startColor.r, startColor.g, startColor.b, newAlpha);
            yield return null;
        }

        cutsceneText.color = new Color(startColor.r, startColor.g, startColor.b, targetAlpha);
    }

    private void LoadNextScene()
    {
        // Перед загрузкой новой сцены вернем настройки камеры
        RestoreCameraSettings();
        SceneManager.LoadScene(nextSceneName);
    }
}