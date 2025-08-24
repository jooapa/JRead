namespace JRead;

public class JReadHistory
{
    private readonly List<string> _items = new List<string>();
    private int _maxSize = 100;

    public int MaxSize
    {
        get => _maxSize;
        set => _maxSize = Math.Max(1, value);
    }

    public int Count => _items.Count;

    /// <summary>
    /// Adds an item to the history. 
    /// </summary>
    /// <param name="item">The item to add.</param>
    public void Add(string item)
    {
        if (string.IsNullOrWhiteSpace(item))
            return;

        // Don't add duplicates of the last item
        if (_items.Count > 0 && _items[_items.Count - 1] == item)
            return;

        _items.Add(item);

        // Maintain size limit
        while (_items.Count > _maxSize)
        {
            _items.RemoveAt(0);
        }
    }

    public string? Get(int index)
    {
        if (index < 0 || index >= _items.Count)
            return null;
        return _items[index];
    }

    public List<string> GetAll()
    {
        return new List<string>(_items);
    }

    public void Clear()
    {
        _items.Clear();
    }

    public string? GetLast()
    {
        return _items.Count > 0 ? _items[_items.Count - 1] : null;
    }

    public bool Remove(string item)
    {
        return _items.Remove(item);
    }

    public void RemoveAt(int index)
    {
        if (index >= 0 && index < _items.Count)
            _items.RemoveAt(index);
    }

    public JReadHistory(params string[] items)
    {
        foreach (var item in items)
        {
            Add(item);
        }
    }
}