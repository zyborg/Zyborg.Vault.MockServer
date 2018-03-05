using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Routing.Constraints;
using Newtonsoft.Json;

namespace Zyborg.Vault.MockServer.WebHandler
{
    public abstract class AttributedRequestHandler : IRouteResolver, IRequestHandler
    {
        private IEnumerable<QualifiedRoute> _handlerRoutes = null;
        private List<HandlerMethod> _handlerMethods = new List<HandlerMethod>();
        private Dictionary<Guid, HandlerMethod> _handlerMethodMap = new Dictionary<Guid, HandlerMethod>();

        public AttributedRequestHandler()
        {
            var tas = GetType().GetCustomAttributes().OfType<IRouteAttribute>().ToArray();
            if (tas.Length > 0)
            {
                foreach (var a in tas.OfType<ITargetedAttribute>())
                {
                    a.InitForTarget(this);
                }
                _handlerRoutes = tas.SelectMany(a => a.GetRoutes()
                    .Select(r => new QualifiedRoute(r.route, r.constraint)));
            }
            CompileHandlerMethods();
        }

        public IEnumerable<QualifiedRoute> ResolveRoutes()
        {
            return _handlerMethods.SelectMany(hm => hm._routes);
        }

        public Task<IHandlerResult> HandleAsync(HttpContext http)
        {
            var id = (Guid)http.GetRouteData().DataTokens["handlerMethodId"];
            var hm = _handlerMethodMap[id];

            return hm._invoker(http);
        }

        private void CompileHandlerMethods()
        {
            var type = GetType();
            var methods = type.GetMethods(BindingFlags.Public | BindingFlags.Instance);

            foreach (var m in methods)
            {
                var mas = m.GetCustomAttributes().OfType<IRouteAttribute>().ToArray();
                if (mas.Length == 0)
                    continue;

                foreach (var a in mas.OfType<ITargetedAttribute>())
                {
                    a.InitForTarget(m);
                }

                var hm = new HandlerMethod {
                    _method = m,
                    _invoker = ResolveHandlerInvoker(m),
                };
                hm._routes = mas.SelectMany(ma => ma.GetRoutes()).SelectMany(mar => {
                    var id = Guid.NewGuid();
                    _handlerMethodMap[id] = hm;

                    return ResolveFullMethodRoutes(mar.route, mar.constraint, new { handlerMethodId = id });
                });

                _handlerMethods.Add(hm);
            }
        }

        private IEnumerable<QualifiedRoute> ResolveFullMethodRoutes(string route,
                IRouteConstraint constraint, object dataTokens)
        {
            if (_handlerRoutes != null)
            {
                foreach (var hr in _handlerRoutes)
                {
                    var t = route;
                    if (!string.IsNullOrEmpty(hr.Template))
                    {
                        t = $"{hr.Template.TrimEnd('/')}/{t.TrimStart('/')}";
                    }
                    var c = constraint;
                    if (c == null)
                        c = hr.Constraint;
                    else if (hr.Constraint != null)
                        c = new CompositeRouteConstraint(new[] { c, constraint });
                    
                    yield return new QualifiedRoute(t, c, dataTokens);
                }
            }
            else
            {
                yield return new QualifiedRoute(route, constraint, dataTokens);
            }
        }

        private Func<HttpContext, Task<IHandlerResult>> ResolveHandlerInvoker(MethodInfo m)
        {
            Func<HttpContext, Task<IHandlerResult>> invoker = null;
            var hrReturn = typeof(IHandlerResult).IsAssignableFrom(m.ReturnType);
            var hrTaskReturn = m.ReturnType.GetGenericTypeDefinition() == typeof(Task<>)
                    && m.ReturnType.IsGenericType
                    && typeof(IHandlerResult).IsAssignableFrom(m.ReturnType.GenericTypeArguments[0]);
            
            if (hrReturn || hrTaskReturn)
            {
                var pis = m.GetParameters();
                if (pis.Length == 1 && pis[0].ParameterType == typeof(HttpContext))
                {
                    invoker = http => InvokeHandlerMethod(hrTaskReturn, m, this, new[] { http });
                }
                else
                {
                    var binders = pis.Select((pi, index) => {
                        var prs = pi.GetCustomAttributes().Where(a => a is IRequestParameterBinder)
                            .Select(a => a as IRequestParameterBinder).ToArray();
                        if (prs.Length == 0)
                            prs = new[] { new BindQueryAttribute() };
                        
                        foreach (var pr in prs)
                        {
                            if (pr is ITargetedAttribute ta)
                                ta.InitForTarget(pi);
                        }

                        Action<HttpContext, object[]> binder = (http, paramValues) => {
                            foreach (var pr in prs)
                            {
                                pr.Bind(http, m, index, paramValues);
                            }
                        };
                        
                        return binder;
                    });

                    invoker = http => {
                        var paramValues = new object[pis.Length];
                        foreach (var b in binders)
                            b(http, paramValues);
                        return InvokeHandlerMethod(hrTaskReturn, m, this, paramValues);
                    };
                }
            }
            return invoker;
        }

