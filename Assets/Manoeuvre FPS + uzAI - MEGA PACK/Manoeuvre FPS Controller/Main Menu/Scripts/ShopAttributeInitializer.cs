using UnityEngine;
using UnityEngine.UI;

namespace Manoeuvre
{
    public class ShopAttributeInitializer : MonoBehaviour
    {

        public Text AttributeNameText;
        public Text AttributeValueText;
        public Text AttributeUpgradeValueText;

        [Space]
        public Slider AttributeUpgradePreviewSlider;
        public Slider AttributeSlider;

        /// <summary>
        /// This will be called on 3 occassions.
        /// 1. When the Shop  is open for the very first timee and the first weapon is selected.
        /// 2. Whenever the next or previous weapon is selected.
        /// 3. Whenever the player upgrade the current selected weapon, its value will also be updated.
        /// </summary>
        public void SetAttributeUI(string _atName, float _atValue, float _atUpgradeValue, float _atMaxValue, float _atMinValue, bool isMax)
        {
            //set name and values text
            AttributeNameText.text = _atName;
            AttributeValueText.text = _atValue.ToString();

            if(!isMax)
                AttributeUpgradeValueText.text = "[" + _atUpgradeValue.ToString() + "]";
            else
                AttributeUpgradeValueText.text = "[MAX]";

            //set upgrade slider value
            AttributeUpgradePreviewSlider.minValue = _atMinValue;
            AttributeUpgradePreviewSlider.maxValue = _atMaxValue;
            AttributeUpgradePreviewSlider.value = _atUpgradeValue;

            //set slider value
            AttributeSlider.minValue = _atMinValue;
            AttributeSlider.maxValue = _atMaxValue;
            AttributeSlider.value = _atValue;
        }

    }
}