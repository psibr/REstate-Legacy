using System;
using System.Collections.Generic;

namespace REstate
{
    public class State
        : IEquatable<State>, IEquatable<KeyValuePair<string, string>>
    {
        public State(string machineDefinitionId, string stateName, string commitTag = null)
        {
            if (string.IsNullOrWhiteSpace(machineDefinitionId))
                throw new ArgumentException("Argument is null or whitespace", nameof(stateName));
            if (string.IsNullOrWhiteSpace(stateName))
                throw new ArgumentException("Argument is null or whitespace", nameof(stateName));

            MachineDefinitionId = machineDefinitionId;
            StateName = stateName;
            CommitTag = commitTag;
        }

        public string MachineDefinitionId { get; }

        public string StateName { get; }

        public string CommitTag { get; }

        public override string ToString()
        {
            return StateName;
        }

        /// <summary>
        /// Indicates whether the current object is equal to another object of the same type.
        /// </summary>
        /// <returns>
        /// true if the current object is equal to the <paramref name="other"/> parameter; otherwise, false.
        /// </returns>
        /// <param name="other">An object to compare with this object.</param>
        public bool Equals(State other)
        {
            return other != null
                   && MachineDefinitionId == other.MachineDefinitionId
                   && StateName == other.StateName;
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
                && StateName == other.Value;
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

            var state = obj as State;
            if (state != null)
                return Equals(state);

            if (obj is KeyValuePair<int, string>)
                return Equals((KeyValuePair<int, string>)obj);

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
                return (MachineDefinitionId.GetHashCode() * 397) ^ (StateName?.GetHashCode() ?? 0);
            }
        }

        public static bool operator ==(State a, State b)
        {
            return ReferenceEquals(a, b) || (object)a != null && a.Equals(b);
        }

        public static bool operator !=(State a, State b)
        {
            return !(a == b);
        }
    }
}