        private static Task<IHandlerResult> InvokeHandlerMethod(bool asyncReturn, MethodInfo m, object target, object[] paramValues)
        {
            IHandlerResult result;
            if (asyncReturn)
            {
                var t = (Task)m.Invoke(target, paramValues);
                t.GetAwaiter().GetResult();
                result = (IHandlerResult)t.GetType().GetProperty("Result").GetValue(t);
            }
            else
            {
                result = (IHandlerResult)m.Invoke(target, paramValues);
            }

            return Task.FromResult(result);
        }

        /// Attributes that implement this interface will be initialized with the target element
        /// (method, parameter, etc.) that they are decorated upon before any other action is
        /// invoked on them.
        public interface ITargetedAttribute
        {
            void InitForTarget(object attributeTarget);
        }

        public interface IRouteAttribute
        {
            IEnumerable<(string route, IRouteConstraint constraint)> GetRoutes();
        }

        public abstract class BaseHandleVerbAttribute : Attribute, ITargetedAttribute, IRouteAttribute,
                IRouteConstraint
        {
            private MethodInfo _attributeTarget;

            protected BaseHandleVerbAttribute(string httpMethod, string template = null)
            {
                HttpMethod = httpMethod;
                HttpMethodChecker = m => string.Equals(HttpMethod, m, StringComparison.OrdinalIgnoreCase);
                Template = template;
            }

            protected BaseHandleVerbAttribute(Func<string, bool> httpMethodChecker, string template = null)
            {
                HttpMethodChecker = httpMethodChecker;
                Template = template;
            }

            public string HttpMethod { get; }

            public Func<string, bool> HttpMethodChecker { get; }

            public string Template { get; }

            void ITargetedAttribute.InitForTarget(object attributeTarget)
            {
                _attributeTarget = attributeTarget as MethodInfo;
            }

            public IEnumerable<(string, IRouteConstraint)> GetRoutes()
            {
                if (_attributeTarget == null)
                    throw new InvalidOperationException($@"uninitialized targetted attribute");

                var t = Template ?? _attributeTarget?.Name;
                if (t != null)
                    yield return (t, this);
            }

            [Microsoft.AspNetCore.Mvc.HttpGet]
            public virtual bool Match(HttpContext http, IRouter route, string routeKey,
                RouteValueDictionary values, RouteDirection routeDirection)
            {
                return HttpMethodChecker(http.Request.Method);
            }
        }

