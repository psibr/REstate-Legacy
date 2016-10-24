using System;
using System.Collections.Generic;

namespace REstate
{
    public class Trigger
        : IEquatable<Trigger>, IEquatable<KeyValuePair<string, string>>
    {
        public Trigger(string machineDefinitionId, string triggerName)
        {
            if (string.IsNullOrWhiteSpace(triggerName))
                throw new ArgumentException("Argument is null or whitespace", nameof(triggerName));
            if (String.IsNullOrWhiteSpace(machineDefinitionId))
                throw new ArgumentException("Argument is null or whitespace", nameof(machineDefinitionId));

            MachineDefinitionId = machineDefinitionId;
            TriggerName = triggerName;
        }

        public Trigger(KeyValuePair<string, string> statePair)
        {
            if (string.IsNullOrWhiteSpace(statePair.Key))
                throw new ArgumentException("Argument is null or whitespace", nameof(statePair));
            if (string.IsNullOrWhiteSpace(statePair.Value))
                throw new ArgumentException("Argument is null or whitespace", nameof(statePair));

            MachineDefinitionId = statePair.Key;
            TriggerName = statePair.Value;
        }

        public virtual string MachineDefinitionId { get; }

        public virtual string TriggerName { get; }

        public override string ToString()
        {
            return TriggerName;
        }

        /// <summary>
        /// Indicates whether the current object is equal to another object of the same type.
        /// </summary>
        /// <returns>
        /// true if the current object is equal to the <paramref name="other"/> parameter; otherwise, false.
        /// </returns>
        /// <param name="other">An object to compare with this object.</param>
        public bool Equals(Trigger other)
        {
            return other != null
                   && MachineDefinitionId == other.MachineDefinitionId
                   && TriggerName == other.TriggerName;
        }

        /// <summary>
        /// Indicates whether the current object is equal to another object of the same type.
        /// </summary>
        /// <returns>
        /// true if the current object is equal to the <paramref name="other"/> parameter; otherwise, false.
        /// </returns>
        /// <param name="other">An object to compare with this object.</param>
        public bool Equals(KeyValuePair<string, string> other)
        {
            return !string.IsNullOrWhiteSpace(other.Key)
                   && !string.IsNullOrWhiteSpace(other.Value)
                   && MachineDefinitionId == other.Key
                   && TriggerName == other.Value;
        }

        /// <summary>
        /// Determines whether the specified object is equal to the current object.
        /// </summary>
        /// <returns>
        /// true if the specified object  is equal to the current object; otherwise, false.
        /// </returns>
        /// <param name="obj">The object to compare with the current object. </param><filterpriority>2</filterpriority>
        public override bool Equals(object obj)
        {
            if (obj == null)
                return false;

            var trigger = obj as Trigger;
            if (trigger != null)
                return Equals(trigger);

            if (obj is KeyValuePair<string, string>)
                return Equals((KeyValuePair<string, string>)obj);

            return false;
        }

        /// <summary>
        /// Serves as the default hash function.
        /// </summary>
        /// <returns>
        /// A hash code for the current object.
        /// </returns>
        /// <filterpriority>2</filterpriority>
        public override int GetHashCode()
        {
            unchecked
            {
                return (MachineDefinitionId.GetHashCode() * 397) ^ (TriggerName?.GetHashCode() ?? 0);
            }
        }

        public static bool operator ==(Trigger a, Trigger b)
        {
            return ReferenceEquals(a, b) || (object)a != null && a.Equals(b);
        }

        public static bool operator !=(Trigger a, Trigger b)
        {
            return !(a == b);
        }
    }
}
