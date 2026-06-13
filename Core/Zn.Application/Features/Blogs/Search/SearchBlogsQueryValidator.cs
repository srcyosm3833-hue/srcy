using FluentValidation;

namespace Zn.Application.Features.Blogs.Search
{
    /// <summary>
    /// <see cref="SearchBlogsQuery"/> için FluentValidation kuralları. Arama terimi (Q)
    /// zorunludur (boş/whitespace olamaz) ve azami uzunluğu aşamaz; sayfa boyutu
    /// güvenli aralıkta (1–<see cref="SearchBlogsQuery.MaxPageSize"/>) olmalıdır.
    /// Validator yalnızca query'ye uygulanır; veritabanı/iş kuralları handler ve
    /// repository tarafındadır.
    /// </summary>
    public sealed class SearchBlogsQueryValidator : AbstractValidator<SearchBlogsQuery>
    {
        public SearchBlogsQueryValidator()
        {
            RuleFor(x => x.Q)
                .NotEmpty().WithMessage("Arama terimi boş olamaz")
                .MaximumLength(SearchBlogsQuery.QueryMaxLength)
                .WithMessage($"Arama terimi en fazla {SearchBlogsQuery.QueryMaxLength} karakter olabilir.");

            RuleFor(x => x.PageSize)
                .InclusiveBetween(1, SearchBlogsQuery.MaxPageSize)
                .WithMessage($"Sayfa boyutu 1 ile {SearchBlogsQuery.MaxPageSize} arasında olmalıdır.");
        }
    }
}
