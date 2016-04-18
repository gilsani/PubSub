using System;
using System.Collections.Generic;
using System.Linq;

namespace PubSub
{
	/// <summary>
	/// <para>Publish subscribe service.</para>
	/// <para>Enables view models and other components to communicate with without having to know anything about each other besides a simple Subscription contract.</para>
	/// </summary>
	public class PubSubService
	{
		private static PubSubService instance = null;
		private static PubSubService Default {
			get {
				return instance ?? (instance = new PubSubService ());
			}
		} 

		private object locker = new object();
		private readonly Dictionary<string, Action<object, object>> events;

		private PubSubService ()
		{
			events = new Dictionary<string, Action<object, object>> ();
		}

		private void subscribe (object subscriber, string key, Action<object, object> callback)
		{
			lock (locker) {
				if (!events.ContainsKey (key)) {
					events.Add (key, callback);
				} else {
					events [key] = callback;
				}	
			}
		}

		private void publish (Func<KeyValuePair<string, Action<object, object>> ,bool> predicate, Action<Action<object, object>> actionToInvoke)
		{
			lock (locker) {
				var actions = events.Where (predicate).Select (d => d.Value);	
				foreach (var action in actions) {
					actionToInvoke.Invoke (action);
				}
			}
		}

		private void unsubscribe (string key)
		{
			lock (locker) {
				var keysToRemove = events.Where (d => d.Key == key).Select (d => d.Key).ToList ();
				foreach (var aKey in keysToRemove) {
					events.Remove (aKey);
				}	
			}
		}

		/// <summary>
		/// Subscribe the specified subscriber, key and callback.
		/// </summary>
		/// <param name="subscriber">Subscriber.</param>
		/// <param name="key">Key.</param>
		/// <param name="callback">Callback.</param>
		public static void Subscribe (object subscriber, string key, Action callback)
		{
			var theKey = $"{key}_{subscriber.GetType().Name}";
			PubSubService.Default.subscribe (subscriber, theKey, (sender, args) => callback.Invoke ());
		}

		/// <summary>
		/// Subscribe the specified subscriber, key and callback.
		/// </summary>
		/// <param name="subscriber">Subscriber.</param>
		/// <param name="key">Key.</param>
		/// <param name="callback">Callback.</param>
		/// <typeparam name="TSender">The type of the sender.</typeparam>
		public static void Subscribe<TSender> (object subscriber, string key, Action<TSender> callback)
		{
			var theKey = $"{typeof(TSender)}_{key}_{subscriber.GetType().Name}";
			PubSubService.Default.subscribe (subscriber, theKey, (sender, args) => callback.Invoke ((TSender)sender));
		}

		/// <summary>
		/// Subscribe the specified subscriber, key and callback.
		/// </summary>
		/// <param name="subscriber">Subscriber.</param>
		/// <param name="key">Key.</param>
		/// <param name="callback">Callback.</param>
		/// <typeparam name="TArgs">The type of the argument.</typeparam>
		public static void Subscribe<TArgs> (object subscriber, string key, Action<TArgs> callback)
		{
			var theKey = $"{typeof(TArgs)}_{key}_{subscriber.GetType().Name}";
			PubSubService.Default.subscribe (subscriber, theKey, (sender, args) => callback.Invoke ((TArgs)args));
		}

		/// <summary>
		/// Subscribe the specified subscriber, key and callback.
		/// </summary>
		/// <param name="subscriber">Subscriber.</param>
		/// <param name="key">Key.</param>
		/// <param name="callback">Callback.</param>
		/// <typeparam name="TSender">The type of the sender.</typeparam>
		/// <typeparam name="TArgs">The type of the argument.</typeparam>
		public static void Subscribe<TSender, TArgs> (object subscriber, string key, Action<TSender, TArgs> callback)
		{
			var theKey = $"{typeof(TSender)}_{typeof(TArgs)}_{key}_{subscriber.GetType().Name}";
			PubSubService.Default.subscribe (subscriber, theKey, (sender, args) => callback.Invoke ((TSender)sender, (TArgs)args));
		}

		/// <summary>
		/// Publish the specified key.
		/// </summary>
		/// <param name="key">Key.</param>
		public static void Publish (string key)
		{
			PubSubService.Default.publish (d => {
				var tmp = d.Key.Split('_'); 
				return tmp[0] == key;
			}, (action) => action.Invoke (null, null));
		}

		/// <summary>
		/// Publish the specified sender and key.
		/// </summary>
		/// <param name="sender">Sender.</param>
		/// <param name="key">Key.</param>
		/// <typeparam name="TSender">The type of the sender.</typeparam>
		public static void Publish<TSender> (TSender sender, string key)
		{
			var theKey = $"{typeof(TSender)}_{key}";
			PubSubService.Default.publish (d => {
				var tmp = d.Key.Split('_'); 
				if (tmp.Count () >= 2) {
					var newKey = $"{tmp[0]}_{tmp[1]}";
					return newKey == theKey;
				}
				return false;
			}, (action) => action.Invoke (sender, null));
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
			PubSubService.Default.publish (d => {
				var tmp = d.Key.Split('_'); 
				if (tmp.Count () >= 2) {
					var newKey = $"{tmp[0]}_{tmp[1]}";
					return newKey == theKey;
				}
				return false;
			}, (action) => action.Invoke (null, args));
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
			PubSubService.Default.publish (d => {
				var tmp = d.Key.Split('_'); 
				if (tmp.Count () >= 3) {
					var newKey = $"{tmp[0]}_{tmp[1]}_{tmp[2]}";
					return newKey == theKey;
				}
				return false;
			}, (action) => action.Invoke (sender, args));
		}

		/// <summary>
		/// Unsubscribe the specified subscriber and key.
		/// </summary>
		/// <param name="subscriber">Subscriber.</param>
		/// <param name="key">Key.</param>
		public static void Unsubscribe (object subscriber, string key)
		{
			var theKey = $"{key}_{subscriber.GetType().Name}";
			PubSubService.Default.unsubscribe (theKey);
		}

		/// <summary>
		/// Unsubscribe the specified subscriber and key.
		/// </summary>
		/// <param name="subscriber">Subscriber.</param>
		/// <param name="key">Key.</param>
		/// <typeparam name="TSender">The type of the sender.</typeparam>
		public static void Unsubscribe<TSender> (object subscriber, string key)
		{
			var theKey = $"{typeof(TSender)}_{key}_{subscriber.GetType().Name}";
			PubSubService.Default.unsubscribe (theKey);
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
			PubSubService.Default.unsubscribe (theKey);
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
			PubSubService.Default.unsubscribe (theKey);
		}
	}
}