        [AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
        public class HandleGetAttribute : BaseHandleVerbAttribute
        {
            public HandleGetAttribute(string template = null) : base(HttpMethods.IsGet, template)
            { }
        }

        [AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
        public class HandleHeadAttribute : BaseHandleVerbAttribute
        {
            public HandleHeadAttribute(string template = null) : base(HttpMethods.IsHead, template)
            { }
        }

        [AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
        public class HandlePostAttribute : BaseHandleVerbAttribute
        {
            public HandlePostAttribute(string template = null) : base(HttpMethods.IsPost, template)
            { }
        }

        [AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
        public class HandlePutAttribute : BaseHandleVerbAttribute
        {
            public HandlePutAttribute(string template = null) : base(HttpMethods.IsPut, template)
            { }
        }

        [AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
        public class HandleDeleteAttribute : BaseHandleVerbAttribute
        {
            public HandleDeleteAttribute(string template = null) : base(HttpMethods.IsDelete, template)
            { }
        }

        [AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
        public class HandleListAttribute : BaseHandleVerbAttribute
        {
            public HandleListAttribute(string template = null) : base("LIST", template)
            { }

            public override bool Match(HttpContext http, IRouter route, string routeKey,
                RouteValueDictionary values, RouteDirection routeDirection)
            {
                // Canonical LIST method
                if (base.Match(http, route, routeKey, values, routeDirection))
                    return true;

                // Alternative using GET method
                var requ = http.Request;
                return HttpMethods.IsGet(requ.Method)
                    && requ.Query.TryGetValue("list", out var list)
                    && list.Equals("1");
            }
        }

        public interface IRequestParameterBinder : ITargetedAttribute
        {
            void Bind(HttpContext http, MethodInfo method, int paramIndex, object[] paramValues);
        }

        [AttributeUsage(AttributeTargets.Parameter)]
        public abstract class RequestParameterBinder : Attribute, IRequestParameterBinder
        {
            private ParameterInfo _attributeTarget;

            public RequestParameterBinder(string name = null, object defaultValue = null)
            {
                Name = name;
                DefaultValue = defaultValue;
            }

            public string Name { get; }

            public object DefaultValue { get; }


            public virtual void InitForTarget(object attributeTarget)
            {
                _attributeTarget = attributeTarget as ParameterInfo;
            }
            public virtual void Bind(HttpContext http, MethodInfo method, int paramIndex, object[] paramValues)
            {
                var param = method.GetParameters()[paramIndex];
                paramValues[paramIndex] = Resolve(http, _attributeTarget);
            }

            protected abstract object Resolve(HttpContext http, ParameterInfo param);
        }

        [AttributeUsage(AttributeTargets.Parameter)]
        public class BindQueryAttribute : RequestParameterBinder
        {
            public BindQueryAttribute(string name = null, object defaultValue = null)
                : base(name, defaultValue)
            { }

            protected override object Resolve(HttpContext http, ParameterInfo param)
            {
                var t = param.ParameterType;
                var n = Name ?? param.Name;
                if (http.Request.Query.TryGetValue(n, out var values))
                {
                    var et = t.IsArray ? t.GetElementType() : null;
                    if (t.IsGenericType && t.GetGenericTypeDefinition() == typeof(IEnumerable<>))
                        et = t.GetGenericArguments()[0];

                    if (et != null)
                    {
                        if (et == typeof(string))
                            return values.ToArray();
                        
                        var etConverter = TypeDescriptor.GetConverter(et);
                        return values.Select(v => etConverter.ConvertFromString(v)).ToArray();
                    }

                    var tConverter = TypeDescriptor.GetConverter(t);
                    return tConverter.ConvertFromString(values[0]);
                }

                return DefaultValue ?? param.DefaultValue;
            }
        }

        [AttributeUsage(AttributeTargets.Parameter)]
        public class BindHeaderAttribute : RequestParameterBinder
        {
            public BindHeaderAttribute(string name = null, object defaultValue = null)
                : base(name, defaultValue)
            { }

            protected override object Resolve(HttpContext http, ParameterInfo param)
            {
                var t = param.ParameterType;
                var n = Name ?? param.Name;

                if (http.Request.Headers.TryGetValue(n, out var values))
                {
                    var et = t.IsArray ? t.GetElementType() : null;
                    if (t.IsGenericType && t.GetGenericTypeDefinition() == typeof(IEnumerable<>))
                        et = t.GetGenericArguments()[0];

                    if (et != null)
                    {
                        if (et == typeof(string))
                            return values.ToArray();
                        
                        var etConverter = TypeDescriptor.GetConverter(et);
                        return values.Select(v => etConverter.ConvertFromString(v)).ToArray();
                    }

                    var tConverter = TypeDescriptor.GetConverter(t);
                    return tConverter.ConvertFromString(values[0]);
                }

                return DefaultValue ?? param.DefaultValue;
            }
        }


        [AttributeUsage(AttributeTargets.Parameter)]
        public class BindBodyAttribute : RequestParameterBinder
        {
            public BindBodyAttribute(object defaultValue = null)
                : base(null, defaultValue)
            { }

            protected override object Resolve(HttpContext http, ParameterInfo param)
            {
                var t = param.ParameterType;
                if (http.Request.ContentLength.GetValueOrDefault() > 0)
                {
                    using (var sr = new StreamReader(http.Request.Body))
                    {
                        var body = sr.ReadToEnd();
                        return JsonConvert.DeserializeObject(body, t);
                    }
                }

                return DefaultValue ?? param.DefaultValue;
            }
        }

        private class HandlerMethod
        {
            public MethodInfo _method;
            public IEnumerable<QualifiedRoute> _routes;
            public Func<HttpContext, Task<IHandlerResult>> _invoker;
        }
    }
}