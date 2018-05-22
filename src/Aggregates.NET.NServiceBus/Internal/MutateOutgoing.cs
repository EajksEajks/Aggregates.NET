﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Aggregates.Contracts;
using Aggregates.Extensions;
using Aggregates.Logging;
using NServiceBus;
using NServiceBus.Pipeline;

namespace Aggregates.Internal
{
    internal class MutateOutgoing : Behavior<IOutgoingLogicalMessageContext>
    {
        private static readonly ILog Logger = LogProvider.GetLogger("MutateOutgoing");

        public override Task Invoke(IOutgoingLogicalMessageContext context, Func<Task> next)
        {
            // Set aggregates.net message and corr id
            if (context.Headers.ContainsKey(Headers.MessageId))
                context.Headers[$"{Defaults.PrefixHeader}.{Defaults.MessageIdHeader}"] = context.Headers[Headers.MessageId];
            if(context.Headers.ContainsKey(Headers.CorrelationId))
                context.Headers[$"{Defaults.PrefixHeader}.{Defaults.CorrelationIdHeader}"] = context.Headers[Headers.CorrelationId];

            if (context.GetMessageIntent() == MessageIntentEnum.Reply)
                return next();

            IMutating mutated = new Mutating(context.Message.Instance, context.Headers ?? new Dictionary<string, string>());

            var mutators = MutationManager.Registered.ToList();
            if (!mutators.Any()) return next();

            IContainer container;

            if (!context.Extensions.TryGet<IContainer>(out container))
                container = Configuration.Settings.Container;

            foreach (var type in mutators)
            {
                var mutator = (IMutate)container.TryResolve(type);
                if (mutator == null)
                {
                    Logger.WarnEvent("MutateFailure", "Failed to construct mutator {Mutator}", type.FullName);
                    continue;
                }
                
                mutated = mutator.MutateOutgoing(mutated);
            }
            
            foreach (var header in mutated.Headers)
                context.Headers[header.Key] = header.Value;
            context.UpdateMessage(mutated.Message);

            return next();
        }
    }
    internal class MutateOutgoingRegistration : RegisterStep
    {
        public MutateOutgoingRegistration() : base(
            stepId: "MutateOutgoing",
            behavior: typeof(MutateOutgoing),
            description: "runs mutators on outgoing messages"
        )
        {
            InsertAfter("MutateOutgoingMessages");
        }
    }

}
