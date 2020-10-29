using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using UnityEngine;
using Pool = System.Collections.Generic.Dictionary<System.Type,
    (
        System.Collections.ObjectModel.ObservableCollection<ISpawnable> Collection,
        PoolConfig Config,
        UnityEngine.GameObject Folder, 
        SpawnPoolEvents PoolEvents
    )>;

public class SpawnItemService
{
    Pool Pool;
    
    public SpawnItemService(Pool pool)
    {
        Pool = pool;
    }

    #region Adding
    public T AddItem<T>() where T : class, ISpawnable
    {
        var pool = Pool[typeof(T)];
        var poolConfig = SpawnPool.Current.GetConfigFor<T>();
        
        var obj = GameObject.Instantiate(poolConfig.Object, pool.Folder.transform.position, pool.Folder.transform.rotation);
        obj.name = poolConfig.Object.name;
        obj.transform.parent = pool.Folder.transform;
        
        var item = obj.GetComponent(typeof(ISpawnable)) as ISpawnable;
        pool.Collection.Add(item);

        var component = (item as MonoBehaviour);
        component.gameObject.transform.position = pool.Folder.transform.position;
        component.gameObject.transform.parent = pool.Folder.transform;

        if(pool.Collection.Count == pool.Config.Amount) pool.PoolEvents.DidFillPool(pool.Collection);
        else if(pool.Collection.Count > pool.Config.Amount) pool.PoolEvents.DidOverflowPool(pool.Collection);

        return (T)item;
    }

    public IEnumerable<T> AddItems<T>(int amount) where T : class, ISpawnable
    {
        var items = new List<T>();

        for (int i = 0; i < amount; i++)
        {
            items.Add(AddItem<T>());
        }

        return items;
    }
    #endregion

    #region Leasing
    public T LeaseItemOrDefault<T>(string name = null, bool createNewIfEmpty = false, Action<T> prepareItem = null) where T : class, ISpawnable
    {
        var pool = Pool[typeof(T)];
        var item = pool.Collection.Count > 0 
            ? (T)pool.Collection[0] 
            : createNewIfEmpty 
                ? AddItem<T>()
                : null;
        
        if(item != null)
        {
            var component = (item as MonoBehaviour);
            component.name = name ?? item.GetType().Name;
            component.transform.parent = null;

            prepareItem.Invoke(item);

            pool.Collection.Remove(item);
        }
        else
        {   
            if(pool.Collection.Count == 0) pool.PoolEvents.DidEmptyPool(pool.Collection);
        }
        
        if(!createNewIfEmpty && pool.Collection.Count <= pool.Config.RunningLowAmount) pool.PoolEvents.DidRunLow(pool.Collection);

        return item;
    }

    public IEnumerable<T> LeaseItems<T>(int amount = 1, bool createNewIfEmpty = false, Action<T> prepareItem = null) where T : class, ISpawnable
    {
        var pool = Pool[typeof(T)];
        var items = new List<T>();

        if(createNewIfEmpty && items.Count == 0)
        {
            var item = LeaseItemOrDefault<T>(createNewIfEmpty: createNewIfEmpty, prepareItem: prepareItem);
            if(item != null) items.Add(item);
        }
        else
        {
            while(pool.Collection.Count > 0 && items.Count < amount) 
            {
                var item = LeaseItemOrDefault<T>(createNewIfEmpty: createNewIfEmpty, prepareItem: prepareItem);
                if(item != null) items.Add(item);
            }
        }
        
        return items;
    }

    public bool TryLeaseItems<T>(out List<T> items, int amount = 1, bool createNewIfEmpty = false, Action<T> prepareItem = null) where T: class, ISpawnable
    {
        var pool = Pool[typeof(T)];
        items = new List<T>();

        if(createNewIfEmpty && items.Count == 0)
        {
            var item = LeaseItemOrDefault<T>(createNewIfEmpty: createNewIfEmpty, prepareItem: prepareItem);
            if(item != null) items.Add(item);
        }
        else
        {
            while(pool.Collection.Count > 0 && items.Count < amount) 
            {
                var item = LeaseItemOrDefault<T>(createNewIfEmpty: createNewIfEmpty, prepareItem: prepareItem);
                if(item != null) items.Add(item);
            }
        }

        return items != null && items.Count > 0;
    }
    #endregion

    #region Returning
    public void ReturnItem<T>(T item) where T : class, ISpawnable
    {
        var pool = Pool[item.GetType()];
        pool.Collection.Add(item);

        var component = (item as MonoBehaviour);
        component.gameObject.transform.position = pool.Folder.transform.position;
        component.gameObject.transform.parent = pool.Folder.transform;

        if(pool.Collection.Count == pool.Config.Amount) pool.PoolEvents.DidFillPool(pool.Collection);
        else if(pool.Collection.Count > pool.Config.Amount) pool.PoolEvents.DidOverflowPool(pool.Collection);
    }
    #endregion
}
