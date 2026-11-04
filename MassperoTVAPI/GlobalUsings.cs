// Compatibility shim: ApplicationDbContext.cs (intentionally left untouched) still has 'using MassperoTVAPI.Core.Entities;'.
// The global usings below give it the HR entity namespaces it needs; the empty namespace keeps its old using valid.
// Remove together with the old using line if the DbContext is ever edited.
global using MassperoTVAPI.Modules.Hr.Domain.Entities.Candidates;
global using MassperoTVAPI.Modules.Hr.Domain.Entities.Categories;
global using MassperoTVAPI.Modules.Hr.Domain.Entities.Configurations;
global using MassperoTVAPI.Modules.Hr.Domain.Entities.Employees;
global using MassperoTVAPI.Modules.Hr.Domain.Entities.Identity;
global using MassperoTVAPI.Modules.Hr.Domain.Entities.Interviews;
global using MassperoTVAPI.Modules.Hr.Domain.Entities.Issues;
global using MassperoTVAPI.Modules.Hr.Domain.Entities.Jobs;
global using MassperoTVAPI.Modules.Hr.Domain.Entities.Lookups;
global using MassperoTVAPI.Modules.Hr.Domain.Entities.Offers;
global using MassperoTVAPI.Modules.Hr.Domain.Entities.Processes;
global using MassperoTVAPI.Modules.Hr.Domain.Entities.Teams;

namespace MassperoTVAPI.Core.Entities { }
