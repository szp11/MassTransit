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
namespace MassTransit
{
    using System;
    using System.Threading.Tasks;
    using Pipeline;


    public class FilterPipe<T> :
        IPipe<T>
        where T : class, PipeContext
    {
        readonly IFilter<T> _filter;
        readonly IPipe<T> _next;

        public FilterPipe(IFilter<T> filter, IPipe<T> next)
        {
            _filter = filter;
            _next = next;
        }

        public Task Send(T context)
        {
            return _filter.Send(context, _next);
        }

        public bool Inspect(IPipeInspector inspector)
        {
            throw new NotImplementedException();
        }
    }
}