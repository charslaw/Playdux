using System;

namespace Shared;

public class BaseObserver<T>(
    Action<T>? onNext = null,
    Action<Exception>? onError = null,
    Action? onCompleted = null
) : IObserver<T>
{
    public void OnCompleted() => onCompleted?.Invoke();

    public void OnError(Exception error) => onError?.Invoke(error);

    public void OnNext(T value) => onNext?.Invoke(value);
}

public static class IObservableExtensions
{
    public static IDisposable Subscribe<T>(
        this IObservable<T> observable,
        Action<T>? onNext = null,
        Action<Exception>? onError = null,
        Action? onCompleted = null
    ) => observable.Subscribe(new BaseObserver<T>(onNext, onError, onCompleted));
}