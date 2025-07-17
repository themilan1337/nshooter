using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI; // Обязательно для работы с UI элементами, как Slider

public class LoadingScreenController : MonoBehaviour
{
    [Header("UI Элементы")]
    public Slider progressBar; // Сюда перетащим наш Slider

    // Статическое поле для хранения имени сцены, которую нужно загрузить.
    // Это делает наш загрузчик универсальным.
    public static string sceneToLoad;

    void Start()
    {
        // Проверяем, было ли установлено имя сцены
        if (string.IsNullOrEmpty(sceneToLoad))
        {
            Debug.LogError("Имя сцены для загрузки не установлено! Загрузка главной сцены по умолчанию.");
            // В качестве запасного варианта можно загрузить, например, меню
            // SceneManager.LoadScene("MainMenu"); 
            return;
        }

        // Запускаем корутину (асинхронный процесс) для загрузки
        StartCoroutine(LoadSceneAsync());
    }

    IEnumerator LoadSceneAsync()
    {
        // Запускаем асинхронную загрузку сцены
        AsyncOperation operation = SceneManager.LoadSceneAsync(sceneToLoad);

        // Цикл будет работать, пока сцена не загрузится полностью
        while (!operation.isDone)
        {
            // operation.progress изменяется от 0.0 до 0.9.
            // Когда сцена загружена, но еще не активирована, progress равен 0.9.
            // Чтобы получить прогресс от 0 до 1, делим на 0.9.
            float progressValue = Mathf.Clamp01(operation.progress / 0.9f);
            
            // Обновляем значение нашего слайдера
            progressBar.value = progressValue;

            // Ждем следующего кадра, прежде чем продолжить цикл
            yield return null;
        }
    }
}