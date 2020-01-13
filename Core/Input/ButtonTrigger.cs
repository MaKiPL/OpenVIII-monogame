using System;

namespace OpenVIII
{
    /// <summary>
    /// Triggers for actions
    /// </summary>
    [Flags]
    public enum ButtonTrigger : byte
    {
        /// <summary>
        /// Don't trigger
        /// </summary>
        None = 0x0,

        /// <summary>
        /// If was released now pressed
        /// </summary>
        OnPress = 0x1,

        /// <summary>
        /// If was pressed now released
        /// </summary>
        OnRelease = 0x2,

        /// <summary>
        /// If pressed, keeps triggering
        /// </summary>
        Press = 0x4,

        /// <summary>
        /// Trigger on value being out of the deadzone.
        /// </summary>
        Analog = 0x8,

        /// <summary>
        /// Don't check delay when triggering input.
        /// </summary>
        IgnoreDelay = 0x10,

        ///<summary>Require mouseover</summary>
        MouseOver = 0x20,

        /// <summary>
        /// For scrolling only
        /// </summary>
        Scrolling = 0x40,

        /// <summary>
        /// Force ignores default trigger.
        /// </summary>
        Force = 0x80
        
    }
}