using System;

namespace RabbitMQ.Next;

internal class Disposer: IDisposable
{
    private Action disposeCallback;

    public Disposer(Action disposeCallback)
    {
        this.disposeCallback = disposeCallback;
    }


    public void Dispose()
    {
        this.disposeCallback?.Invoke();
        this.disposeCallback = null;
    }
}
