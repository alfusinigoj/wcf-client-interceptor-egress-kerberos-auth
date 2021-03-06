﻿using System;
using System.Configuration;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using System.ServiceModel.Dispatcher;

namespace Pivotal.RouteServiceIwaWcfInterceptor
{
    public class IwaInterceptorEndpointBehaviour : IEndpointBehavior
    {
        public void AddBindingParameters(ServiceEndpoint endpoint, BindingParameterCollection bindingParameters)
        {
        }

        public void ApplyClientBehavior(ServiceEndpoint endpoint, ClientRuntime clientRuntime)
        {
            clientRuntime.MessageInspectors.Add(new SvcRequestIwaInterceptor());

            if(Convert.ToBoolean(ConfigurationManager.AppSettings["ImpersonateClientUser"]))
                clientRuntime.MessageInspectors.Add(new ImpersonateInterceptor());
        }

        public void ApplyDispatchBehavior(ServiceEndpoint endpoint, EndpointDispatcher endpointDispatcher)
        {
        }

        public void Validate(ServiceEndpoint endpoint)
        {
        }
    }
}