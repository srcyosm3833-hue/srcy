using System;
using System.Collections.Generic;

namespace Zn.Domain.Entity.Common
{
    /// <summary>
    /// Tüm entity'lerin türediği, birincil anahtar tipi parametrik temel sınıf.
    /// Eşitlik karşılaştırması referans üzerinden değil, kimlik (Id) üzerinden yapılır.
    /// </summary>
    /// <typeparam name="TId">Birincil anahtar tipi (Guid, int, long vb.).</typeparam>
    public abstract class BaseEntity<TId> : IEntity<TId>, IEquatable<BaseEntity<TId>>
        where TId : notnull, IEquatable<TId>
    {
        public TId Id { get; set; } = default!;

        public DateTime CreatedAt { get; set; }

        public DateTime? UpdatedAt { get; set; }

        /// <summary>
        /// Entity henüz veritabanına kaydedilmemişse (Id atanmamışsa) true döner.
        /// Transient entity'ler kimlik karşılaştırmasına girmez.
        /// </summary>
        public bool IsTransient()
        {
            return EqualityComparer<TId>.Default.Equals(Id, default!);
        }

        public bool Equals(BaseEntity<TId>? other)
        {
            if (other is null)
                return false;

            if (ReferenceEquals(this, other))
                return true;

            // Farklı entity tipleri aynı Id'ye sahip olsa bile eşit sayılmaz.
            if (GetType() != other.GetType())
                return false;

            if (IsTransient() || other.IsTransient())
                return false;

            return EqualityComparer<TId>.Default.Equals(Id, other.Id);
        }

        public override bool Equals(object? obj)
        {
            return Equals(obj as BaseEntity<TId>);
        }

        public override int GetHashCode()
        {
            return IsTransient() ? base.GetHashCode() : HashCode.Combine(GetType(), Id);
        }

        public static bool operator ==(BaseEntity<TId>? left, BaseEntity<TId>? right)
        {
            return left?.Equals(right) ?? right is null;
        }

        public static bool operator !=(BaseEntity<TId>? left, BaseEntity<TId>? right)
        {
            return !(left == right);
        }
    }

    /// <summary>
    /// Birincil anahtarı <see cref="Guid"/> olan entity'ler için kısayol temel sınıf.
    /// Projedeki entity'ler doğrudan bundan türeyebilir.
    /// </summary>
    public abstract class BaseEntity : BaseEntity<Guid>
    {
    }
}
