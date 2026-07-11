using System;  
using System.Collections.Generic;  
using UnityEngine;  
  
namespace MagicTile.ServiceLocator  
{  
    public static class ServiceLocator  
    {  
        private static readonly Dictionary<Type, object> _services = new();  
  
        public static void Register<T>(T service) where T : class  
        {  
            var type = typeof(T);  
            if (_services.ContainsKey(type))  
                Debug.LogWarning($"[ServiceLocator] {type.Name} is already registered. Overwriting.");  
            _services[type] = service;  
        }  
  
        public static T Get<T>() where T : class  
        {  
            if (_services.TryGetValue(typeof(T), out var service))  
                return service as T;  
            Debug.LogWarning($"[ServiceLocator] {typeof(T).Name} is not registered.");  
            return null;  
        }  
  
        public static void Unregister<T>() where T : class => _services.Remove(typeof(T));  
  
        public static bool IsRegistered<T>() where T : class => _services.ContainsKey(typeof(T));  
  
        public static void Clear() => _services.Clear();  
    }  
} 
