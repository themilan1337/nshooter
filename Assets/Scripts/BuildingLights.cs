using UnityEngine;

public class BuildingLights : MonoBehaviour {
    // Укажите в инспекторе, какой элемент в массиве материалов отвечает за окна
    public int windowMaterialIndex;
    
    // Перетащите сюда материал для ВКЛЮЧЕННЫХ окон
    public Material lightsOnMaterial;
    
    // Перетащите сюда материал для ВЫКЛЮЧЕННЫХ окон
    public Material lightsOffMaterial;

    // Галочка для управления светом из инспектора
    public bool areLightsOn;

    private MeshRenderer meshRenderer;
    private Material[] originalMaterials;

    private void Start() {
        meshRenderer = GetComponent<MeshRenderer>();
        
        // Сохраняем копию оригинальных материалов, чтобы не терять их
        originalMaterials = meshRenderer.materials;
        
        // Устанавливаем начальное состояние
        SetLights(areLightsOn);
    }

    /// <summary>
    /// Включает или выключает свет в окнах, меняя материал.
    /// </summary>
    /// <param name="isOn">True - включить, False - выключить.</param>
    public void SetLights(bool isOn) {
        if (meshRenderer == null || lightsOnMaterial == null || lightsOffMaterial == null) {
            Debug.LogWarning("Компоненты не назначены в скрипте BuildingLights!", this);
            return;
        }

        // Важно: нужно получить массив материалов, изменить его и присвоить обратно.
        // Нельзя менять напрямую meshRenderer.materials[index].
        Material[] currentMaterials = meshRenderer.materials;
        
        if (windowMaterialIndex < 0 || windowMaterialIndex >= currentMaterials.Length) {
            Debug.LogError("windowMaterialIndex выходит за пределы массива материалов!", this);
            return;
        }

        // Меняем нужный материал в массиве
        currentMaterials[windowMaterialIndex] = isOn ? lightsOnMaterial : lightsOffMaterial;

        // Присваиваем измененный массив обратно рендереру
        meshRenderer.materials = currentMaterials;
        
        // Обновляем состояние флага
        areLightsOn = isOn;
    }
    
    // Дополнительно: добавим метод, чтобы можно было удобно тестировать в редакторе
    private void OnValidate()
    {
        // Эта проверка нужна, чтобы код не выполнялся до запуска сцены
        if (meshRenderer != null)
        {
            SetLights(areLightsOn);
        }
    }
}