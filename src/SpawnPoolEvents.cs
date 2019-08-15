
using System;
using System.Collections.ObjectModel;

public class SpawnPoolEvents
{   
    public event Action<ObservableCollection<ISpawnable>> OnEmptyPool;

    public void DidEmptyPool(ObservableCollection<ISpawnable> pool)
    {
        OnEmptyPool?.Invoke(pool);
    }

    public event Action<ObservableCollection<ISpawnable>> OnRunningLow;

    public void DidRunLow(ObservableCollection<ISpawnable> pool)
    {
        OnRunningLow?.Invoke(pool);  
    }

    public event Action<ObservableCollection<ISpawnable>> OnFullPool;

    public void DidFillPool(ObservableCollection<ISpawnable> pool)
    {
        OnFullPool?.Invoke(pool);
    }

    public event Action<ObservableCollection<ISpawnable>> OnOverflow;

    public void DidOverflowPool(ObservableCollection<ISpawnable> pool)
    {
        OnOverflow?.Invoke(pool);    
    }
}