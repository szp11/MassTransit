﻿// Copyright 2007-2014 Chris Patterson, Dru Sellers, Travis Smith, et. al.
//  
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use
// this file except in compliance with the License. You may obtain a copy of the 
// License at 
// 
//     http://www.apache.org/licenses/LICENSE-2.0 
// 
// Unless required by applicable law or agreed to in writing, software distributed
// under the License is distributed on an "AS IS" BASIS, WITHOUT WARRANTIES OR 
// CONDITIONS OF ANY KIND, either express or implied. See the License for the 
// specific language governing permissions and limitations under the License.
namespace MassTransit.SubscriptionConnectors
{
    using System.Collections.Generic;
    using System.Linq;
    using Exceptions;
    using Pipeline;
    using Pipeline.Sinks;
    using Policies;
    using Util;


    /// <summary>
    ///     Interface implemented by objects that tie an inbound pipeline together with
    ///     consumers (by means of calling a consumer factory).
    /// </summary>
    public interface ConsumerConnector
    {
        ConnectHandle Connect<TConsumer>(IInboundPipe inboundPipe, IConsumerFactory<TConsumer> consumerFactory, IRetryPolicy retryPolicy)
            where TConsumer : class;
    }


    public class ConsumerConnector<T> :
        ConsumerConnector
        where T : class
    {
        readonly IEnumerable<ConsumerMessageConnector> _connectors;

        public ConsumerConnector()
        {
            if (TypeMetadataCache<T>.HasSagaInterfaces)
                throw new ConfigurationException("A saga cannot be registered as a consumer");

            _connectors = Consumes()
                .Concat(ConsumesContext())
                .Concat(ConsumesAll())
                .Distinct((x, y) => x.MessageType == y.MessageType)
                .ToList();
        }

        public IEnumerable<ConsumerMessageConnector> Connectors
        {
            get { return _connectors; }
        }

        public ConnectHandle Connect<TConsumer>(IInboundPipe inboundPipe, IConsumerFactory<TConsumer> consumerFactory, IRetryPolicy retryPolicy)
            where TConsumer : class
        {
            return new MultipleConnectHandle(_connectors.Select(x => x.Connect(inboundPipe, consumerFactory, retryPolicy)));
        }

        static IEnumerable<ConsumerMessageConnector> ConsumesContext()
        {
            return ConsumerMetadataCache<T>.ContextConsumerTypes.Select(x => x.GetConsumerContextConnector());
        }

        static IEnumerable<ConsumerMessageConnector> Consumes()
        {
            return ConsumerMetadataCache<T>.ConsumerTypes.Select(x => x.GetConsumerConnector());
        }

        static IEnumerable<ConsumerMessageConnector> ConsumesAll()
        {
            return ConsumerMetadataCache<T>.MessageConsumerTypes.Select(x => x.GetConsumerMessageConnector());
        }
    }
}