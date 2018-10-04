﻿using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Ascon.Pilot.WebClient.Models
{
    public interface IContextHolder : IDisposable
    {
        IContext GetContext(HttpContext httpContext);
        IContext NewContext(Guid id);
    }

    public class ContextHolder : IContextHolder
    {
        private readonly Dictionary<Guid, IContext> _contexts = new Dictionary<Guid, IContext>();

        public IContext NewContext(Guid id)
        {
            var context = new Context();
            RegisterClient(context, id);
            return context;
        }

        public IContext GetContext(HttpContext httpContext)
        {
            var clientIdString = httpContext.User.FindFirstValue(ClaimTypes.Sid);
            
            if (!string.IsNullOrEmpty(clientIdString))
            {
                var clientId = Guid.Parse(clientIdString);
                if (_contexts.ContainsKey(clientId))
                {
                    var client = _contexts[clientId];
                    if (client.IsInitialized)
                        return client;
                }

                var context = NewContext(clientId);
                context.Build(httpContext);
                return context;
            }

            return null;
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
        }

    }
}
