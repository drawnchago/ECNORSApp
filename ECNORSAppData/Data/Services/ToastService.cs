namespace ECNORSApp.Services;

public enum ToastLevel { Info, Success, Warning, Error }

public sealed class ToastMessage
{
    public Guid Id { get; } = Guid.NewGuid();
    public ToastLevel Level { get; init; }
    public string Title { get; init; } = "";
    public string Message { get; init; } = "";
    public int TimeoutMs { get; init; } = 4000; // auto-close
}

public interface IToastService
{
    event Action<ToastMessage>? OnShow;
    event Action<Guid>? OnRemove;

    void ShowInfo(string message, string? title = null, int timeoutMs = 4000);
    void ShowSuccess(string message, string? title = null, int timeoutMs = 3500);
    void ShowWarning(string message, string? title = null, int timeoutMs = 5000);
    void ShowError(string message, string? title = null, int timeoutMs = 7000);

    void Remove(Guid id);
}

public sealed class ToastService : IToastService
{
    public event Action<ToastMessage>? OnShow;
    public event Action<Guid>? OnRemove;

    public void ShowInfo(string message, string? title = null, int timeoutMs = 4000)
        => OnShow?.Invoke(new ToastMessage { Level = ToastLevel.Info, Title = title ?? "Info", Message = message, TimeoutMs = timeoutMs });

    public void ShowSuccess(string message, string? title = null, int timeoutMs = 3500)
        => OnShow?.Invoke(new ToastMessage { Level = ToastLevel.Success, Title = title ?? "Éxito", Message = message, TimeoutMs = timeoutMs });

    public void ShowWarning(string message, string? title = null, int timeoutMs = 5000)
        => OnShow?.Invoke(new ToastMessage { Level = ToastLevel.Warning, Title = title ?? "Aviso", Message = message, TimeoutMs = timeoutMs });

    public void ShowError(string message, string? title = null, int timeoutMs = 7000)
        => OnShow?.Invoke(new ToastMessage { Level = ToastLevel.Error, Title = title ?? "Error", Message = message, TimeoutMs = timeoutMs });

    public void Remove(Guid id) => OnRemove?.Invoke(id);
}
