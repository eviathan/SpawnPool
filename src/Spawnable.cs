using System.Collections.Specialized;
using UnityEngine;

public class Spawnable : MonoBehaviour, ISpawnable
{
    private SpawnPool Pool;

    void Awake()
    {
        Pool = SpawnPool.Current;
    }

    public void OnChange(object sender, NotifyCollectionChangedEventArgs args)
    {

    }

    public void Return()
    {
        Pool.ItemService.ReturnItem(this);
    }
}