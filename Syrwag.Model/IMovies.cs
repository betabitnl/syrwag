using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Syrwag.Model
{
    public interface IMovies
    {
        Task InitialiseAsync(CancellationToken cancellationToken = default);

        Task DropAllAsync(CancellationToken cancellationToken = default);

        Task SaveAsync(Movie movie, CancellationToken cancellationToken = default);

        Task SaveAsync(Person person, CancellationToken cancellationToken = default);

        Task<IEnumerable<Movie>> FindByWriter(Person writer, CancellationToken cancellationToken = default);

    }
}