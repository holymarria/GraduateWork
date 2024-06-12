using UnityEngine;
using UnityEngine.EventSystems;

public class RawImageTrigger : MonoBehaviour, IPointerClickHandler
{
    // Метод, который будет вызван при щелчке на RawImage
    public void OnPointerClick(PointerEventData eventData)
    {
        Debug.Log("RawImage Clicked: " + gameObject.name);
        // Добавьте сюда свой код для выполнения действия при нажатии на RawImage
    }
}
