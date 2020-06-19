using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class NoticeText : MonoBehaviour
{
    private Text Text;

    // Start is called before the first frame update
    void Start()
    {
        Text = GetComponentInChildren<Text>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public IEnumerator ShowText(string text, float time)
    {
        Text.text = text;
        yield return new WaitForSeconds(time);
        Text.text = "";
    }
}
