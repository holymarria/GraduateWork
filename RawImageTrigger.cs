using UnityEngine;
using UnityEngine.EventSystems;

public class RawImageTrigger : MonoBehaviour, IPointerClickHandler
{
    // �����, ������� ����� ������ ��� ������ �� RawImage
    public void OnPointerClick(PointerEventData eventData)
    {
        Debug.Log("RawImage Clicked: " + gameObject.name);
        // �������� ���� ���� ��� ��� ���������� �������� ��� ������� �� RawImage
    }
}
