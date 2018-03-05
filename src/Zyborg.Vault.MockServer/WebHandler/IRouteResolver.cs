using System.Collections.Generic;
using System.Reflection;

namespace Zyborg.Vault.MockServer.WebHandler
{
    public interface IRouteResolver
    {
         IEnumerable<QualifiedRoute> ResolveRoutes();
    }
}