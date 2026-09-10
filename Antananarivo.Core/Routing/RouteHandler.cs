using Antananarivo.Core.Http;

namespace Antananarivo.Core.Routing;

public delegate HttpResponse RouteHandler(HttpRequest request);
