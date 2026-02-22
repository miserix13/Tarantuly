using Microsoft.EntityFrameworkCore;
using Tarantuly.Metadata;

namespace Tarantuly.Discovery;

public interface IEfCrudModelDiscovery
{
    CrudModel Discover(DbContext dbContext);
}
