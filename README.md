# PubSub
My implementation of an Awesome Publish Subscribe service.

## What is this?
The PubSubService enables view models and other components to communicate with without having to know anything about each other besides a simple Subscription contract.

## How it works?
There are two parts to PubSubService:
* Subscribe - Listen for events with a certain signature and perform some action when they are received. Mulitple subscribers can be listening for the same event.
* Publish - Publish an event for listeners to act upon. If no listeners have subscribed then the event is ignored.

The PubSubService is a static class with Subscribe and Publish methods.

Events have a string key parameter that is used as way to address events. The Subscribe and Publish methods use generic parameters to further control how events are delivered - two events with the same key text but different generic type arguments will not be delivered to the same subscriber.

#### The API:
```cshapr
* Subscribe (object subscriber, string key, Action action)
* Subscribe<TArgs> (object subscriber, string key, Action<TArgs> action)
* Subscribe<TSender, TArgs> (object subscriber, string key, Action<TSender, TArgs> action)
* Publish (string key)
* Publish<TArgs> (string key, TArgs args)
* Publish<TSender, TArgs> (TSender sender, string key, TArgs args)
* Unsubscribe (object subscriber, string key)
* Unsubscribe<TArgs> (object subscriber, string key)
* Unsubscribe<TSender, TArgs> (object subscriber, string key)
```
#### Unsubscribe
An object can unsubscribe from a publish signature so that no future events are received. The Unsubscribe method syntax should reflect the signature of the subscription.

# Summary
The PubSubService is a simple way to reduce coupling, especially between view models. It can be used to publish and receive events or pass an argument between classes. Classes should unsubscribe from events they no longer wish to receive.
