using System;
using System.Collections.Generic;
using System.Linq;

namespace PubSub
{
	/// <summary>
	/// <para>Publish Subscribe service.</para>
	/// <para>Enables view models and other components to communicate with without having to know anything about each other besides a simple Subscription contract.</para>
	/// </summary>
	public class PubSubService
	{
	    public static PubSubService Default { get; } = new PubSubService();

	    private readonly object _locker = new object();
		private readonly Dictionary<string, Handler> _handlers;

		private PubSubService ()
		{
			_handlers = new Dictionary<string, Handler> ();
		}

		private void Subscribe (string key, Handler handler)
		{
			lock (_locker) {
				if (!_handlers.ContainsKey (key)) {
					_handlers.Add (key, handler);
				} else {
					_handlers [key] = handler;
				}	
			}
		}

	    private IEnumerable<Handler> GetHandlers(Func<KeyValuePair<string, Handler>, bool> predicate)
	    {
            lock (_locker)
            {
                var handlersToDelete = _handlers.Where((pair => !pair.Value.Subscriber.IsAlive)).Select(p => p.Key);
                foreach (var key in handlersToDelete)
                {
                    _handlers.Remove(key);
                }
                var hadnlers = _handlers.Where(predicate).Select(d => d.Value);
                return hadnlers;
            }
	    }

        private void Unsubscribe (string key)
		{
			lock (_locker) {
				var keysToRemove = _handlers.Where (d => d.Key == key).Select (d => d.Key).ToList ();
				foreach (var aKey in keysToRemove) {
					_handlers.Remove (aKey);
				}	
			}
		}

		/// <summary>
		/// Subscribe the specified subscriber, key and handler.
		/// </summary>
		/// <param name="subscriber">Subscriber.</param>
		/// <param name="key">Key.</param>
		/// <param name="action">Callback.</param>
		public static void Subscribe (object subscriber, string key, Action action)
		{
            var handler = new Handler()
            {
                Subscriber = new WeakReference(subscriber),
                Action = action
            };
			var theKey = $"{key}_{subscriber.GetType().Name}";
            Default.Subscribe (theKey, handler);
		}

		/// <summary>
		/// Subscribe the specified subscriber, key and handler.
		/// </summary>
		/// <param name="subscriber">Subscriber.</param>
		/// <param name="key">Key.</param>
		/// <param name="action">Callback.</param>
		/// <typeparam name="TArgs">The type of the argument.</typeparam>
		public static void Subscribe<TArgs> (object subscriber, string key, Action<TArgs> action)
		{
            var handler = new Handler()
            {
                Subscriber = new WeakReference(subscriber),
                Action = action
            };
            var theKey = $"{typeof(TArgs)}_{key}_{subscriber.GetType().Name}";
            Default.Subscribe (theKey, handler);
		}

		/// <summary>
		/// Subscribe the specified subscriber, key and handler.
		/// </summary>
		/// <param name="subscriber">Subscriber.</param>
		/// <param name="key">Key.</param>
		/// <param name="action">Callback.</param>
		/// <typeparam name="TSender">The type of the sender.</typeparam>
		/// <typeparam name="TArgs">The type of the argument.</typeparam>
		public static void Subscribe<TSender, TArgs> (object subscriber, string key, Action<TSender, TArgs> action)
		{
            var handler = new Handler()
            {
                Subscriber = new WeakReference(subscriber),
                Action = action
            };
            var theKey = $"{typeof(TSender)}_{typeof(TArgs)}_{key}_{subscriber.GetType().Name}";
            Default.Subscribe (theKey, handler);
		}

		/// <summary>
		/// Publish the specified key.
		/// </summary>
		/// <param name="key">Key.</param>
		public static void Publish (string key)
		{
            var hadnlers = Default.GetHandlers(d => {
                var tmp = d.Key.Split('_');
                return tmp[0] == key;
            });
            foreach (var hadnler in hadnlers)
            {
                ((Action)hadnler.Action)();
            }
		}

		/// <summary>
		/// Publish the specified key and args.
		/// </summary>
		/// <param name="key">Key.</param>
		/// <param name="args">Arguments.</param>
		/// <typeparam name="TArgs">The type of the argument.</typeparam>
		public static void Publish<TArgs> (string key, TArgs args)
		{
			var theKey = $"{typeof(TArgs)}_{key}";
            var hadnlers = Default.GetHandlers(d => {
                var tmp = d.Key.Split('_');
                if (tmp.Count() < 2) return false;
                var newKey = $"{tmp[0]}_{tmp[1]}";
                return newKey == theKey;
            });
            foreach (var hadnler in hadnlers)
            {
                ((Action<TArgs>)hadnler.Action)(args);
            }
		}

		/// <summary>
		/// Publish the specified sender, key and args.
		/// </summary>
		/// <param name="sender">Sender.</param>
		/// <param name="key">Key.</param>
		/// <param name="args">Arguments.</param>
		/// <typeparam name="TSender">The type of the sender.</typeparam>
		/// <typeparam name="TArgs">The type of the argument.</typeparam>
		public static void Publish<TSender, TArgs> (TSender sender, string key, TArgs args)
		{
			var theKey = $"{typeof(TSender)}_{typeof(TArgs)}_{key}";
            var hadnlers = Default.GetHandlers(d => {
                var tmp = d.Key.Split('_');
                if (tmp.Count() < 3) return false;
                var newKey = $"{tmp[0]}_{tmp[1]}_{tmp[2]}";
                return newKey == theKey;
            });
            foreach (var hadnler in hadnlers)
            {
                ((Action<TSender, TArgs>)hadnler.Action)(sender, args);
            }
		}

		/// <summary>
		/// Unsubscribe the specified subscriber and key.
		/// </summary>
		/// <param name="subscriber">Subscriber.</param>
		/// <param name="key">Key.</param>
		public static void Unsubscribe (object subscriber, string key)
		{
			var theKey = $"{key}_{subscriber.GetType().Name}";
            Default.Unsubscribe (theKey);
		}

		/// <summary>
		/// Unsubscribe the specified subscriber and key.
		/// </summary>
		/// <param name="subscriber">Subscriber.</param>
		/// <param name="key">Key.</param>
		/// <typeparam name="TArgs">The type of the argument.</typeparam>
		public static void Unsubscribe<TArgs> (object subscriber, string key)
		{
			var theKey = $"{typeof(TArgs)}_{key}_{subscriber.GetType().Name}";
            Default.Unsubscribe (theKey);
		}

		/// <summary>
		/// Unsubscribe the specified subscriber and key.
		/// </summary>
		/// <param name="subscriber">Subscriber.</param>
		/// <param name="key">Key.</param>
		/// <typeparam name="TSender">The type of the sender.</typeparam>
		/// <typeparam name="TArgs">The type of the argument.</typeparam>
		public static void Unsubscribe<TSender, TArgs> (object subscriber, string key)
		{
			var theKey = $"{typeof(TSender)}_{typeof(TArgs)}_{key}_{subscriber.GetType().Name}";
            Default.Unsubscribe (theKey);
		}
	}
}

