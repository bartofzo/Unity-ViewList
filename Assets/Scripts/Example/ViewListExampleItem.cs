using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace UnityViewList
{
    // We override ViewList.Item with out custom interface for a list item
    public class ViewListExampleItem : ViewList<ListViewExampleValue>.Item, IPointerClickHandler
    {
        public Text Text;

        // Clicking this item toggles it's selection status
        // This is something we have to manually define for a list item
        public void OnPointerClick(PointerEventData eventData)
        {
            // We toggle Selected. This will trigger OnSelectionChange automatically
            this.Selected = !this.Selected;
            
        }

        // Gets called when this item selection status changes
        public override void OnSelectionChange(bool selected)
        {
            AdjustColor();
        }

        // Initialize UI elements here
        public override void OnInitialize()
        {
            this.Text.text = Value.text;
            AdjustColor(); // init with right color
        }

        private void AdjustColor()
        {
            this.Text.color = this.Selected ? Color.red : Color.white;
        }
    }

    // Example object that we want ordered
    public class ListViewExampleValue
    {
        public string text;
        public ListViewExampleValue()
        {
            this.text = "Random number: " + UnityEngine.Random.Range(0, 100000);
        }
    }
}