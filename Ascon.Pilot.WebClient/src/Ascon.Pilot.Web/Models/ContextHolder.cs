using System;
using System.Collections.Generic;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;

namespace Ascon.Pilot.Web.Models
{
    public interface IContextHolder : IDisposable
    {
        IContext GetContext(HttpContext httpContext);
        IContext NewContext(Guid contextId);
        void RemoveContext(Guid contextId);
    }

    public class ContextHolder : IContextHolder, IContextLifetimeListener
    {
        private readonly IContextLifetimeService _lifetimeService;
        private readonly Dictionary<Guid, IContext> _contexts = new Dictionary<Guid, IContext>();

        public ContextHolder(IContextLifetimeService lifetimeService)
        {
            _lifetimeService = lifetimeService;
            _lifetimeService.SetContextLifetimeListener(this);
        }

        public IContext NewContext(Guid contextId)
        {
            var context = new Context();
            RegisterClient(context, contextId);
            return context;
        }

        public void RemoveContext(Guid contextId)
        {
            UnregisterClient(contextId);
            _lifetimeService.Unregister(contextId);
        }

        public IContext GetContext(HttpContext httpContext)
        {
            var clientIdString = httpContext.User.FindFirstValue(ClaimTypes.Sid);
            
            if (!string.IsNullOrEmpty(clientIdString))
            {
                var clientId = Guid.Parse(clientIdString);
                if (_contexts.TryGetValue(clientId, out var client))
                {
                    if (client.IsInitialized)
                    {
                        _lifetimeService.Renewal(clientId);
                        return client;
                    }
                }
                
                var context = NewContext(clientId);
                context.Build(httpContext);
                return context;
            }

            return null;
        }

        public void OnTimeIsUp(Guid contextId)
        {
            UnregisterClient(contextId);
        }

        public void Dispose()
        {
            foreach (var item in _contexts)
            {
                item.Value.Dispose();
            }

            _contexts.Clear();
        }

        private void RegisterClient(IContext client, Guid clientId)
        {
            _contexts[clientId] = client;
            _lifetimeService.Register(clientId);
        }

        private void UnregisterClient(Guid contextId)
        {
            if (_contexts.TryGetValue(contextId, out var context))
            {
                context.Dispose();
            }

            _contexts.Remove(contextId);
        }
    }
}
