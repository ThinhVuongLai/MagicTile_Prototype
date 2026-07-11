using System;

public abstract class Singleton<T> where T : Singleton<T>
{
    private static T _instance;
    private static readonly object _lock = new object();

    public static T Instance
    {
        get
        {
            if (_instance == null)
            {
                lock (_lock)
                {
                    if (_instance == null)
                    {
                        _instance = Activator.CreateInstance(typeof(T), true) as T;
                    }
                }
            }
            return _instance;
        }
    }

    public static bool HasInstance => _instance != null;

    protected Singleton() { }

    protected virtual void OnDestroy() { }

    public static void DestroyInstance()
    {
        if (_instance != null)
        {
            _instance.OnDestroy();
            _instance = null;
        }
    }
}
