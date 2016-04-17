using System;
using System.Collections.Generic;
using System.Linq;

namespace PubSub
{
	/// <summary>
	/// <para>Publish subscribe service.</para>
	/// <para></para>
	/// <para>Notes:</para>
	/// <para>1. Publish and Subscribe 'TArgs' must be of the same type for a specific subscription key else the subscriber won't receive the message.</para>
	/// <para></para>
	/// 	  <para>2. Same subscriber can have two or more subscriptions with the same key if 'TArgs' is different for each subscription.
	/// 		       otherwise the last subscription will override the previous.</para>
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
		private readonly Dictionary<string, Action<object>> events;

		private PubSubService ()
		{
			events = new Dictionary<string, Action<object>> ();
		}

		/// <summary>
		/// Subscribe the specified subscriber, key and callback.
		/// </summary>
		/// <param name="subscriber">Subscriber.</param>
		/// <param name="key">Key.</param>
		/// <param name="callback">Callback.</param>
		/// <typeparam name="TArgs">The 1st type parameter.</typeparam>
		public static void Subscribe<TArgs> (object subscriber, string key, Action<TArgs> callback)
		{
			var theKey = $"{typeof(TArgs)}_{key}_{subscriber.GetType().Name}";
			var service = PubSubService.Default;
			lock (service.locker) {
				if (!service.events.ContainsKey (theKey)) {
					service.events.Add (theKey, (args) => callback.Invoke ((TArgs)args));
				} else {
					service.events [theKey] = (args) => callback.Invoke ((TArgs)args);
				}	
			}
		}

		/// <summary>
		/// Publish the specified key and args.
		/// </summary>
		/// <param name="key">Key.</param>
		/// <param name="args">Arguments.</param>
		/// <typeparam name="TArgs">The 1st type parameter.</typeparam>
		public static void Publish<TArgs> (string key, TArgs args)
		{
			var theKey = $"{typeof(TArgs)}_{key}";
			var service = PubSubService.Default;
			lock (service.locker) {
				var actions = service.events.Where (d => {
					var tmp = d.Key.Split('_'); 
					var newKey = $"{tmp[0]}_{tmp[1]}";
					return newKey == theKey;
				}).Select (d => d.Value);	
				foreach (var action in actions) {
					action.Invoke (args);
				}
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
			var service = PubSubService.Default;
			lock (service.locker) {
				var keysToRemove = service.events.Where (d => d.Key.EndsWith (theKey)).Select (d => d.Key).ToList ();
				foreach (var aKey in keysToRemove) {
					service.events.Remove (aKey);
				}	
			}
		}
	}
}

