using System;

namespace REstate.Platform
{
    public static class FluentIfExtension
    {
        public static T If<T>(this T fluentObject, Func<T, bool> predicate, Action<T> fluentContinuation)
        {
            if (predicate(fluentObject))
            {
                fluentContinuation(fluentObject);
            }

            return fluentObject;
        }
    }
}