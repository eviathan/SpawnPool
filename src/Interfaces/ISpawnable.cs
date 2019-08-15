using System.Collections.Specialized;

/// <summary>
/// Interface for Spawnable item types
/// </summary>
public interface ISpawnable 
{
    void OnChange(object sender, NotifyCollectionChangedEventArgs args);
}