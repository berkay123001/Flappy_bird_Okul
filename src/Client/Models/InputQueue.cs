namespace Client.Models;

public sealed class InputQueue
{
    private readonly Queue<InputEvent> _queue = new();

    public int Count => _queue.Count;

    public void Enqueue(InputType type, double timestamp) =>
        _queue.Enqueue(new InputEvent(type, timestamp));

    public bool TryDequeue(out InputEvent input)
    {
        if (_queue.Count > 0)
        {
            input = _queue.Dequeue();
            return true;
        }

        input = default;
        return false;
    }

    public bool TryPeek(out InputEvent input)
    {
        if (_queue.Count > 0)
        {
            input = _queue.Peek();
            return true;
        }

        input = default;
        return false;
    }

    public void Clear() => _queue.Clear();

    public readonly record struct InputEvent(InputType Type, double Timestamp);
}
