# Features

Bu klasör CQRS dikey dilimlerini barındırır. Her özellik kendi alt klasöründe,
ait olduğu bileşenlerle birlikte teslim edilir:

```
Features/
  Auth/
    Register/
      RegisterCommand.cs        # immutable record (command/query)
      RegisterHandler.cs        # Wolverine handler (plain metot, IRequest yok)
      RegisterValidator.cs      # FluentValidation AbstractValidator<RegisterCommand>
      RegisterMapper.cs         # Mapperly [Mapper] partial class (entity <-> DTO)
      RegisterResponse.cs       # response/DTO record
```

Konvansiyonlar:
- Mediator: **Wolverine** — handler'lar `IRequest`/`IRequestHandler` kullanmaz; plain
  `Handle`/`HandleAsync` metotlarıdır, `IMessageBus.InvokeAsync` ile çağrılır.
- Mapping: **Mapperly** — `[Mapper]` attribute'lu `partial class`, source-generated.
- Validation: **FluentValidation** — validator'lar yalnızca command/query'ye uygulanır,
  asla entity'lere değil. Entity invariant'ları Domain'de korunur.
- Sonuç tipi: `Zn.Application.Common.Results.Result` / `Result<T>`.

İlk dikey dilim ROADMAP Faz 1'de (Auth) eklenecektir.
