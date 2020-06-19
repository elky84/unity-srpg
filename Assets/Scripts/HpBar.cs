using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

public class HpBar : MonoBehaviour
{
    private Slider _hpSlider;
    private Text HpText { get; set; }

    private Image _hpSliderFill;

    public void SetValue(float value)
    {
        _hpSlider.value = value;
    }

    void Awake()
    {
        _hpSlider = this.GetComponentInChildren<Slider>();
        HpText = transform.Find("Hp/Text").GetComponent<Text>();
        _hpSliderFill = _hpSlider.transform.Find("Fill Area/Fill").GetComponent<Image>();
    }

    public void SetHpText<T>(T hp, T maxHp)
    {
        HpText.text = hp.ToString() + "/" + maxHp.ToString();
    }

    public void SetColor(Color color)
    {
        _hpSliderFill.color = color;
    }
}