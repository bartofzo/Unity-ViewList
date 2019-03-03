/*
 * 
 * 
 * Example 
 * 
 * 
 */

using UnityEngine;
using UnityEngine.UI;

namespace UnityViewList
{
    public class ViewListExample : MonoBehaviour
    {
        public ViewListExampleList listView;
        public Toggle multiSelectToggle;

        // Start is called before the first frame update
        void Start()
        {
            listView.CanMultiselect = multiSelectToggle.isOn;

            for (int i = 0; i < 10; i++)
            {
                listView.AddNew(new ListViewExampleValue());
            }
        }

        public void OnMultiToggleChange(bool value)
        {
            listView.CanMultiselect = value;
        }

        public void OnAddButtonClick()
        {
            listView.AddNew(new ListViewExampleValue());
        }

        public void OnPrintButtonClick()
        {
            // We prove that the output is the same as that what we see in the UI
            Debug.Log("------Printing values in order");
            foreach (var value in listView.Values)
            {
                Debug.Log(value.text);
            }
        }

    }
}