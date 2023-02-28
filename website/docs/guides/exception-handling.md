---
sidebar_position: 4
---

# Exception Handling

In this section, we will learn how to configure the Exception Types to be handled by the Policy.

:::info
The content in this section is **applicable to any of the Retry Policy types**. 
For the sake of simplicity, in the examples Simple Retries will be used.
:::


## How to handle an exception of a given type

You execute the policy for a specific exception type by simply configuring `Handle<TypeOfException>()`.

```csharp
.AddMiddlewares(
    middlewares => middlewares
    .RetrySimple(
        (config) => config
            .Handle<ExternalGatewayException>() // Exceptions to be handled
            ...
    )
    ...
)
```


## How to handle any exception regardless of the type

You execute the policy for any exception thrown by simply configuring with `HandleAnyException()` instead of `Handle<TypeOfException>()`.


```csharp
.AddMiddlewares(
    middlewares => middlewares
    .RetrySimple(
        (config) => config
            .HandleAnyException()
            ...
    )
    ...
)
```


## How to handle any exception regardless of the type

You execute the policy for any exception thrown by simply configuring with `HandleAnyException()` instead of `Handle<TypeOfException>()`.


```csharp
.AddMiddlewares(
    middlewares => middlewares
    .RetrySimple(
        (config) => config
            .HandleAnyException()
            ...
    )
    ...
)
```

## How to handle Exceptions that met given criteria

In case you need to handle multiple types of exceptions or apply any complex condition, you can use the `Handle(Func<RetryContext, bool> func)` method as the following example.

```csharp
.AddMiddlewares(
    middlewares => middlewares
    .RetrySimple(
        (config) => config
            .Handle(context => context.Exception is ArgumentException or ArgumentNullException)
            ...
    )
    ...
)

```
