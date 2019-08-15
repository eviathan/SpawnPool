using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using UnityEngine;
using Pool = System.Collections.Generic.Dictionary<System.Type,
    (
        System.Collections.ObjectModel.ObservableCollection<ISpawnable> Collection,
        PoolConfig Config,
        UnityEngine.GameObject
        Folder, SpawnPoolEvents
        PoolEvents
    )>;

public class SpawnPool : MonoBehaviour
{
    private static SpawnPool _current = null;
    private static readonly object padlock = new object();
    public static SpawnPool Current
    { 
        get
        {
            lock(padlock)
            {
                if(_current == null) _current = GameObject.FindWithTag("SpawnPool").GetComponent<SpawnPool>();
                return _current;
            }
        }
    }

    public List<PoolConfig> Configurations;

    public SpawnItemService ItemService { get; private set; }

    private Pool _pool = new Pool();

    private void Awake()
    {
        ItemService = new SpawnItemService(_pool);
        Configurations.ForEach(CreateItemPool);
    }

    private void CreateItemPool(PoolConfig config)
    {
        var spawnable = config.Object.GetComponent(typeof(ISpawnable)) as ISpawnable;
        config.Type = spawnable.GetType();

        if(spawnable != null)
        {
            var folder = new GameObject(spawnable.GetType().Name);
            folder.transform.parent = transform;

            if(!_pool.ContainsKey(spawnable.GetType()))
            {
                _pool[spawnable.GetType()] = (
                    Collection: new ObservableCollection<ISpawnable>(),
                    Config: config,
                    Folder: folder,
                    PoolEvents: new SpawnPoolEvents()
                );
            }

            _pool[spawnable.GetType()].Collection.CollectionChanged += spawnable.OnChange;

            typeof(SpawnItemService).GetMethod("AddItems")
                                    .MakeGenericMethod(spawnable.GetType())
                                    .Invoke(ItemService, new object[] { config.Amount });
        }
        else
        {
            throw new Exception($"Pool item must have a Component that extends {nameof(Spawnable)} or implementes {nameof(ISpawnable)}");
        }
    }

    public PoolConfig GetConfigFor<T> ()
    {
        return Configurations.FirstOrDefault(i => i.Type == typeof(T));
    }

    public  ObservableCollection<T> GetCollectionFor<T>() where T: class, ISpawnable
    {
        return _pool[typeof(T)].Collection as ObservableCollection<T>; 
    }

    public SpawnPoolEvents EventsFor<T>() where T: class, ISpawnable
    {
        if(!_pool.ContainsKey(typeof(T))) throw new Exception($"No SpawnPool was found for type: {typeof(T)}");
        return  _pool[typeof(T)].PoolEvents;
    }
}