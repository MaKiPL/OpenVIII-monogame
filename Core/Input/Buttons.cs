namespace OpenVIII
{
    public enum Buttons
    {
        None, Up, Down, Left, Right, Okay, Cancel, Exit, Switch, Menu, Back, Start, X, Y, B, A, L1, L2, L3, R1, R2, R3, Select,
        LeftStickX, LeftStickY, RightStickX, RightStickY,
        MouseX, MouseY, MouseXjoy, MouseYjoy,
        MouseLeft, MouseMiddle, MouseRight, Mouse4, Mouse5, MouseWheelup, MouseWheeldown,
        Triangle = Y, Square = X, Circle = B, Cross = A
    }
    public enum GamePadButtons
    {
        None,
        // DPad Buttons
        Up, Down, Left, Right,
        // Buttons
        Back, Start, X, Y, B, A, LeftShoulder, RightShoulder, LeftStick, RightStick,
        // Thumbsticks
        ThumbSticks_Left, ThumbSticks_Right,
        // Triggers
        Triggers_Left, Triggers_Right,
        // PSX Alias
        L1 = LeftShoulder, L2= Triggers_Left, L3= LeftStick, R1= RightShoulder, R2= Triggers_Right, R3= RightStick,
        Select = Back, Triangle = Y, Square = X, Circle = B, Cross = A
    }
    public enum MouseButtons
    {
        None, MouseToStick,
        LeftButton, MiddleButton, RightButton, XButton1, XButton2, MouseWheelup, MouseWheeldown,
    }
}