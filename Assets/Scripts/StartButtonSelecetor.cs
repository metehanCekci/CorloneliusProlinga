using UnityEngine;
using UnityEngine.EventSystems; // EventSystem için gerekli
using UnityEngine.UI;

public class ForceUISelection : MonoBehaviour
{
    public GameObject targetButton; // İlk seçilmesini istediğin buton

    void Start()
    {
        // Başlangıçta butonu seç
        SelectTarget();
    }

    void Update()
    {
        // Eğer mouse ile dışarı tıklandıysa ve seçim kaybolduysa
        if (EventSystem.current.currentSelectedGameObject == null)
        {
            SelectTarget();
        }
    }

    private void SelectTarget()
    {
        if (targetButton != null)
        {
            EventSystem.current.SetSelectedGameObject(targetButton);
        }
    }
}
