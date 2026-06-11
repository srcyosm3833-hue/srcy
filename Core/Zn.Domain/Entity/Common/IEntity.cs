using System;

namespace Zn.Domain.Entity.Common
{
    /// <summary>
    /// Tüm entity'ler için tip parametresinden bağımsız işaretleyici (marker) arayüz.
    /// Generic kısıtlamalarda ve reflection ile entity taramada ortak nokta sağlar.
    /// </summary>
    public interface IEntity
    {
    }

    /// <summary>
    /// Birincil anahtarı <typeparamref name="TId"/> tipinde olan entity sözleşmesi.
    /// </summary>
    /// <typeparam name="TId">Birincil anahtar tipi (Guid, int, long vb.).</typeparam>
    public interface IEntity<out TId> : IEntity
        where TId : notnull
    {
        TId Id { get; }
    }
}
