/*
 * 
 *
 *  Example implementation of the ViewList. 
 *  
 *  Derive the class from ViewList with your custom object type.
 * 
 * 
 */

using UnityEngine;

namespace UnityViewList
{
    public class ViewListExampleList : ViewList<ListViewExampleValue>
    {
        public GameObject Prefab;

        // We provide our custom InstantiateNewItem function that returns an object that takes care of the UI of each list item
        public override Item InstantiateNewItem()
        {
            return Instantiate(Prefab).GetComponent<ViewListExampleItem>();
            
        }
    }
}