using System.Collections;
using TMPro;
using UnityEngine;

public class DamageFontUI : MonoBehaviour
{
    [SerializeField] private string damageStr;
    [SerializeField] private TextMeshProUGUI text;
    [SerializeField] private Canvas canvas;

    public void SetDamage(int damage, Vector3 worldPosition)
    {
        damageStr = damage.ToString();
        transform.position = worldPosition;
        StartCoroutine(UpdateDamageFont());
    }

    private IEnumerator UpdateDamageFont()
    {
        Debug.Log($"DamageFontUI: Starting UpdateDamageFont");
        text.text = damageStr;
        yield return new WaitForSeconds(1.0f);
        ObjectPoolManager.Instance.Push(this);
        Debug.Log($"DamageFontUI: End UpdateDamageFont");
    }

    void LateUpdate()
    {
        if (Vector3.Distance(transform.position, Camera.main.transform.position) < 100.0f)
        {
            // 나 바라보도록
            transform.rotation = Quaternion.LookRotation(transform.position - Camera.main.transform.position);
            canvas.enabled = true;
        }
        else
        {
            canvas.enabled = false;
        }
    }
}
