using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FeatherSharp.ComponentModel
{
    /// <summary>
    /// Specifies feather actions that can be executed on code items.
    /// </summary>
    public enum FeatherAction
    {
        /// <summary>
        /// The feather action that injects invocations of the
        /// <see cref="System.ComponentModel.INotifyPropertyChanged.PropertyChanged" /> event.
        /// </summary>
        NotifyPropertyChanged,

        /// <summary>
        /// The feather action that injects invocations of augmented log methods.
        /// </summary>
        Log,
    }
}
